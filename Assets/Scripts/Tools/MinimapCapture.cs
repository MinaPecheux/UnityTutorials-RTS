using UnityEngine;

public static class MinimapCapture
{
    public static void TakeScreenshot(string name, Vector2Int size, Camera camera)
    {
        RenderTexture prevRt = camera.targetTexture;

        // prepare render
        RenderTexture rt = new RenderTexture(size.x, size.y, 24, RenderTextureFormat.ARGB32);
        rt.antiAliasing = 4;

        camera.targetTexture = rt;
        camera.Render();

        // transfer to Texture2D and bytes
        Texture2D output = new Texture2D(size.x, size.y, TextureFormat.RGB24, false);

        RenderTexture.active = rt;
        output.ReadPixels(new Rect(0, 0, size.x, size.y), 0, 0, false);

        byte[] bytes = output.EncodeToJPG(90);
        Object.DestroyImmediate(output);

        // For testing purposes, also write to a file in the project folder
        string filePath = Application.dataPath + $"/{name}.jpg";
        System.IO.File.WriteAllBytes(filePath, bytes);

        // clean
        RenderTexture.active = null;
        camera.targetTexture = prevRt;
        rt.DiscardContents();

        Debug.Log($"Took screenshot: {filePath}");
    }
}
