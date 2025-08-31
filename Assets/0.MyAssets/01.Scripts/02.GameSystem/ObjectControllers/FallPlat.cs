using System.Collections;
using Photon.Pun;
using UnityEngine;

public class FallPlat : MonoBehaviourPun
{
	public float fallTime = 0.5f;

	void OnCollisionEnter(Collision collision)
	{
		foreach (ContactPoint contact in collision.contacts)
		{
			if (collision.gameObject.CompareTag("Player"))
			{
                if (!PhotonNetwork.IsMasterClient) return;

                StartCoroutine(Fall(fallTime));
			}
		}
	}

	IEnumerator Fall(float time)
	{
		yield return new WaitForSeconds(time);
        photonView.RPC(nameof(DestroyObject), RpcTarget.All, true);
    }
    [PunRPC]
    void DestroyObject()
    {
        Destroy(gameObject);
    }

}
