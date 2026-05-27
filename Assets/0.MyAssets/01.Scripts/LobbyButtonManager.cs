using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyButtonManager : MonoBehaviour
{
    [SerializeField] private Button ModeSelectButton;
    [SerializeField] private Button PlayButton;

    private void Start()
    {
        ModeSelectButton.onClick.AddListener(OnClickModeSelectButton);
        PlayButton.onClick.AddListener(OnClickPlayButton);
    }
    public void OnClickModeSelectButton()
    {
        SceneManager.LoadScene("ModeSelect");
    }
    public void OnClickPlayButton()
    {
        SceneManager.LoadScene("Matching");
    }
}
