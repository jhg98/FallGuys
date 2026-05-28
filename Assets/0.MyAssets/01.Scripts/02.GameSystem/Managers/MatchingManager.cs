using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class MatchingManager : MonoBehaviourPunCallbacks
{
    public List<string> mapNameList = new List<string>();

    // 매칭 상태를 표시할 UI 텍스트
    [SerializeField] private TextMeshProUGUI statusText;

    [SerializeField] private float closeRoomDelay = 5f;

    // 포톤 서버 설정
    private string gameVersion = "1";
    private string region = "kr";

    // 룸 속성 키
    private const string ModeKey = "Mode";
    private const string MinPlayersKey = "MinPlayers";
    private const string IsRoomClosingKey = "IsRoomClosing";
    private const string RoundKey = "Round";
    private const string IsFinalKey = "IsFinal";
    private const string SelectedMapKey = "SelectedMap";

    // 플레이 모드
    private const string Practice = "Practice";
    private const string Competitive = "Competitive";

    private bool isRoomClosing = false;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }
    private void Start()
    {
        statusText.text = "마스터 서버에 연결 중...";

        Connect();
    }
    private void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("마스터 서버에 이미 연결되어 있습니다. 룸 입장 시도 중...");
            statusText.text = "마스터 서버에 이미 연결되어 있습니다. 룸 입장 시도 중...";

            JoinRandomRoom();
        }
        else
        {
            Debug.Log("마스터 서버에 연결 중...");
            statusText.text = "마스터 서버에 연결 중...";

            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"마스터 서버 연결이 끊어졌습니다: {cause}. 재연결 시도 중...");
        statusText.text = "마스터 서버 연결이 끊어졌습니다. 재연결 시도 중...";

        Connect();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("마스터 서버에 연결되었습니다. 룸 입장 시도 중...");
        statusText.text = "마스터 서버에 연결되었습니다. 룸 입장 시도 중...";

        JoinRandomRoom();
    }
    private void JoinRandomRoom()
    {
        if (PhotonNetwork.IsConnected)
        {
            Hashtable expectedProps = new Hashtable();

            if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
            {
                expectedProps[ModeKey] = Practice;
            }
            else
            {
                expectedProps[ModeKey] = Competitive;
            }

            PhotonNetwork.JoinRandomRoom(expectedProps, 0);
        }
        else
        {
            Debug.Log("마스터 서버에 연결되지 않았습니다. 재연결 시도 중...");
            statusText.text = "마스터 서버에 연결되지 않았습니다. 재연결 시도 중...";

            Connect();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("랜덤 룸 입장에 실패했습니다. 새로운 룸 생성 중...");
        statusText.text = "랜덤 룸 입장에 실패했습니다. 새로운 룸 생성 중...";

        CreateRoom();
    }
    private void CreateRoom()
    {
        RoomOptions options = new RoomOptions();
        Hashtable customProps = new Hashtable();

        // 연습모드일 경우
        if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
        {
            customProps[ModeKey] = Practice;
            customProps[MinPlayersKey] = 1;
            options.MaxPlayers = 1;
        }
        // 경쟁모드일 경우
        else
        {
            customProps[ModeKey] = Competitive;
            customProps[MinPlayersKey] = 2;
            options.MaxPlayers = 10;
        }
        customProps[RoundKey] = 1;
        customProps[IsFinalKey] = false;

        options.CustomRoomProperties = customProps;
        options.CustomRoomPropertiesForLobby = new string[] { ModeKey };

        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnJoinedRoom()
    {
        NotifyRoomInfo();
        TryRoomClose();
    }
    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        NotifyRoomInfo();
        TryRoomClose();
    }
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        NotifyRoomInfo();
    }
    private void NotifyRoomInfo()
    {
        if (PhotonNetwork.CurrentRoom == null)
        {
            Debug.LogWarning("현재 입장한 방이 없습니다.");
            return;
        }

        string roomName = PhotonNetwork.CurrentRoom.Name;
        string mode = (string)PhotonNetwork.CurrentRoom.CustomProperties[ModeKey];
        int minPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties[MinPlayersKey];
        int maxPlayers = PhotonNetwork.CurrentRoom.MaxPlayers;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        string message = $"현재 방 이름: {roomName}\n" +
                         $"모드: {mode}\n" +
                         $"최소 인원: {minPlayers}\n" +
                         $"최대 인원: {maxPlayers}\n" +
                         $"현재 인원: {playerCount}";

        Debug.Log(message);
        statusText.text = message;
    }
    private void TryRoomClose()
    {
        // 방 닫기는 마스터 클라이언트만 수행
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 방이 없거나, 닫혀있거나, 이미 닫는 중이면 무시
        if (PhotonNetwork.CurrentRoom == null || !PhotonNetwork.CurrentRoom.IsOpen || isRoomClosing)
            return;

        // 방 닫기 조건 검사
        if (!IsPossibleToCloseRoom())
            return;

        isRoomClosing = true;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { IsRoomClosingKey, isRoomClosing } });

        Invoke(nameof(CloseRoom), closeRoomDelay);
    }
    private void CloseRoom()
    {
        // 방 닫기는 마스터 클라이언트만 수행
        if (!PhotonNetwork.IsMasterClient)
            return;

        // 방 닫는 시점에도 검사 필요. 닫기 카운트 중에 상태가 변해서 조건을 충족하지 못할 수 있음
        if (!IsPossibleToCloseRoom())
        {
            isRoomClosing = false;
            PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { IsRoomClosingKey, isRoomClosing } });

            return;
        }

        PhotonNetwork.CurrentRoom.IsOpen = false;

        isRoomClosing = false;
        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { IsRoomClosingKey, isRoomClosing } });

        StartGame();
    }
    private bool IsPossibleToCloseRoom()
    {
        // 연습모드일 경우 조건 없이 닫기 가능
        if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
        {
            return true;
        }
        // 경쟁모드일 경우 최소 인원 도달 시 닫기 가능
        else if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Competitive)
        {
            int minPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties[MinPlayersKey];
            if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers)
                return true;
        }
        return false;
    }
    private void StartGame()
    {
        int minPlayers = (int)PhotonNetwork.CurrentRoom.CustomProperties[MinPlayersKey];

        if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers)
        {
            SelectMap();
        }
        else
        {
            Debug.LogWarning("플레이어 수가 부족하여 게임을 시작할 수 없습니다.");
            statusText.text = "플레이어 수가 부족하여 게임을 시작할 수 없습니다.";
            // 방이 닫힌 후 플레이어가 나가서 최소 인원이 미달되었을 때의 처리 로직 추가 예정
        }
    }
    private void SelectMap()
    {
        string mapName;

        // 연습모드일 경우 랜덤 또는 선택한 맵 지정
        if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
        {
            if (GameSettings.Instance.IsMapRandom)
            {
                mapName = GetRandomMap();
            }
            else
            {
                mapName = GameSettings.Instance.SelectedMap;
            }
        }
        // 경쟁모드일 경우 플레이어 수와 라운드 정보에 맞게 랜덤 지정
        else
        {
            mapName = GetRandomMap();
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(new Hashtable { { SelectedMapKey, mapName } });
    }
    private string GetRandomMap()
    {
        string selectedMap;

        // 우선은 완전 랜덤으로 구현
        int index = Random.Range(0, mapNameList.Count);
        selectedMap = mapNameList[index];

        // 추후 모드로 구분이 아닌, 라운드 정보로 구분 예정
        // All / First / Middle / Final

        // 연습모드일 경우 모든 맵을 랜덤으로 선택
        if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
        {
            // 추후 맵 자료구조 구현 예정
        }
        // 경쟁모드일 경우 플레이어 수와 라운드 정보에 맞게 랜덤 지정
        else if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Competitive)
        {
            // 추후 맵 자료구조 구현 예정
            // 각 맵 당 플레이 가능 인원과 라운드 정보를 가지게끔 구현 예정
            int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            int round = (int)PhotonNetwork.CurrentRoom.CustomProperties[RoundKey];
            bool isFinal = (bool)PhotonNetwork.CurrentRoom.CustomProperties[IsFinalKey];
        }

        return selectedMap;
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        // 방 상태가 변경되면 UI 업데이트
        if (propertiesThatChanged.ContainsKey(IsRoomClosingKey))
        {
            NotifyRoomClosing();
        }

        // 맵이 정해지면 로드
        if (propertiesThatChanged.ContainsKey(SelectedMapKey))
        {
            LoadMap();
        }
    }
    private void NotifyRoomClosing()
    {
        bool isRoomClosing = (bool)PhotonNetwork.CurrentRoom.CustomProperties[IsRoomClosingKey];

        if (isRoomClosing)
        {
            Debug.Log("방이 곧 닫힙니다.");
            statusText.text = "방이 곧 닫힙니다.";
            return;
        }
        else
        {
            // 방이 열린 상태일 때만 표시
            if (PhotonNetwork.CurrentRoom.IsOpen)
            {
                Debug.Log("방 닫기가 취소되었습니다.");
                statusText.text = "방 닫기가 취소되었습니다.";

                Invoke(nameof(NotifyRoomInfo), 1f);
                return;
            }
        }
    }
    private void LoadMap()
    {
        string selectedMap = (string)PhotonNetwork.CurrentRoom.CustomProperties[SelectedMapKey];

        if (string.IsNullOrEmpty(selectedMap))
        {
            Debug.LogError("선택된 맵이 없습니다. 맵을 로드할 수 없습니다.");
            statusText.text = "선택된 맵이 없습니다. 맵을 로드할 수 없습니다.";
            return;
        }

        Debug.Log($"{selectedMap} 맵이 선택되었습니다. 게임이 곧 시작됩니다.");
        statusText.text = $"{selectedMap} 맵이 선택되었습니다. 게임이 곧 시작됩니다.";

        Invoke(nameof(LoadLevel), 1f);
    }
    private void LoadLevel()
    {
        string selectedMap = (string)PhotonNetwork.CurrentRoom.CustomProperties[SelectedMapKey];

        PhotonNetwork.LoadLevel(selectedMap);
    }
}
