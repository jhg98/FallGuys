using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }
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

    public enum PlayMode
    {
        Practice,
        Competitive
    }

    public PlayMode CurrentPlayMode { get; set; } = PlayMode.Competitive;
    public bool IsMapRandom { get; set; } = true;
    public string SelectedMap { get; set; } = "Map1"; // 맵 랜덤 여부가 false일 때 사용
    public int CurrentRound { get; set; } = 1;
    public bool IsFinalRound { get; set; } = false;
}
