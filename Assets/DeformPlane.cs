using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
public class DeformPlane : MonoBehaviour
{
    public Transform cornerA;
    public Transform cornerB;
    public Transform cornerC;
    public Transform cornerD;

    public float influenceRadius = 4f;

    Mesh mesh;
    MeshCollider meshCollider;

    Vector3[] baseVertices;
    Vector3[] deformedVertices;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        meshCollider = GetComponent<MeshCollider>();

        baseVertices = mesh.vertices;
        deformedVertices = new Vector3[baseVertices.Length];
    }

    void LateUpdate()
    {
        Deform();
    }

    void Deform()
    {
        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(baseVertices[i]);

            float height = 0f;
            height += CornerInfluence(worldPos, cornerA);
            height += CornerInfluence(worldPos, cornerB);
            height += CornerInfluence(worldPos, cornerC);
            height += CornerInfluence(worldPos, cornerD);

            deformedVertices[i] = new Vector3(
                baseVertices[i].x,
                height,
                baseVertices[i].z
            );
        }

        mesh.vertices = deformedVertices;
        mesh.RecalculateNormals();

        // Update collider safely
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    float CornerInfluence(Vector3 vertexWorld, Transform corner)
    {
        if (!corner) return 0f;

        Vector3 flatV = new Vector3(vertexWorld.x, 0, vertexWorld.z);
        Vector3 flatC = new Vector3(corner.position.x, 0, corner.position.z);

        float dist = Vector3.Distance(flatV, flatC);
        float t = Mathf.Clamp01(1f - dist / influenceRadius);

        return corner.position.y * t;
    }
}
