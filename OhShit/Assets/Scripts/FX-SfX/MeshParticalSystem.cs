using UnityEngine;

public class MeshParticalSystem : MonoBehaviour
{
    private const int MAX_QUADS = 15000;
    private Mesh mesh;

    [Header("UV Atlas")]
    [SerializeField] private int uvGridColumns = 1;
    [SerializeField] private int uvGridRows = 1;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private Color[] colors;

    private float[] fadingQuadLifetime;
    private float[] fadingQuadAge;
    private Color[] fadingQuadColor;

    private int quadIndex;

    private void Awake()
    {
        mesh = new Mesh();

        vertices = new Vector3[4 * MAX_QUADS];
        uv = new Vector2[4 * MAX_QUADS];
        triangles = new int[6 * MAX_QUADS];
        colors = new Color[4 * MAX_QUADS];

        fadingQuadLifetime = new float[MAX_QUADS];
        fadingQuadAge = new float[MAX_QUADS];
        fadingQuadColor = new Color[MAX_QUADS];

        GetComponent<MeshFilter>().mesh = mesh;

        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }

    public void UpdateFadingQuads()
    {
        bool changed = false;

        for (int i = 0; i < quadIndex; i++)
        {
            if (fadingQuadLifetime[i] <= 0f)
                continue;

            fadingQuadAge[i] += Time.deltaTime;

            if (fadingQuadAge[i] >= fadingQuadLifetime[i])
            {
                SetQuadAlpha(i, 0f);
                fadingQuadLifetime[i] = 0f;
                changed = true;
                continue;
            }

            float fade = 1f - fadingQuadAge[i] / fadingQuadLifetime[i];
            float alpha = fadingQuadColor[i].a * fade;

            SetQuadAlpha(i, alpha);
            changed = true;
        }

        if (changed)
        {
            mesh.colors = colors;
        }
    }

    public int AddQuad(Vector3 position, float rotation, Vector3 quadSize, Color color, int uvIndex = 0)
    {
        if (quadIndex >= MAX_QUADS)
        {
            Debug.LogWarning("Maximum number of quads reached.");
            return -1;
        }

        int spawnedIndex = quadIndex;

        fadingQuadLifetime[spawnedIndex] = 0f;
        fadingQuadAge[spawnedIndex] = 0f;
        fadingQuadColor[spawnedIndex] = color;

        UpdateQuad(spawnedIndex, position, rotation, quadSize, color, uvIndex);
        quadIndex++;

        return spawnedIndex;
    }

    public int AddQuad(Vector3 position, float rotation, Vector3 quadSize)
    {
        return AddQuad(position, rotation, quadSize, Color.white, 0);
    }

    public int AddFadingQuad(
        Vector3 position,
        float rotation,
        Vector3 quadSize,
        Color color,
        float lifetime)
    {
        if (quadIndex >= MAX_QUADS)
        {
            Debug.LogWarning("Maximum number of quads reached.");
            return -1;
        }

        int spawnedIndex = quadIndex;

        fadingQuadLifetime[spawnedIndex] = lifetime;
        fadingQuadAge[spawnedIndex] = 0f;
        fadingQuadColor[spawnedIndex] = color;

        UpdateQuad(
            spawnedIndex,
            position,
            rotation,
            quadSize,
            color
        );

        quadIndex++;

        return spawnedIndex;
    }

    public void UpdateQuad(int index, Vector3 position, float rotation, Vector3 quadSize, Color color, int uvIndex = 0)
    {
        int vIndex = index * 4;
        int vIndex0 = vIndex;
        int vIndex1 = vIndex + 1;
        int vIndex2 = vIndex + 2;
        int vIndex3 = vIndex + 3;

        Quaternion rot = Quaternion.Euler(0, 0, rotation);

        vertices[vIndex0] = position + rot * new Vector3(-quadSize.x, -quadSize.y, 0);
        vertices[vIndex1] = position + rot * new Vector3(-quadSize.x, quadSize.y, 0);
        vertices[vIndex2] = position + rot * new Vector3(quadSize.x, quadSize.y, 0);
        vertices[vIndex3] = position + rot * new Vector3(quadSize.x, -quadSize.y, 0);

        SetQuadUV(vIndex0, vIndex1, vIndex2, vIndex3, uvIndex);

        colors[vIndex0] = color;
        colors[vIndex1] = color;
        colors[vIndex2] = color;
        colors[vIndex3] = color;

        int tIndex = index * 6;

        triangles[tIndex] = vIndex0;
        triangles[tIndex + 1] = vIndex1;
        triangles[tIndex + 2] = vIndex2;

        triangles[tIndex + 3] = vIndex2;
        triangles[tIndex + 4] = vIndex3;
        triangles[tIndex + 5] = vIndex0;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10000f);
    }

    public void UpdateQuad(int index, Vector3 position, float rotation, Vector3 quadSize)
    {
        UpdateQuad(index, position, rotation, quadSize, Color.white, 0);
    }

    private void SetQuadAlpha(int index, float alpha)
    {
        int vIndex = index * 4;

        colors[vIndex].a = alpha;
        colors[vIndex + 1].a = alpha;
        colors[vIndex + 2].a = alpha;
        colors[vIndex + 3].a = alpha;
    }

    private void SetQuadUV(int v0, int v1, int v2, int v3, int uvIndex)
    {
        int columns = Mathf.Max(1, uvGridColumns);
        int rows = Mathf.Max(1, uvGridRows);
        int totalCells = columns * rows;

        uvIndex = ((uvIndex % totalCells) + totalCells) % totalCells;

        int col = uvIndex % columns;
        int row = uvIndex / columns;

        float cellW = 1f / columns;
        float cellH = 1f / rows;

        float minX = col * cellW;
        float minY = 1f - (row + 1) * cellH;
        float maxX = minX + cellW;
        float maxY = minY + cellH;

        uv[v0] = new Vector2(minX, minY);
        uv[v1] = new Vector2(minX, maxY);
        uv[v2] = new Vector2(maxX, maxY);
        uv[v3] = new Vector2(maxX, minY);
    }
}
