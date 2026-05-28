using Photon.Pun;
using UnityEngine;

public class PlayerRespawner : MonoBehaviourPun
{
    public Transform lastCheckpoint;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    private void Start()
    {
        if (lastCheckpoint == null)
        {
            // 처음엔 시작포인트로 설정
            lastCheckpoint = GameObject.FindWithTag("StartPoint").transform;
        }
    }
    public void SetCheckpoint(Transform checkpoint)
    {
        lastCheckpoint = checkpoint;
    }

    public void Respawn()
    {
        if (photonView != null && !photonView.IsMine) return;

        Vector3 respawnPos = lastCheckpoint.position;
        Quaternion respawnRot = lastCheckpoint.rotation;

        if(rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = respawnPos;
        transform.rotation = respawnRot;

        if (photonView != null)
        {
            photonView.RPC("UpdatePosition", RpcTarget.All, respawnPos, respawnRot);
        }
    }
    [PunRPC]
    public void UpdatePosition(Vector3 position, Quaternion rotation)
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = position;
        transform.rotation = rotation;
    }
}
