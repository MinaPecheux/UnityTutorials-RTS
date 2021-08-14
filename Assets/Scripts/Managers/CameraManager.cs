using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    public float translationSpeed = 60f;
    public float zoomSpeed = 30f;
    public float altitude = 40f;

    public Transform groundTarget;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private Vector3 _forwardDir;
    private Coroutine _mouseOnScreenCoroutine;
    private int _mouseOnScreenBorder;
    private bool _placingBuilding;

    public Material minimapIndicatorMaterial;
    private float _minimapIndicatorStrokeWidth = 0.1f; // relative to indicator size
    private Transform _minimapIndicator;
    private Mesh _minimapIndicatorMesh;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _mouseOnScreenCoroutine = null;
        _mouseOnScreenBorder = -1;
        _placingBuilding = false;

        _PrepareMapIndicator();

        groundTarget.position = Utils.MiddleOfScreenPointToWorld();
    }

    void Update()
    {
        if (GameManager.instance.gameIsPaused) return;

        if (_mouseOnScreenBorder >= 0)
        {
            _TranslateCamera(_mouseOnScreenBorder);
        }
        else
        {
            if (Input.GetKey(KeyCode.UpArrow))
                _TranslateCamera(0);
            if (Input.GetKey(KeyCode.RightArrow))
                _TranslateCamera(1);
            if (Input.GetKey(KeyCode.DownArrow))
                _TranslateCamera(2);
            if (Input.GetKey(KeyCode.LeftArrow))
                _TranslateCamera(3);
        }

        // only use scroll for zoom if not currently placing a building
        if (!_placingBuilding && Math.Abs(Input.mouseScrollDelta.y) > 0f)
            _Zoom(Input.mouseScrollDelta.y > 0f ? 1 : -1);
    }

    private void OnEnable()
    {
        EventManager.AddListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", _OnPlaceBuildingOff);
        EventManager.AddListener("MoveCamera", _OnMoveCamera);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
        EventManager.RemoveListener("MoveCamera", _OnMoveCamera);
    }

    private void _OnPlaceBuildingOn()
    {
        _placingBuilding = true;
    }

    private void _OnPlaceBuildingOff()
    {
        _placingBuilding = false;
    }

    public void OnMouseEnterScreenBorder(int borderIndex)
    {
        _mouseOnScreenCoroutine = StartCoroutine(_SetMouseOnScreenBorder(borderIndex));
    }

    public void OnMouseExitScreenBorder()
    {
        StopCoroutine(_mouseOnScreenCoroutine);
        _mouseOnScreenBorder = -1;
    }

    private IEnumerator _SetMouseOnScreenBorder(int borderIndex)
    {
        yield return new WaitForSeconds(0.3f);
        _mouseOnScreenBorder = borderIndex;
    }

    private void _OnMoveCamera(object data)
    {
        Vector3 pos = (Vector3) data;
        float indicatorW = _minimapIndicatorMesh.vertices[1].x - _minimapIndicatorMesh.vertices[0].x;
        float indicatorH = _minimapIndicatorMesh.vertices[2].z - _minimapIndicatorMesh.vertices[0].z;
        pos.x -= indicatorW / 2f;
        pos.z -= indicatorH / 2f;
        Vector3 off = transform.position - Utils.MiddleOfScreenPointToWorld();
        Vector3 newPos = pos + off;
        newPos.y = 100f;
        transform.position = newPos;

        _FixAltitude();
        _ComputeMinimapIndicator(false);
    }

    private void _TranslateCamera(int dir)
    {
        if (dir == 0)       // top
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 1)  // right
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2)  // bottom
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 3)  // left
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

        _FixAltitude();
        _ComputeMinimapIndicator(false);
    }

    private void _Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, 26f);

        _ComputeMinimapIndicator(true);
    }

    private void _FixAltitude()
    {
        // translate camera at proper altitude: cast a ray to the ground
        // and move up the hit point
        _ray = new Ray(transform.position, Vector3.up * -1000f);
        if (Physics.Raycast(
                _ray,
                out _hit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            )) transform.position = _hit.point + Vector3.up * altitude;
    }

    private void _PrepareMapIndicator()
    {
        GameObject g = new GameObject("MinimapIndicator");
        _minimapIndicator = g.transform;
        g.layer = 11; // put on "Minimap" layer
        _minimapIndicator.position = Vector3.zero;
        _minimapIndicatorMesh = _CreateMinimapIndicatorMesh();
        MeshFilter mf = g.AddComponent<MeshFilter>();
        mf.mesh = _minimapIndicatorMesh;
        MeshRenderer mr = g.AddComponent<MeshRenderer>();
        mr.material = new Material(minimapIndicatorMaterial);
        _ComputeMinimapIndicator(true);
    }

    private Mesh _CreateMinimapIndicatorMesh()
    {
        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[] {
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,
            Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero
        };
        int[] triangles = new int[] {
            0, 4, 1, 4, 5, 1,
            0, 2, 6, 6, 4, 0,
            6, 2, 7, 2, 3, 7,
            5, 7, 3, 3, 1, 5
        };
        m.vertices = vertices;
        m.triangles = triangles;
        return m;
    }

    private void _ComputeMinimapIndicator(bool zooming)
    {
        Vector3 middle = Utils.MiddleOfScreenPointToWorld();
        groundTarget.position = middle;
        // if zooming: recompute the indicator mesh
        if (zooming)
        {
            Vector3[] viewCorners = Utils.ScreenCornersToWorldPoints();
            float w = viewCorners[1].x - viewCorners[0].x;
            float h = viewCorners[2].z - viewCorners[0].z;
            for (int i = 0; i < 4; i++)
            {
                viewCorners[i].x -= middle.x;
                viewCorners[i].z -= middle.z;
            }
            Vector3[] innerCorners = new Vector3[]
            {
                new Vector3(viewCorners[0].x + _minimapIndicatorStrokeWidth * w, 0f, viewCorners[0].z + _minimapIndicatorStrokeWidth * h),
                new Vector3(viewCorners[1].x - _minimapIndicatorStrokeWidth * w, 0f, viewCorners[1].z + _minimapIndicatorStrokeWidth * h),
                new Vector3(viewCorners[2].x + _minimapIndicatorStrokeWidth * w, 0f, viewCorners[2].z - _minimapIndicatorStrokeWidth * h),
                new Vector3(viewCorners[3].x - _minimapIndicatorStrokeWidth * w, 0f, viewCorners[3].z - _minimapIndicatorStrokeWidth * h)
            };
            Vector3[] allCorners = new Vector3[]
            {
                viewCorners[0], viewCorners[1], viewCorners[2], viewCorners[3],
                innerCorners[0], innerCorners[1], innerCorners[2], innerCorners[3]
            };
            for (int i = 0; i < 8; i++)
                allCorners[i].y = 100f;
            _minimapIndicatorMesh.vertices = allCorners;
            _minimapIndicatorMesh.RecalculateNormals();
            _minimapIndicatorMesh.RecalculateBounds();
        }
        // move the game object at the center of the main camera screen
        _minimapIndicator.position = middle;
    }
}
