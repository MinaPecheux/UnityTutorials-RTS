using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class UnitManager : MonoBehaviour
{
    protected static MaterialPropertyBlock _materialPropertyBlock;
    protected static MaterialPropertyBlock _MaterialPropertyBlock
    {
        get
        {
            if (_materialPropertyBlock == null)
                _materialPropertyBlock = new MaterialPropertyBlock();
            return _materialPropertyBlock;
        }
    }

    public GameObject selectionCircle;
    public GameObject fov;
    public Renderer meshRenderer;
    public AudioSource contextualSource;
    public Animator animator;

    public int ownerMaterialSlotIndex = 0;

    protected BoxCollider _collider;
    public virtual Unit Unit { get; set; }

    private bool _selected = false;
    public bool IsSelected { get => _selected; }
    private int _selectIndex = -1;
    public int SelectIndex { get => _selectIndex; }

    public GameObject healthbar;
    protected Renderer _healthbarRenderer;
    private GameObject _levelUpVFX;
    private Material _levelUpVFXMaterial;
    private Coroutine _levelUpVFXCoroutine = null;

    private Vector3 _meshSize;
    public Vector3 MeshSize => _meshSize;

    private void Awake()
    {
        _meshSize = meshRenderer.GetComponent<Renderer>().bounds.size / 2;

        if (healthbar)
        {
            healthbar.SetActive(false);
            _healthbarRenderer = healthbar.GetComponent<Renderer>();
        }
    }

    private void OnMouseDown()
    {
        Select(
            true,
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift)
        );
    }

    public void Initialize(Unit unit)
    {
        _collider = GetComponent<BoxCollider>();
        Unit = unit;
    }

    public void EnableFOV(float size)
    {
        fov.SetActive(true);
        MeshRenderer mr = fov.GetComponent<MeshRenderer>();
        mr.material = new Material(mr.material);
        StartCoroutine(_ScalingFOV(size));
    }

    public void SetOwnerMaterial(int owner)
    {
        Color playerColor = GameManager.instance.gamePlayersParameters.players[owner].color;
        Material[] materials = meshRenderer.materials;
        materials[ownerMaterialSlotIndex].color = playerColor;
        meshRenderer.materials = materials;
    }

    public void Attack(Transform target)
    {
        UnitManager um = target.GetComponent<UnitManager>();
        if (um == null) return;
        um.TakeHit(Unit.AttackDamage);
    }

    public void TakeHit(int attackPoints)
    {
        Unit.HP -= attackPoints;
        UpdateHealthbar();
        if (Unit.HP <= 0) _Die();
    }

    protected virtual bool IsActive()
    {
        return true;
    }

    protected virtual bool IsMovable()
    {
        return true;
    }

    public void Select() { Select(false, false); }
    public void Select(bool singleClick, bool holdingShift)
    {
        // basic case: using the selection box
        if (!singleClick)
        {
            _SelectUtil();
            return;
        }

        // single click: check for shift key
        if (!holdingShift)
        {
            List<UnitManager> selectedUnits = new List<UnitManager>(Globals.SELECTED_UNITS);
            foreach (UnitManager um in selectedUnits)
                um.Deselect();
            _SelectUtil();
        }
        else
        {
            if (!Globals.SELECTED_UNITS.Contains(this))
                _SelectUtil();
            else
                Deselect();
        }
    }

    public void Deselect()
    {
        if (!Globals.SELECTED_UNITS.Contains(this)) return;
        Globals.SELECTED_UNITS.Remove(this);

        EventManager.TriggerEvent("DeselectedUnit", Unit);
        selectionCircle.SetActive(false);
        if (healthbar)
            healthbar.SetActive(false);
        _selected = false;
        _selectIndex = -1;
    }

    private IEnumerator _ScalingFOV(float size)
    {
        float r = 0f, t = 0f, step = 0.02f;
        float scaleUpTime = 0.3f;
        Vector3 _startScale = fov.transform.localScale;
        Vector3 _endScale = size * Vector3.one;
        _endScale.z = 1f;
        do
        {
            fov.transform.localScale = Vector3.Lerp(_startScale, _endScale, r);
            t += step;
            r = t / scaleUpTime;
            yield return new WaitForSecondsRealtime(step);
        } while (r < 1f);
    }

    private void _SelectUtil()
    {
        // abort if not active
        if (!IsActive()) return;
        // abort if already selected
        if (Globals.SELECTED_UNITS.Contains(this)) return;

        Globals.SELECTED_UNITS.Add(this);
        EventManager.TriggerEvent("SelectedUnit", Unit);
        selectionCircle.SetActive(true);
        if (healthbar)
        {
            healthbar.SetActive(true);
            UpdateHealthbar();
        }

        // play sound
        contextualSource.PlayOneShot(Unit.Data.onSelectSound);

        _selected = true;
        _selectIndex = Globals.SELECTED_UNITS.Count - 1;
    }

    private void _Die()
    {
        if (_selected)
            Deselect();
        Destroy(gameObject);
    }

    protected virtual void UpdateHealthbar()
    {
        if (!_healthbarRenderer) return;
        _healthbarRenderer.GetPropertyBlock(_MaterialPropertyBlock);
        _MaterialPropertyBlock.SetFloat("_Health", Unit.HP / (float)Unit.MaxHP);
        _healthbarRenderer.SetPropertyBlock(_MaterialPropertyBlock);
    }

    public void LevelUp()
    {
        // destroy previous (unfinished) visual effect
        if (_levelUpVFX)
        {
            if (_levelUpVFXCoroutine != null)
                StopCoroutine(_levelUpVFXCoroutine);
            _levelUpVFXCoroutine = null;
            Destroy(_levelUpVFX);
        }

        // play sound
        AudioClip clip = GameManager.instance.gameGlobalParameters.onLevelUpSound;
        contextualSource.PlayOneShot(clip);

        // create visual effect (+ discard it after a few seconds)
        GameObject vfx = Instantiate(Resources.Load("Prefabs/Units/LevelUpVFX")) as GameObject;
        Vector3 meshScale = meshRenderer.transform.localScale;
        float s = Mathf.Max(meshScale.x, meshScale.z);
        Transform t = vfx.transform;
        t.localScale = new Vector3(s, meshScale.y, s);
        t.position = transform.position;

        _levelUpVFX = vfx;
        _levelUpVFXMaterial = t.GetComponent<Renderer>().material;
        _levelUpVFXCoroutine = StartCoroutine("_UpdatingLevelUpVFX");
    }

    private IEnumerator _UpdatingLevelUpVFX()
    {
        float lifetime = 1f, t = 0f, step = 0.05f;
        while (t < lifetime) {
            _levelUpVFXMaterial.SetFloat("_CurrentTime", t);
            t += step;
            yield return new WaitForSeconds(step);
        }
        Destroy(_levelUpVFX);
    }

    public void SetAnimatorBoolVariable(string name, bool boolValue)
    {
        if (animator == null) return;
        animator.SetBool(name, boolValue);
    }

    public void SetAnimatorTriggerVariable(string name)
    {
        if (animator == null) return;
        animator.SetTrigger(name);
    }
}
