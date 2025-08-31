using Photon.Pun;
using UnityEngine;

public class PlayerRespawner : MonoBehaviourPun
{
    public Transform lastCheckpoint;

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

        // 위치 변경 시 캐릭터 컨트롤러 비활성화 필요
        var cc = GetComponent<CharacterController>();

        if (cc != null)
        {
            cc.enabled = false;
            transform.position = respawnPos;
            transform.rotation = respawnRot;
            cc.enabled = true;
        }
        else
        {
            transform.position = respawnPos;
            transform.rotation = respawnRot;
        }

        if (photonView != null)
        {
            photonView.RPC("UpdatePosition", RpcTarget.All, respawnPos, respawnRot);
        }
    }
    [PunRPC]
    public void UpdatePosition(Vector3 position, Quaternion rotation)
    {
        transform.position = position;
        transform.rotation = rotation;
    }
}
