using Photon.Pun;
using UnityEngine;

public class FallTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var view = other.GetComponent<PhotonView>();
            if ((view != null && view.IsMine) || view == null)
            {
                var player = other.GetComponent<PlayerRespawner>();
                if (player != null)
                {
                    player.Respawn();

                    // 생존 모드일 경우 탈락 구현 예정
                }
            }
        }
    }
}
