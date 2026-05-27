using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class TimerManager : MonoBehaviourPun
{
    public Text TimerText;
    public float timeLimit = 30f;

    private double startTime;
    private float duration;
    private bool isRunning = false;

    void Update()
    {
        if (!isRunning) return;

        float remaining = Mathf.Clamp(duration - (float)(PhotonNetwork.Time - startTime), 0, duration);

        if (remaining > 0)
        {
            TimerText.text = remaining.ToString("F2");
        }
        else
        {
            TimerText.gameObject.SetActive(false);
            GameManager.Instance.EndRound();
        }
    }

    public void StartTimer()
    {
        startTime = PhotonNetwork.Time;
        duration = timeLimit;
        isRunning = true;

        TimerText.gameObject.SetActive(true);
        TimerText.text = startTime.ToString();
    }
    public void StopTimer()
    {
        isRunning = false;

        TimerText.gameObject.SetActive(false);
    }
}
