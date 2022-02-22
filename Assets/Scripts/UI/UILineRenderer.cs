/* Adapted from: https://www.youtube.com/watch?v=--LB7URk60A */
using UnityEngine;
using UnityEngine.UI;

public class UILineRenderer : Graphic
{

    public float thickness;

    public Vector2 start;
    public Vector2 end;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        float angle = GetAngle(start, end) + 90f;
        DrawVerticesForPoint(start, end, angle, vh);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(1, 2, 3);
    }

    public float GetAngle(Vector2 me, Vector2 target)
    {
        //panel resolution go there in place of 9 and 16

        return (float)(Mathf.Atan2(
            Screen.height * (target.y - me.y),
            Screen.width * (target.x - me.x)
        ) * (180 / Mathf.PI));
    }

    void DrawVerticesForPoint(Vector2 p1, Vector2 p2, float angle, VertexHelper vh)
    {
        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(p1.x, p1.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(p1.x, p1.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(-thickness / 2, 0);
        vertex.position += new Vector3(p2.x, p2.y);
        vh.AddVert(vertex);

        vertex.position = Quaternion.Euler(0, 0, angle) * new Vector3(thickness / 2, 0);
        vertex.position += new Vector3(p2.x, p2.y);
        vh.AddVert(vertex);
    }
}
