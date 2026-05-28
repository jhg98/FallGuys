using Photon.Pun;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis moveAxis = Axis.X;

    public float distance = 5f;
    public float speed = 3f;
    public float offset = 0f; // 처음 위치만 다르게 주고 싶을 때 사용(시작점과 끝점의 중간에서 시작하고 싶다던가)
    public float waitTime = 1.5f; // 양 끝에서 멈춰있는 시간

    private bool isForward = true;

    private Vector3 startPos;
    private Vector3 endPos;

    private float delayTimer = 0f;
    private bool isWaiting = false;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        Vector3 axisDir = GetAxisDirection();

        startPos = transform.position;
        endPos = startPos + axisDir * distance;

        transform.position += axisDir * offset; // 초기 위치에 오프셋 적용
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        if (isWaiting)
        {
            delayTimer += Time.fixedDeltaTime;
            if (delayTimer >= waitTime)
            {
                isWaiting = false;
                delayTimer = 0f;
                isForward = !isForward; // 방향 반전
            }
            return;
        }

        Vector3 targetPos = isForward ? endPos : startPos;
        Vector3 newPos = Vector3.MoveTowards(rb.position, targetPos, speed * Time.fixedDeltaTime);
        
        rb.MovePosition(newPos);

        if (Vector3.Distance(rb.position, targetPos) < 0.01f)
        {
            rb.MovePosition(targetPos); // 보정

            isWaiting = true;
        }
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