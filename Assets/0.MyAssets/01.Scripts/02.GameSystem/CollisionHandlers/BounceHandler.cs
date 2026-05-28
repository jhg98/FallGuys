using Photon.Pun;
using UnityEngine;

public class BounceHandler : MonoBehaviour
{
    public enum BounceMode
    {
        ImpactDirection, // 충돌 반대방향
        ReflectDirection // 거울처럼 반사
    }
    [SerializeField] private BounceMode bounceMode = BounceMode.ImpactDirection;

    // 테스트 후 적절한 값으로 설정 필요
    [SerializeField] private float maxForce = 30f;
    [SerializeField] private float maxVelocity = 20f;
    [SerializeField] private float maxStunTime = 3f;

    private float stunTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var view = collision.gameObject.GetComponent<PhotonView>();
            if ((view != null && view.IsMine) || view == null)
            {
                Debug.Log($"충돌 속도 크기: {collision.relativeVelocity.magnitude}");

                Vector3 bounceDir;
                if (collision.relativeVelocity.magnitude > 0.01f)
                {
                    if (bounceMode == BounceMode.ReflectDirection && collision.contactCount > 0)
                    {
                        Vector3 normal = collision.contacts[0].normal;
                        // 거울처럼 반사된 방향으로 튕김
                        bounceDir = Vector3.Reflect(collision.relativeVelocity.normalized, normal);
                    }
                    else
                    {
                        // 부딪힌 반대 방향으로 튕김
                        bounceDir = -collision.relativeVelocity.normalized;
                    }

                }
                // 장애물에 리지드바디가 없거나 키네마틱이면 장애물의 속도는 고려 안됨.
                // 즉 플레이어가 가만히 있을 때는 충돌 속도가 0에 가까움. 이때는 충돌면 법선 벡터 사용.
                else if (collision.contactCount > 0)
                {
                    bounceDir = -collision.contacts[0].normal;
                }
                else
                {
                    bounceDir = Vector3.up;
                }
                Debug.Log($"방향: {bounceDir}");

                // 0~maxVelocity 사이에서 충돌 속도 크기가 어디 위치해있는지
                float impact = Mathf.InverseLerp(0, maxVelocity, collision.relativeVelocity.magnitude);
                // 값이 너무 작으면 강제 설정
                if (impact < 0.1f)
                {
                    impact = 0.1f;
                }

                // 최대 힘 * 속도 비율에 따라 힘 적용
                float finalForce = maxForce * impact;
                Debug.Log($"최종 힘: {finalForce}");

                // 힘에 따라 스턴 시간 적용. 0.3, maxStunTime 사이에서 impact 위치의 값.
                stunTime = Mathf.Lerp(0.3f, maxStunTime, impact);
                Debug.Log($"스턴 시간: {stunTime}");

                Vector3 forceVector = bounceDir * finalForce;

                collision.gameObject.GetComponent<PlayerController>().Hit(forceVector, stunTime);
            }
        }
    }
}