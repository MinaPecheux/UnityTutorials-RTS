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

    public bool autoAdaptAltitude;

    private Camera _camera;
    private RaycastHit _hit;
    private Ray _ray;

    private float _distance = 500f;
    private Vector3 _forwardDir;
    private Coroutine _mouseOnScreenCoroutine;
    private int _mouseOnScreenBorder;
    private bool _placingBuilding;

    private float _minX;
    private float _maxX;
    private float _minZ;
    private float _maxZ;
    private Vector3 _camOffset;
    private Vector3 _camHalfViewZone;
    private float _camMinimapBuffer = 5f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _forwardDir = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        _mouseOnScreenCoroutine = null;
        _mouseOnScreenBorder = -1;
        _placingBuilding = false;
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
        EventManager.AddListener("ClickedMinimap", _OnClickedMinimap);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
        EventManager.RemoveListener("ClickedMinimap", _OnClickedMinimap);
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

    private void _OnClickedMinimap(object data)
    {
        Vector3 pos = _FixBounds((Vector3) data);
        SetPosition(pos);

        if (autoAdaptAltitude)
            _FixAltitude();
    }

    private void _TranslateCamera(int dir)
    {
        if (dir == 0 && transform.position.z - _camOffset.z + _camHalfViewZone.z <= _maxZ)          // top
            transform.Translate(_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 1 && transform.position.x + _camHalfViewZone.x <= _maxX)                    // right
            transform.Translate(transform.right * Time.deltaTime * translationSpeed);
        else if (dir == 2 && transform.position.z - _camOffset.z - _camHalfViewZone.z >= _minZ)     // bottom
            transform.Translate(-_forwardDir * Time.deltaTime * translationSpeed, Space.World);
        else if (dir == 3 && transform.position.x - _camHalfViewZone.x >= _minX)                    // left
            transform.Translate(-transform.right * Time.deltaTime * translationSpeed);

        _FixGroundTarget();

        if (autoAdaptAltitude)
            _FixAltitude();
    }

    private void _Zoom(int zoomDir)
    {
        _camera.orthographicSize += zoomDir * Time.deltaTime * zoomSpeed;
        _camera.orthographicSize = Mathf.Clamp(_camera.orthographicSize, 8f, 26f);

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;

        // fix bounds
        Vector3 pos = Utils.MiddleOfScreenPointToWorld();
        pos = _FixBounds(pos);
        SetPosition(pos);
    }

    private Vector3 _FixBounds(Vector3 pos)
    {
        if (pos.x - _camHalfViewZone.x < _minX) pos.x = _minX + _camHalfViewZone.x;
        if (pos.x + _camHalfViewZone.x > _maxX) pos.x = _maxX - _camHalfViewZone.x;
        if (pos.z - _camHalfViewZone.z < _minZ) pos.z = _minZ + _camHalfViewZone.z;
        if (pos.z + _camHalfViewZone.z > _maxZ) pos.z = _maxZ - _camHalfViewZone.z;
        return pos;
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

    private void _FixGroundTarget()
    {
        groundTarget.position = Utils.MiddleOfScreenPointToWorld(_camera);
    }

    public void InitializeBounds()
    {
        _minX = 0;
        _maxX = GameManager.instance.terrainSize;
        _minZ = 0;
        _maxZ = GameManager.instance.terrainSize;

        (Vector3 minWorldPoint, Vector3 maxWorldPoint) = Utils.GetCameraWorldBounds();
        _camOffset = transform.position - (maxWorldPoint + minWorldPoint) / 2f;
        _camHalfViewZone = (maxWorldPoint - minWorldPoint) / 2f + Vector3.one * _camMinimapBuffer;

        _FixGroundTarget();
    }

    public void SetPosition(Vector3 pos)
    {
        transform.position = pos - _distance * transform.forward;
        _FixGroundTarget();
    }
}
