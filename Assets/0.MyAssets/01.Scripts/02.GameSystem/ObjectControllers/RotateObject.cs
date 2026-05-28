using Photon.Pun;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public float speed = 180f;

    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Z;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

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

        Quaternion deltaRotation = Quaternion.AngleAxis(speed * Time.fixedDeltaTime, axisDir);

        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}