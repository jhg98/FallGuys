using Photon.Pun;
using UnityEngine;

public class MovableObject : MonoBehaviour
{
    public enum Axis { X, Y, Z }
    public Axis moveAxis = Axis.X;

    public float distance = 5f;
    public float speed = 3f;
    public float offset = 0f; // 처음 시작 위치만 다르게 주고 싶을 때 사용
    public float waitTime = 1.5f; // 양 끝에서 멈춰있는 시간

    private bool isForward = true;
    private Vector3 startPos;

    private float delayTimer = 0f;
    private bool isWaiting = false;

    void Awake()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        startPos = transform.position;

        switch (moveAxis)
        {
            case Axis.X:
                transform.position += Vector3.right * offset;
                break;
            case Axis.Y:
                transform.position += Vector3.up * offset;
                break;
            case Axis.Z:
                transform.position += Vector3.forward * offset;
                break;
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        if (isWaiting)
        {
            delayTimer += Time.deltaTime;
            if (delayTimer >= waitTime)
            {
                isWaiting = false;
                delayTimer = 0f;
                isForward = !isForward; // 방향 반전
            }
            return;
        }

        Vector3 dir = Vector3.zero;
        float pos = 0f;
        float start = 0f;

        switch (moveAxis)
        {
            case Axis.X:
                dir = Vector3.right;
                pos = transform.position.x;
                start = startPos.x;
                break;
            case Axis.Y:
                dir = Vector3.up;
                pos = transform.position.y;
                start = startPos.y;
                break;
            case Axis.Z:
                dir = Vector3.forward;
                pos = transform.position.z;
                start = startPos.z;
                break;
        }

        if (isForward)
        {
            if (pos < start + distance)
            {
                transform.position += dir * speed * Time.deltaTime;
            }
            else
            {
                transform.position = startPos + dir * distance; // 위치 정확하게 보정
                isWaiting = true;
            }
        }
        else
        {
            if (pos > start)
            {
                transform.position -= dir * speed * Time.deltaTime;
            }
            else
            {
                transform.position = startPos;
                isWaiting = true;
            }
        }
    }
}