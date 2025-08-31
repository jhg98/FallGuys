using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    public static ButtonManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnClickModeSelectButton()
    {
        SceneManager.LoadScene("ModeSelect");
    }
    public void OnClickPracticeModeButton()
    {
        Debug.Log("연습 모드 설정");
        GameSettings.Instance.CurrentPlayMode = GameSettings.PlayMode.Practice;
        GameSettings.Instance.IsMapRandom = true;
        // 맵 랜덤 또는 선택 로직 추가 예정
        // 맵 선택 버튼 클릭 시 맵 리스트 활성화 or 맵 리스트에 랜덤까지 넣기
    }
    public void OnClickCompetitiveModeButton()
    {
        Debug.Log("경쟁 모드 설정");
        GameSettings.Instance.CurrentPlayMode = GameSettings.PlayMode.Competitive;
        GameSettings.Instance.IsMapRandom = true;
    }
    public void OnClickLobbyButton()
    {
        SceneManager.LoadScene("Lobby");
    }
    public void OnClickPlayButton()
    {
        SceneManager.LoadScene("Matching");
    }
}
