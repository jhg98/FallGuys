using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ModeSelectButtonManager : MonoBehaviour
{
    [SerializeField] private Button PracticeModeButton;
    [SerializeField] private Button CompetitiveModeButton;
    [SerializeField] private Button LobbyButton;

    void Start()
    {
        PracticeModeButton.onClick.AddListener(OnClickPracticeModeButton);
        CompetitiveModeButton.onClick.AddListener(OnClickCompetitiveModeButton);
        LobbyButton.onClick.AddListener(OnClickLobbyButton);

        if(GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Practice)
        {
            PracticeModeButton.interactable = false;
            CompetitiveModeButton.interactable = true;
        }
        else if (GameSettings.Instance.CurrentPlayMode == GameSettings.PlayMode.Competitive)
        {
            PracticeModeButton.interactable = true;
            CompetitiveModeButton.interactable = false;
        }
    }
    public void OnClickPracticeModeButton()
    {
        Debug.Log("연습 모드 설정");

        PracticeModeButton.interactable = false;
        CompetitiveModeButton.interactable = true;

        GameSettings.Instance.CurrentPlayMode = GameSettings.PlayMode.Practice;
        GameSettings.Instance.IsMapRandom = true;

        // 맵 랜덤 또는 선택 로직 추가 예정
        // 맵 선택 버튼 클릭 시 맵 리스트 활성화 or 맵 리스트에 랜덤까지 넣기
    }
    public void OnClickCompetitiveModeButton()
    {
        Debug.Log("경쟁 모드 설정");

        PracticeModeButton.interactable = true;
        CompetitiveModeButton.interactable = false;

        GameSettings.Instance.CurrentPlayMode = GameSettings.PlayMode.Competitive;
        GameSettings.Instance.IsMapRandom = true;
    }
    public void OnClickLobbyButton()
    {
        SceneManager.LoadScene("Lobby");
    }
}
