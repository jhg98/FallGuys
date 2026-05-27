using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class CountDownManager : MonoBehaviourPun
{
    public Text CountDownText;
    public int countDownTime = 3;

    // 마스터 클라이언트에서 카운트다운 후 모든 클라이언트에 동기화
    public IEnumerator CountDown()
    {
        yield return new WaitForSeconds(0.5f);

        photonView.RPC(nameof(SetCountDownTextActive), RpcTarget.All, true);

        int remaining = countDownTime;
        while (remaining > 0)
        {
            photonView.RPC(nameof(UpdateCountDownText), RpcTarget.All, remaining);
            yield return new WaitForSeconds(1f);
            remaining--;
        }
        photonView.RPC(nameof(UpdateCountDownText), RpcTarget.All, remaining); // Start!

        // 0.5초 대기 후 텍스트 비활성화 및 게임 시작
        yield return new WaitForSeconds(0.5f);
        photonView.RPC(nameof(SetCountDownTextActive), RpcTarget.All, false);
        photonView.RPC(nameof(OnCountdownEnd), RpcTarget.All);
    }
    [PunRPC]
    private void SetCountDownTextActive(bool isActive)
    {
        CountDownText.gameObject.SetActive(isActive);
    }
    [PunRPC]
    private void UpdateCountDownText(int time)
    {
        if (time > 0)
        {
            CountDownText.text = time.ToString();
        }
        else if (time == 0)
        {
            CountDownText.text = "Go!";
        }
    }
    [PunRPC]
    public void OnCountdownEnd()
    {
        GameManager.Instance.StartPlay();
    }
}
