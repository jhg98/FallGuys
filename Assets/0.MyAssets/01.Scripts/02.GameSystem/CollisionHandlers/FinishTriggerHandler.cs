using Photon.Pun;
using UnityEngine;

public class FinishTriggerHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("골인");

            var view = other.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                //var player = other.GetComponent<ThirdPersonController>();
                var player = other.GetComponent<PlayerController>();
                if (player != null)
                {
                    GameManager.Instance.OnPlayerFinished(view.Owner.ActorNumber, player);
                }
            }
        }
    }
}
