using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        // 서버에 연결되어 있지 않은데 인스턴스가 존재한다면 파괴
        if (!PhotonNetwork.IsConnected && Instance != null)
        {
            Destroy(gameObject);
            Instance = null;
        }

        if (Instance == null)
        {
            // 서버에 연결되어 있을 때만 인스턴스 생성
            if (PhotonNetwork.IsConnected)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Text EndText;
    public Text ResultText;
    public Image TextBackgroundImage;

    public PlayerController LocalPlayer { get; private set; }
    public int RoundNumber { get; private set; } = 1;
    public List<int> SuccessPlayerNums { get; private set; } = new();
    public List<int> FailPlayerNums { get; private set; } = new();
    // 라운드별 결과 저장할 딕셔너리 추가 예정

    private TimerManager timerManager;
    private CountDownManager countDownManager;

    // 맵별 정보 저장 로직 추후 추가 예정
    private int successLimit = 1;

    private bool isGameEnd = false;

    private void Start()
    {
        timerManager = FindObjectOfType<TimerManager>();
        countDownManager = FindObjectOfType<CountDownManager>();
    }

    public void RegisterPlayer(PlayerController player)
    {
        LocalPlayer = player;
    }

    // 플레이어의 Spawned 속성이 변경될 때마다 시작 시도
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.ContainsKey("Spawned"))
        {
            if (!PhotonNetwork.IsMasterClient) return;
            TryStartRound();
        }
    }
    // 마스터 클라이언트가 변경되면 다시 시도
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        TryStartRound();
    }
    private void TryStartRound()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 모든 플레이어의 Spawned 속성이 true인지 확인
        bool allSpawned = PhotonNetwork.PlayerList.All(player =>
            player.CustomProperties.ContainsKey("Spawned") && (bool)player.CustomProperties["Spawned"]);

        // 모든 플레이어가 스폰되면 시작
        if (allSpawned)
        {
            StartRound();
        }
    }
    private void StartRound()
    {
        Debug.Log($"{RoundNumber}라운드 시작");

        StartCoroutine(countDownManager.CountDown());

    }
    public void StartPlay()
    {
        Instance.LocalPlayer.CanControlPlayer = true;
        Instance.LocalPlayer.CanControlCamera = true;

        timerManager.StartTimer();
    }

    // 본인만 호출
    public void OnPlayerFinished(int actorNum, PlayerController player)
    {
        // 조작 불가 처리, UI 처리는 본인만 진행
        player.CanControlPlayer = false;

        TextBackgroundImage.gameObject.SetActive(true);
        ResultText.gameObject.SetActive(true);
        //ResultText.text = "성공";
        ResultText.text = "SURVIVED";

        // 오브젝트 비활성화는 모든 클라이언트에게 동기화
        photonView.RPC(nameof(DisablePlayer), RpcTarget.All, player.photonView.ViewID);
        // 성공 인원은 모든 클라이언트에서 저장
        photonView.RPC(nameof(AddSuccessPlayer), RpcTarget.All, actorNum);
    }
    [PunRPC]
    private void DisablePlayer(int viewID)
    {
        // 코루틴으로 1초 뒤 비활성화
        GameObject playerObj = PhotonView.Find(viewID).gameObject;
        StartCoroutine(DisableObject(playerObj, 1f));
    }
    private IEnumerator DisableObject(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        obj.gameObject.SetActive(false);
    }
    [PunRPC]
    private void AddSuccessPlayer(int actorNum)
    {
        // 성공 인원이 제한 인원보다 아직 작을 때만 추가
        if (SuccessPlayerNums.Count < successLimit)
        {
            Debug.Log("성공 인원 추가");
            SuccessPlayerNums.Add(actorNum);
            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "SuccessCount", SuccessPlayerNums.Count } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(props);

            if (!PhotonNetwork.IsMasterClient) return;

            // 성공 인원이 다 차면 라운드 종료
            if (SuccessPlayerNums.Count >= successLimit)
            {
                EndRound();
            }
        }
    }

    public void EndRound()
    {
        Debug.Log($"{RoundNumber}라운드 종료");

        if (!PhotonNetwork.IsMasterClient) return;

        if (SuccessPlayerNums.Count > 1)
        {
            Debug.Log("다음 라운드 진행");
            isGameEnd = false;
            RoundNumber++;
        }
        else if (SuccessPlayerNums.Count == 1)
        {
            Debug.Log("우승자 나옴");
            isGameEnd = true;
        }
        else if (SuccessPlayerNums.Count == 0)
        {
            Debug.Log("성공자 없음");
            isGameEnd = true;
        }

        photonView.RPC(nameof(EndRoundRPC), RpcTarget.All, RoundNumber);
    }
    [PunRPC]
    private void EndRoundRPC(int roundNum)
    {
        StartCoroutine(EndPlay(roundNum));
    }
    private IEnumerator EndPlay(int roundNum)
    {
        Instance.LocalPlayer.CanControlPlayer = false;
        Instance.LocalPlayer.CanControlCamera = false;

        yield return new WaitForSecondsRealtime(2f);

        TextBackgroundImage.gameObject.SetActive(true);
        ResultText.gameObject.SetActive(false);
        EndText.gameObject.SetActive(true);
        //EndText.text = $"{roundNum}라운드 종료";
        EndText.text = "MATCH OVER";

        yield return new WaitForSecondsRealtime(2f);

        EndText.gameObject.SetActive(false);

        if (PhotonNetwork.IsMasterClient && isGameEnd)
        {
            photonView.RPC(nameof(EndGame), RpcTarget.All);
        } 
        else if (PhotonNetwork.IsMasterClient && !isGameEnd)
        {
            // 다음 라운드 진행 메서드 예정
        }
    }
    [PunRPC]
    private void EndGame()
    {
        Debug.Log("게임 종료");
        // 게임이 끝나면 방 나가기
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("방 나감");
        // 방 나간 후 서버 연결도 끊기
        PhotonNetwork.Disconnect();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("서버 연결 끊김: " + cause);

        // 연결 끊긴 후에 커서 복구 및 로비 이동, 게임매니저 파괴
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("Lobby");

        Destroy(gameObject);
    }

}
