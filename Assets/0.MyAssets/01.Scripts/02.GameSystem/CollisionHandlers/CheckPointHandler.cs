using Photon.Pun;
using UnityEngine;

public class CheckPointHandler : MonoBehaviour
{
    public Transform checkPoint;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            var view = other.GetComponent<PhotonView>();
            if ((view != null && view.IsMine) || view == null)
            {
                var player = other.GetComponent<PlayerRespawner>();
                if (player != null)
                {
                    player.SetCheckpoint(checkPoint);
                }
            }
        }
    }
}
