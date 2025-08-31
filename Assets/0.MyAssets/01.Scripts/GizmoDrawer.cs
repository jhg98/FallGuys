using Unity.VisualScripting;
using UnityEngine;

public class GizmoDrawer : MonoBehaviour
{
    public Color startPointColor = Color.yellow;
    public Color checkPointColor = Color.blue;
    public float gizmoRadius = 0.4f;

    public Color colliderColor = Color.green;

    private void OnDrawGizmos()
    {
        DrawPoints("StartPoint", startPointColor);
        DrawPoints("CheckPoint", checkPointColor);

        DrawBoxColliders();
        DrawCapsuleColliders();
        DrawSphereColliders();
    }

    private void DrawPoints(string tag, Color color)
    {
        GameObject[] roots = GameObject.FindGameObjectsWithTag(tag);
        foreach (var root in roots)
        {
            if (root == null) continue;
            foreach (Transform child in root.transform)
            {
                Gizmos.color = color;
                Gizmos.DrawSphere(child.position, gizmoRadius);
            }
        }
    }
    private void DrawBoxColliders()
    {
        Gizmos.color = colliderColor;
        foreach (BoxCollider box in FindObjectsOfType<BoxCollider>())
        {
            if (box.enabled)
            {
                Gizmos.matrix = box.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, box.size);
            }
        }
    }
    private void DrawCapsuleColliders()
    {
        Gizmos.color = colliderColor;
        foreach (CapsuleCollider capsule in FindObjectsOfType<CapsuleCollider>())
        {
            if (capsule.enabled)
            {
                Gizmos.matrix = capsule.transform.localToWorldMatrix;
                Vector3 size = Vector3.one;
                size[capsule.direction] = capsule.height;
                size[(capsule.direction + 1) % 3] = capsule.radius * 2;
                size[(capsule.direction + 2) % 3] = capsule.radius * 2;
                Gizmos.DrawWireCube(capsule.center, size);
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void DrawSphereColliders()
    {
        Gizmos.color = colliderColor;
        foreach (SphereCollider sphere in FindObjectsOfType<SphereCollider>())
        {
            if (sphere.enabled)
            {
                Gizmos.matrix = sphere.transform.localToWorldMatrix;
                Gizmos.DrawWireSphere(sphere.center, sphere.radius);
            }
        }
        Gizmos.matrix = Matrix4x4.identity;
    }
}