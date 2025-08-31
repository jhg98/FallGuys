using Cinemachine;
using Photon.Pun;

public class CameraSetUp : MonoBehaviourPun
{
    void Start()
    {
        if (photonView.IsMine)
        {
            CinemachineVirtualCamera followCam = FindObjectOfType<CinemachineVirtualCamera>();

            followCam.Follow = transform.Find("PlayerCameraRoot");
        }
    }
}
