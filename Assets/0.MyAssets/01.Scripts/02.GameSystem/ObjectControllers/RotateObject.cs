using Photon.Pun;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis rotationAxis = Axis.Z;

    public float speed = 180f;

    private Quaternion initialRotation;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        initialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        Vector3 axisDir = GetAxisDirection();

        // PhotonNetwork.Time 값은 매우 커질 수 있어서 처음부터 float로 변환하면 정밀도 떨어지므로
        // double로 받은 후 주기로 나눈 나머지를 float로 변환하여 사용
        double time = PhotonNetwork.IsConnected ? PhotonNetwork.Time : Time.time;
        time = time % (360f / speed);
        
        float angle = (float)time * speed;
        
        Quaternion targetRotation = initialRotation * Quaternion.AngleAxis(angle, axisDir);

        rb.MoveRotation(targetRotation);
    }
    private Vector3 GetAxisDirection()
    {
        switch (rotationAxis)
        {
            case Axis.X:
                return Vector3.right;
            case Axis.Y:
                return Vector3.up;
            case Axis.Z:
                return Vector3.forward;
            default:
                return Vector3.zero;
        }
    }
}