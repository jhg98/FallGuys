using Photon.Pun;
using UnityEngine;

public class PlayerPointer : MonoBehaviourPun
{
    [SerializeField] private GameObject PlayerPointerImage;

    void Start()
    {
        if (photonView.IsMine)
        {
            PlayerPointerImage.SetActive(true);
        }
        else
        {
            PlayerPointerImage.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (photonView.IsMine)
        {
            PlayerPointerImage.transform.forward = Camera.main.transform.forward;
        }
    }
}
