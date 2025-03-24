using UnityEngine;

public class InfiniteGrid : MonoBehaviour
{
    public float cellSize = 1f; // Size of each cell in the grid
    public Color gridColor = Color.gray; // Color of the grid lines
    private Material lineMaterial;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;

        // Create a simple material for drawing the grid lines
        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
        lineMaterial.hideFlags = HideFlags.HideAndDontSave;
        lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        lineMaterial.SetInt("_ZWrite", 0);
    }

    void OnRenderObject()
    {
        if (mainCamera == null)
            return;

        lineMaterial.SetPass(0);

        GL.PushMatrix();
        GL.MultMatrix(transform.localToWorldMatrix);

        GL.Begin(GL.LINES);
        GL.Color(gridColor);

        // Get the camera bounds
        Vector3 cameraPosition = mainCamera.transform.position;
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float left = cameraPosition.x - cameraWidth / 2;
        float right = cameraPosition.x + cameraWidth / 2;
        float top = cameraPosition.y + cameraHeight / 2;
        float bottom = cameraPosition.y - cameraHeight / 2;

        // Calculate the number of vertical and horizontal lines needed
        int verticalLines = Mathf.CeilToInt((right - left) / cellSize) / 2;
        int horizontalLines = Mathf.CeilToInt((top - bottom) / cellSize) / 2;

        // Draw vertical lines
        for (int i = -verticalLines; i <= verticalLines; i++)
        {
            float x = Mathf.Round(cameraPosition.x / cellSize) * cellSize + i * cellSize;
            GL.Vertex3(x, bottom, 0);
            GL.Vertex3(x, top, 0);
        }

        // Draw horizontal lines
        for (int j = -horizontalLines; j <= horizontalLines; j++)
        {
            float y = Mathf.Round(cameraPosition.y / cellSize) * cellSize + j * cellSize;
            GL.Vertex3(left, y, 0);
            GL.Vertex3(right, y, 0);
        }

        GL.End();
        GL.PopMatrix();
    }
}
