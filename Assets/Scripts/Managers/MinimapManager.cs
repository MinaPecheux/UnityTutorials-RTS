/* Adapted from:
 * https://medium.com/@alessandrovalcepina/creating-a-rts-like-minimap-with-unity-9cd578dc4522 */
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MinimapManager : MonoBehaviour
{
    public static bool IS_ENABLED = true;
    private static Material _indicatorMat;

    public float lineWidth;

    private Camera _minimapCam;

    private void Start()
    {
        if (_indicatorMat == null)
            _indicatorMat = new Material(Shader.Find("Sprites/Default"));
        _minimapCam = GetComponent<Camera>();
    }

    public void OnPostRender()
    {
        if (!IS_ENABLED) return;
        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        Vector3 minViewportPoint = _minimapCam.WorldToViewportPoint(minWorldPoint);
        Vector3 maxViewportPoint = _minimapCam.WorldToViewportPoint(maxWorldPoint);
        float minX = minViewportPoint.x;
        float minY = minViewportPoint.y;
        float maxX = maxViewportPoint.x;
        float maxY = maxViewportPoint.y;

        GL.PushMatrix();
        {
            _indicatorMat.SetPass(0);
            GL.LoadOrtho();

            GL.Begin(GL.QUADS);
            GL.Color(new Color(1f, 1f, 0.85f));
            {
                GL.Vertex(new Vector3(minX, minY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, minY + lineWidth, 0));

                GL.Vertex(new Vector3(minX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(minX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(minX + lineWidth, maxY, 0));

                GL.Vertex(new Vector3(minX, maxY + lineWidth, 0));
                GL.Vertex(new Vector3(minX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY - lineWidth, 0));
                GL.Vertex(new Vector3(maxX, maxY + lineWidth, 0));

                GL.Vertex(new Vector3(maxX + lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, minY, 0));
                GL.Vertex(new Vector3(maxX - lineWidth, maxY, 0));
                GL.Vertex(new Vector3(maxX + lineWidth, maxY, 0));
            }
            GL.End();
        }
        GL.PopMatrix();
    }

}