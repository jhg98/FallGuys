using Photon.Pun;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float speed = 1.5f;
	public float limit = 75f; // 최고 각도

	private Vector3 initialRotation;

    private Rigidbody rb;

	void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        initialRotation = transform.localEulerAngles;
    }

    void FixedUpdate()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        float angle = limit * Mathf.Sin(Time.time * speed);

        Quaternion targetRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, angle);
        
        rb.MoveRotation(targetRotation);
    }
}
