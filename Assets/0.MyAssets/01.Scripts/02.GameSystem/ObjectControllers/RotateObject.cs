using Photon.Pun;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed = 3f;

    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Z;

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        Vector3 axisDir = Vector3.zero;

        switch (rotationAxis)
        {
            case Axis.X:
                axisDir = Vector3.right;
                break;
            case Axis.Y:
                axisDir = Vector3.up;
                break;
            case Axis.Z:
                axisDir = Vector3.forward;
                break;
        }

        transform.Rotate(axisDir, speed * Time.deltaTime / 0.01f, Space.Self);
    }
}