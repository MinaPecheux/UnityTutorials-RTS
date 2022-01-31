using UnityEngine;
using System.IO;

public static class MinimapCapture
{
    public static void TakeScreenshot(string name, Vector2Int size, Camera camera, string folderPath = "")
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

        // write to a file in the project folder
        if (folderPath == "")
        {
            folderPath = Application.dataPath + "/Resources/MapCaptures";
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
        }
        string filePath = $"{folderPath}/{name}.jpg";
        File.WriteAllBytes(filePath, bytes);

        // clean
        RenderTexture.active = null;
        camera.targetTexture = prevRt;
        rt.DiscardContents();

        Debug.Log($"Took screenshot: {filePath}");
    }
}
