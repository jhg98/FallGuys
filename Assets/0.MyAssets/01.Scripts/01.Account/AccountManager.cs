using System.Collections;
using Firebase.Auth;
using UnityEngine;

// 로그인 및 계정 생성을 담당하는 클래스
public class AccountManager : MonoBehaviour
{
    private FirebaseAuthManager authManager;
    private FirebaseDBManager dbManager;

    void Start()
    {
        if(UserDataManager.Instance.CurrentUserData != null)
        {
            return;
        }

        authManager = new FirebaseAuthManager();
        dbManager = new FirebaseDBManager();

        Login();
    }
    private void Update()
    {
        // 테스트용: 로그아웃 버튼 클릭 시 로그아웃 함수 호출
        if (Input.GetKeyDown(KeyCode.L)) // 'L' 키를 눌러 로그아웃 테스트
        {
            TestLogout();
        }
    }
    // 로그아웃 테스트용
    private void TestLogout()
    {
        Logout();
    }

    // 로그인
    /// <summary>
    /// 로그인 메서드!!!
    /// 유저 로그인 처리를 해주는 메서드입니다~~
    /// </summary>
    private void Login()
    {
        FirebaseUser user = authManager.GetCurrentUser();

        if (user != null)
        {
            Debug.Log("기존 유저 로그인: " + user.UserId);
            StartCoroutine(LoadUserData());
        }
        else
        {
            Debug.Log("로그인 정보가 없습니다. 익명 로그인 시도 중...");
            authManager.SignInAnonymously();
            StartCoroutine(LoadUserData());
        }
    }
    // 로그아웃(익명로그인이므로 탈퇴와 동일)
    private void Logout()
    {
        authManager.SignOut();
        Debug.Log("사용자가 로그아웃되었습니다.");
        // 로그아웃 후 필요한 추가 작업(예: UI 업데이트 등)
    }
    // 유저 데이터 로드
    private IEnumerator LoadUserData(int retryCount = 0, int maxRetry = 3)
    {
        yield return new WaitUntil(() => authManager.GetCurrentUser() != null);
        FirebaseUser user = authManager.GetCurrentUser();

        UserData userData = new UserData(user.UserId);
        var task = userData.GetUserData(dbManager);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError("유저 데이터 로드 중 예외 발생: " + task.Exception);
            if (retryCount < maxRetry)
            {
                Debug.LogWarning($"재시도 {retryCount + 1}/{maxRetry}");
                yield return new WaitForSeconds(1f); // 잠시 대기 후 재시도
                yield return StartCoroutine(LoadUserData(retryCount + 1, maxRetry));
            }
            else
            {
                Debug.LogError("최대 재시도 횟수 초과. 로드 실패.");
                // 실패 처리(예: UI 알림)
            }
            yield break;
        }

        userData = task.Result;
        if (userData != null)
        {
            Debug.Log("유저 데이터 로드 완료: " + userData.uid);
            UserDataManager.Instance.SetUserData(userData);
        }
        else
        {
            if (retryCount < maxRetry)
            {
                Debug.Log("유저 데이터가 존재하지 않습니다. 새로 생성 후 재시도...");
                yield return StartCoroutine(SaveNewUserData());
                yield return StartCoroutine(LoadUserData(retryCount + 1, maxRetry));
            }
            else
            {
                Debug.LogError("최대 재시도 횟수 초과. 데이터 생성 및 로드 실패.");
                // 실패 처리(예: UI 알림)
            }
        }
    }
    // 신규 데이터 저장
    private IEnumerator SaveNewUserData()
    {
        // 로그인될 때까지 대기
        yield return new WaitUntil(() => authManager.GetCurrentUser() != null);
        FirebaseUser user = authManager.GetCurrentUser();

        UserData userData = InitializeUserData(user.UserId);
        userData.SaveUserData(dbManager);
        Debug.Log("신규 데이터 저장 완료: " + userData.uid);
    }
    // 유저 초기 데이터
    private UserData InitializeUserData(string userId)
    {
        string nickname = GenerateRandomNickname();
        Appearance appearance = new Appearance { color = "pink", pattern = "none" };
        Record record = new Record { wins = 0, playTime = 0f };

        UserData userData = new UserData(userId, nickname, appearance, record);
        return userData;
    }
    // 닉네임 랜덤 생성
    private string GenerateRandomNickname()
    {
        int randomNumber = Random.Range(1000, 9999);
        return "User_" + randomNumber.ToString();
    }
}
