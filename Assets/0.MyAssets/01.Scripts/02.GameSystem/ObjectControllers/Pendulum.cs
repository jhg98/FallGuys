using Photon.Pun;
using System;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float speed = 1.5f;
	public float limit = 75f; // 최고 각도

	private Quaternion initialRotation;

    private Rigidbody rb;

	void Awake()
    {
        rb = GetComponent<Rigidbody>();

        initialRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        // PhotonNetwork.Time 값은 매우 커질 수 있어서 처음부터 float로 변환하면 정밀도 떨어지므로
        // double로 받은 후 주기로 나눈 나머지를 float로 변환하여 사용
        double time = PhotonNetwork.IsConnected ? PhotonNetwork.Time : Time.time;
        double period = 2 * Math.PI / speed; // 진동 주기
        time = time % period;

        float angle = limit * Mathf.Sin((float)time * speed);

        Quaternion targetRotation = initialRotation * Quaternion.AngleAxis(angle, Vector3.forward);

        rb.MoveRotation(targetRotation);
    }
}
