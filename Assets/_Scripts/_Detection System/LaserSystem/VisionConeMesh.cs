using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VisionConeMesh : MonoBehaviour
{
    public float viewAngle = 25f;
    public float viewDistance = 10f;
    public int segments = 20;

    [Header("Height Visualization")]
    public float heightOffset = 0.8f;
    public float thickness = 0.4f;

    private Mesh mesh;

    void Awake()
    {
        mesh = new Mesh();
        mesh.name = "Vision Cone Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
    }

    void LateUpdate()
    {
        DrawConeBand();
    }

    void DrawConeBand()
    {
        if (mesh == null) return;

        int vertexCount = (segments + 1) * 2 + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[segments * 12];

        float halfAngle = viewAngle * 0.5f;
        float angleStep = viewAngle / segments;

        float yMin = heightOffset - thickness * 0.5f;
        float yMax = heightOffset + thickness * 0.5f;

        // 시작점 2개
        vertices[0] = new Vector3(0f, yMin, 0f);
        vertices[1] = new Vector3(0f, yMax, 0f);

        // 바깥 호 점들
        for (int i = 0; i <= segments; i++)
        {
            float angle = -halfAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 dir = new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * viewDistance;

            int index = 2 + i * 2;
            vertices[index] = new Vector3(dir.x, yMin, dir.z);
            vertices[index + 1] = new Vector3(dir.x, yMax, dir.z);
        }

        int triIndex = 0;

        for (int i = 0; i < segments; i++)
        {
            int current = 2 + i * 2;
            int next = current + 2;

            // 아래 면
            triangles[triIndex++] = 0;
            triangles[triIndex++] = current;
            triangles[triIndex++] = next;

            // 위 면
            triangles[triIndex++] = 1;
            triangles[triIndex++] = next + 1;
            triangles[triIndex++] = current + 1;

            // 바깥 띠 면 1
            triangles[triIndex++] = current;
            triangles[triIndex++] = current + 1;
            triangles[triIndex++] = next;

            // 바깥 띠 면 2
            triangles[triIndex++] = next;
            triangles[triIndex++] = current + 1;
            triangles[triIndex++] = next + 1;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    public void SetCone(float angle, float distance, float scanHeight, float scanThickness)
    {
        viewAngle = angle;
        viewDistance = distance;
        heightOffset = scanHeight;
        thickness = scanThickness;
    }
}