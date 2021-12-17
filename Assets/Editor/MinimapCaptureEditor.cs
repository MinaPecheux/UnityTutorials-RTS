using UnityEngine;
using UnityEditor;

public class MinimapCaptureWindow : EditorWindow
{
    private Transform _screenshotCameraAnchor;
    private Camera _screenshotCamera;
    private string _screenshotName = "map";
    private float _mapSize = 700;
    private float _imgSize = 1080;

    [MenuItem("Tools/Minimap Capture")]
    static void Init()
    {
        // get existing open window or make a new one
        MinimapCaptureWindow window = (MinimapCaptureWindow)GetWindow(typeof(MinimapCaptureWindow));
        // set title and icon
        GUIContent titleContent = new GUIContent("Minimap Capture");
        window.titleContent = titleContent;
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Minimap Capture", EditorStyles.boldLabel);

        _screenshotCameraAnchor = (Transform)EditorGUILayout.ObjectField(
            "Camera Anchor", _screenshotCameraAnchor, typeof(Transform), true);
        _screenshotCamera = (Camera)EditorGUILayout.ObjectField(
            "Camera", _screenshotCamera, typeof(Camera), true);
        _screenshotName = EditorGUILayout.TextField("Name", _screenshotName);
        _mapSize = EditorGUILayout.FloatField("Map Size", _mapSize);
        _imgSize = EditorGUILayout.FloatField("Image Size", _imgSize);

        EditorGUILayout.Space();

        EditorGUI.BeginDisabledGroup(_screenshotCameraAnchor == null || _screenshotCamera == null);
        if (GUILayout.Button("Take Screenshot"))
        {
            Vector3 prevAnchorPos = _screenshotCameraAnchor.position;
            float prevOrthoSize = _screenshotCamera.orthographicSize;

            float t = _mapSize / 2;
            _screenshotCameraAnchor.position = new Vector3(t, 0, t);
            _screenshotCamera.orthographicSize = t;

            MinimapCapture.TakeScreenshot(
                _screenshotName,
                new Vector2Int((int)_imgSize, (int)_imgSize),
                _screenshotCamera);

            _screenshotCameraAnchor.position = prevAnchorPos;
            _screenshotCamera.orthographicSize = prevOrthoSize;
        }
        EditorGUI.EndDisabledGroup();
    }

}
