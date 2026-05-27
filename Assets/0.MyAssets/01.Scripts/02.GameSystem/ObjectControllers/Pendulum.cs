using Photon.Pun;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public float speed = 1.5f;
	public float limit = 75f; // 최고 각도

	private Vector3 initialRotation;

	void Awake()
    {
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient) return;

        initialRotation = transform.localEulerAngles;
    }

    void Update()
    {
		float angle = limit * Mathf.Sin(Time.time * speed);
		transform.localRotation = Quaternion.Euler(initialRotation.x, initialRotation.y, angle);
	}
}
