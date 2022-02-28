using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class Utils
{
    static Camera _mainCamera;
    public static Camera MainCamera
    {
        get
        {
            if (_mainCamera == null)
                _mainCamera = Camera.main;
            return _mainCamera;
        }
    }

    static Texture2D _whiteTexture;
    public static Texture2D WhiteTexture
    {
        get
        {
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            return _whiteTexture;
        }
    }

    static Ray _ray;
    static RaycastHit _hit;

    public static void DrawScreenRect(Rect rect, Color color)
    {
        GUI.color = color;
        GUI.DrawTexture(rect, WhiteTexture);
        GUI.color = Color.white;
    }

    public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
    {
        // Top
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
        // Left
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
        // Right
        Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
        // Bottom
        Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
    }

    public static Rect GetScreenRect(Vector3 screenPosition1, Vector3 screenPosition2)
    {
        // Move origin from bottom left to top left
        screenPosition1.y = Screen.height - screenPosition1.y;
        screenPosition2.y = Screen.height - screenPosition2.y;
        // Calculate corners
        var topLeft = Vector3.Min(screenPosition1, screenPosition2);
        var bottomRight = Vector3.Max(screenPosition1, screenPosition2);
        // Create Rect
        return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
    }

    public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
    {
        var v1 = MainCamera.ScreenToViewportPoint(screenPosition1);
        var v2 = MainCamera.ScreenToViewportPoint(screenPosition2);
        var min = Vector3.Min(v1, v2);
        var max = Vector3.Max(v1, v2);
        min.z = camera.nearClipPlane;
        max.z = camera.farClipPlane;

        var bounds = new Bounds();
        bounds.SetMinMax(min, max);
        return bounds;
    }

    public static Vector3 MiddleOfScreenPointToWorld()
        { return MiddleOfScreenPointToWorld(MainCamera); }
    public static Vector3 MiddleOfScreenPointToWorld(Camera cam)
    {
        _ray = cam.ScreenPointToRay(0.5f * new Vector2(Screen.width, Screen.height));
        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            )) return _hit.point;
        return Vector3.zero;
    }

    public static Vector3[] ScreenCornersToWorldPoints()
        { return ScreenCornersToWorldPoints(MainCamera); }
    public static Vector3[] ScreenCornersToWorldPoints(Camera cam)
    {
        Vector3[] corners = new Vector3[4];
        for (int i = 0; i < 4; i++)
        {
            _ray = cam.ViewportPointToRay(new Vector2(i % 2, i / 2));
            if (Physics.Raycast(
                    _ray,
                    out _hit,
                    1000f,
                    Globals.TERRAIN_LAYER_MASK
                )) corners[i] = _hit.point;
        }
        return corners;
    }

    public static int GetAlphaKeyValue(string inputString)
    {
        if (inputString == "0" || inputString == "à") return 0;
        if (inputString == "1" || inputString == "&") return 1;
        if (inputString == "2" || inputString == "é") return 2;
        if (inputString == "3" || inputString == "\"") return 3;
        if (inputString == "4" || inputString == "'") return 4;
        if (inputString == "5" || inputString == "(") return 5;
        if (inputString == "6" || inputString == "§") return 6;
        if (inputString == "7" || inputString == "è") return 7;
        if (inputString == "8" || inputString == "!") return 8;
        if (inputString == "9" || inputString == "ç") return 9;
        return -1;
    }

    static Regex camelCaseRegex = new Regex(@"(?:[a-z]+|[A-Z]+|^)([a-z]|\d)*", RegexOptions.Compiled);
    public static string CapitalizeWords(string str)
    {
        List<string> words = new List<string>();
        MatchCollection matches = camelCaseRegex.Matches(str);
        string word;
        foreach (Match match in matches)
        {
            word = match.Groups[0].Value;
            word = word[0].ToString().ToUpper() + word.Substring(1);
            words.Add(word);
        }
        return string.Join(" ", words);
    }

    public static Color LightenColor(Color color, float factor)
    {
        return new Color(
            Mathf.Clamp01(color.r + factor),
            Mathf.Clamp01(color.g + factor),
            Mathf.Clamp01(color.b + factor),
            Mathf.Clamp01(color.a + factor)
        );
    }

    public static List<Vector2> SampleOffsets(int amount, float radius, Vector2 sampleRegionSize)
    {
        List<Vector2> offsets = new List<Vector2>();
        List<Vector2> samples = PoissonDiscSampling.GeneratePoints(radius, sampleRegionSize);
        float hw = sampleRegionSize.x / 2f;
        float hh = sampleRegionSize.y / 2f;
        for (int i = 0; i < amount && i < samples.Count; i++)
            offsets.Add(new Vector2(samples[i].x - hw, samples[i].y - hh));
        return offsets;
    }

    public static List<Vector3> SamplePositions(int amount, float radius, Vector2 sampleRegionSize)
    {
        return SamplePositions(amount, radius, sampleRegionSize, Vector3.zero);
    }
    public static List<Vector3> SamplePositions(int amount, float radius, Vector2 sampleRegionSize, Vector3 referencePoint)
    {
        List<Vector2> offsets = SampleOffsets(amount, radius, sampleRegionSize);
        return OffsetsToPositions(offsets, referencePoint);
    }

    public static List<Vector3> OffsetsToPositions(List<Vector2> offsets, Vector3 referencePoint)
    {
        List<Vector3> positions = new List<Vector3>();
        for (int i = 0; i < offsets.Count; i++)
        {
            // place unit high above the ground and raycast down
            // to project it back on the terrain
            Vector3 initialPos = referencePoint + new Vector3(offsets[i].x, 1000, offsets[i].y);
            if (Physics.Raycast(initialPos, Vector3.down, out _hit, 2000f, Globals.TERRAIN_LAYER_MASK))
                positions.Add(_hit.point);
        }
        return positions;
    }

    public static Vector3 ProjectOnTerrain(Vector3 pos)
    {
        // place unit high above the ground and raycast down
        // to project it back on the terrain
        Vector3 initialPos = pos + Vector3.up * 1000f;
        if (Physics.Raycast(initialPos, Vector3.down, out _hit, 2000f, Globals.FLAT_TERRAIN_LAYER_MASK))
            pos = _hit.point;
        return pos;
    }

    public static (Vector3, Vector3) GetCameraWorldBounds()
    {
        Vector3 bottomLeftCorner = new Vector3(0f, 0f);
        Vector3 topRightCorner = new Vector3(1f, 1f);
        float dist = 1000f;

        _ray = MainCamera.ViewportPointToRay(bottomLeftCorner);
        Vector3 bottomLeft =
            GameManager.instance.mapWrapperCollider.Raycast(_ray, out _hit, dist)
                ? _hit.point : Vector3.zero;

        _ray = MainCamera.ViewportPointToRay(topRightCorner);
        Vector3 topRight =
            GameManager.instance.mapWrapperCollider.Raycast(_ray, out _hit, dist)
                ? _hit.point : Vector3.zero;

        return (bottomLeft, topRight);
    }

    public static Sprite LoadSpriteFromFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath)) return null;

        byte[] byteArray = System.IO.File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        bool isLoaded = tex.LoadImage(byteArray); // this auto-resizes the texture
        if (isLoaded)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 64f);
        return null;
    }
}
