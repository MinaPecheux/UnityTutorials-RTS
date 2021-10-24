using System.Collections.Generic;
using UnityEngine;

public class UnitsFormation
{
    private static Vector2 _GetOrthogonalDirection(Vector3 dir)
    {
        Vector3 orthoDir3d = Vector3.Cross(Vector3.up, dir);
        Vector2 orthoDir = new Vector2(orthoDir3d.x, orthoDir3d.z);
        return orthoDir.normalized;
    }

    /* Line formation */
    public static List<Vector2> GetLineOffsets(int amount, float samplingRadius, Vector3 movementDirection)
    {
        Vector2 lineDirection = _GetOrthogonalDirection(movementDirection);
        List<Vector2> offsets = new List<Vector2>();
        for (int i = 0; i < amount; i++)
        {
            float d = samplingRadius * (i % 2 == 0 ? 1 : -1);
            offsets.Add(Mathf.CeilToInt(i / 2f) * d * lineDirection);
        }
        return offsets;
    }

    public static List<Vector3> GetLinePositions(int amount, float samplingRadius, Vector3 movementDirection, Vector3 referencePoint)
        => Utils.OffsetsToPositions(GetLineOffsets(amount, samplingRadius, movementDirection), referencePoint);

    /* Grid formation */
    public static List<Vector2> GetGridOffsets(int amount, float samplingRadius, Vector3 movementDirection)
    {
        int rowLength = 5; // max units per row
        Vector2 rowDirection = _GetOrthogonalDirection(movementDirection);
        Vector2 colDirection = -new Vector2(movementDirection.x, movementDirection.z);
        colDirection.Normalize();
        List<Vector2> offsets = new List<Vector2>();
        int x = 0;
        int y = 0;
        for (int i = 0; i < amount; i++)
        {
            if (i != 0 && i % rowLength == 0)
            {
                x = 0;
                y++;
            }
            float d = samplingRadius * (x % 2 == 0 ? 1 : -1);
            float c = samplingRadius * y;
            offsets.Add(Mathf.CeilToInt(x / 2f) * d * rowDirection + c * colDirection);
            x++;
        }
        return offsets;
    }

    public static List<Vector3> GetGridPositions(int amount, float samplingRadius, Vector3 movementDirection, Vector3 referencePoint)
        => Utils.OffsetsToPositions(GetGridOffsets(amount, samplingRadius, movementDirection), referencePoint);

    /* X-Cross formation */
    public static List<Vector2> GetXCrossOffsets(int amount, float samplingRadius, Vector3 movementDirection)
    {
        Vector3 diagA3d = Quaternion.Euler(0, 45, 0) * movementDirection;
        Vector3 diagB3d = Quaternion.Euler(0, -45, 0) * movementDirection;
        Vector2 diagA = new Vector2(diagA3d.x, diagA3d.z);
        Vector2 diagB = new Vector2(diagB3d.x, diagB3d.z);
        diagA.Normalize();
        diagB.Normalize();

        List<Vector2> offsets = new List<Vector2>();
        Vector2 diag;
        for (int i = 0; i < amount; i++)
        {
            diag = i % 2 == 0 ? diagA : diagB;
            float d = samplingRadius * (i % 4 < 2 ? 1 : -1);
            offsets.Add(Mathf.CeilToInt(i / 4f) * diag * d);
        }
        return offsets;
    }

    public static List<Vector3> GetXCrossPositions(int amount, float samplingRadius, Vector3 movementDirection, Vector3 referencePoint)
        => Utils.OffsetsToPositions(GetXCrossOffsets(amount, samplingRadius, movementDirection), referencePoint);

}
