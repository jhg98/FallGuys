using UnityEngine;
using Photon.Pun;

// 자신을 제외한 다른 플레이어들에게 자신의 Transform을 동기화하는 스크립트
public class RemoteTransformSync : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public float lerpSpeed = 10f;

    void Update()
    {
        if (photonView.IsMine) return;

        transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * lerpSpeed);
    }

    // PhotonView가 소유한 객체의 상태를 네트워크로 전송하거나 수신하는 데 사용되는 메서드
    // IsWriting: 현재 객체가 데이터를 보내는지 여부
    // IsReading: 현재 객체가 데이터를 받는지 여부
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 자신의 Transform 정보를 네트워크로 전송
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // 다른 플레이어로부터 Transform 정보를 수신
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}