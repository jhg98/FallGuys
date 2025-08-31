using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

// Firebase 인증 처리 클래스
public class FirebaseAuthManager
{
    private FirebaseAuth auth;

    public FirebaseAuthManager()
    {
        auth = FirebaseAuth.DefaultInstance;
    }

    // 현재 로그인된 사용자 정보 가져오기
    public FirebaseUser GetCurrentUser()
    {
        return auth.CurrentUser;
    }
    // 익명 로그인
    public void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("익명 로그인이 취소되었습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogErrorFormat("익명 로그인 중 오류 발생: {0}", task.Exception);
                return;
            }
            AuthResult result = task.Result;
            Debug.LogFormat("익명 로그인 성공: {0}", result.User.UserId);
        });
    }
    // 로그아웃
    public void SignOut()
    {
        FirebaseUser user = GetCurrentUser();
        string userId = user != null ? user.UserId : "알 수 없는 사용자";

        auth.SignOut();
        Debug.LogFormat("{0}이(가) 로그아웃되었습니다.", userId);
    }
}
