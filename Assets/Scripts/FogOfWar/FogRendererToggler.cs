using System.Collections;
using UnityEngine;

public class FogRendererToggler : MonoBehaviour
{
    public GameObject mesh; // reference to the render you want toggled based on the position of this transform
    [Range(0f, 1f)] public float threshold = 0.1f; //the threshold for when this script considers myRenderer should render

    private const float _UPDATE_RATE = 0.5f;

    private Camera _camera; // the camera using the masked render texture
    private Coroutine _texUpdateCoroutine;

    private FogRendererToggler _mainInstance;
    // made so all instances share the same texture, reducing texture reads
    private static Texture2D _shadowTexture;
    private static Rect _rect;
    private static bool _validTexture = true;

    private void Start()
    {
        // disable if:
        // - FOV game parameter is inactive
        // - or no mesh is defined
        if (
            !GameManager.instance.gameGlobalParameters.enableFOV ||
            !mesh
        ) {
            Destroy(this);
            return;
        }

        // also disable if the unit is mine
        UnitManager um = GetComponent<UnitManager>();
        if (um != null && um.Unit.Owner == GameManager.instance.gamePlayersParameters.myPlayerId)
        {
            Destroy(this);
            return;
        }

        // mark the "first" (arbitrary) instance as main
        if (_mainInstance == null)
            _mainInstance = this;


        // only run the texture updates on the main instance
        if (_mainInstance == this)
        {
            _camera = GameObject.Find("UnexploredAreasCam").GetComponent<Camera>();
            _texUpdateCoroutine = StartCoroutine(_UpdatingShadowTexture());
        }
        else
        {
            _texUpdateCoroutine = null;
        }
    }

    private void OnDisable()
    {
        if (_texUpdateCoroutine != null)
        {
            StopCoroutine(_texUpdateCoroutine);
            _texUpdateCoroutine = null;
        }    
    }

    private void LateUpdate()
    {
        if (!_camera) return;
        bool active = _GetColorAtPosition().grayscale >= threshold;
        if (mesh.activeSelf != active)
            mesh.SetActive(active);
    }

    private void _UpdateShadowTexture()
    {
        if (!_camera)
        {
            _validTexture = false;
            return;
        }

        RenderTexture renderTexture = _camera.targetTexture;
        if (!renderTexture)
        {
            _validTexture = false;
            return;
        }

        if (
            _shadowTexture == null ||
            renderTexture.width != _rect.width ||
            renderTexture.height != _rect.height
        )
        {
            _rect = new Rect(0, 0, renderTexture.width, renderTexture.height);
            _shadowTexture = new Texture2D((int)_rect.width, (int)_rect.height, TextureFormat.RGB24, false);
        }

        RenderTexture.active = renderTexture;
        _shadowTexture.ReadPixels(_rect, 0, 0);
        RenderTexture.active = null;
    }

    private Color _GetColorAtPosition()
    {
        if (!_validTexture) return Color.white;
        Vector3 pixel = _camera.WorldToScreenPoint(transform.position);
        return _shadowTexture.GetPixel((int)pixel.x, (int)pixel.y);
    }

    private IEnumerator _UpdatingShadowTexture()
    {
        while (true)
        {
            _UpdateShadowTexture();
            yield return new WaitForSeconds(_UPDATE_RATE);
        }
    }
}
