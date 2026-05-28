using Photon.Pun;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis moveAxis = Axis.X;

    public float distance = 5f;
    public float speed = 3f;
    public float waitTime = 1.5f; // 양 끝에서 멈춰있는 시간
    public float offset = 0f; // 처음 위치만 다르게 주고 싶을 때 사용(시작점과 끝점의 중간에서 시작하고 싶다던가)

    private Vector3 startPos;
    private Vector3 endPos;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        Vector3 axisDir = GetAxisDirection();

        startPos = transform.position;
        endPos = startPos + axisDir * distance;

        transform.position += axisDir * offset; // 초기 위치에 오프셋 적용
    }

    void FixedUpdate()
    {
        // PhotonNetwork.Time 값은 매우 커질 수 있어서 처음부터 float로 변환하면 정밀도 떨어지므로
        // double로 받은 후 주기로 나눈 나머지를 float로 변환하여 사용
        double time = PhotonNetwork.IsConnected ? PhotonNetwork.Time : Time.time;
        float moveTime = distance / speed;
        float cycleTime = (moveTime + waitTime) * 2;
        float currentTime = (float)(time % cycleTime);

        Vector3 targetPos;

        // 정방향
        if(currentTime < moveTime)
        {
            float t = currentTime / moveTime;
            targetPos = Vector3.Lerp(startPos, endPos, t);
        }
        // 대기
        else if(currentTime < moveTime + waitTime)
        {
            targetPos = endPos;
        }
        // 역방향
        else if(currentTime < moveTime + waitTime + moveTime)
        {
            float t = (currentTime - moveTime - waitTime) / moveTime;
            targetPos = Vector3.Lerp(endPos, startPos, t);
        }
        // 대기
        else
        {
            targetPos = startPos;
        }

        rb.MovePosition(targetPos);
    }

    private Vector3 GetAxisDirection()
    {
        switch (moveAxis)
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