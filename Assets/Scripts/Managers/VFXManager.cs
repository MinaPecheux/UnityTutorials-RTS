using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VfxType
{
    Smoke
}

public class VFXManager : MonoBehaviour
{
    public static VFXManager instance;

    public GameObject poolPrefab;

    [Header("Smoke")]
    private const int SMOKE_EFFECT_POOL_SIZE = 12;
    public GameObject[] smokeEffectPrefabs;

    private Dictionary<VfxType, Transform> _vfxPools;

    public void Start()
    {
        instance = this;

        _vfxPools = new Dictionary<VfxType, Transform>();

        Transform smokePool = Instantiate(poolPrefab, transform).transform;
        smokePool.gameObject.name = "Smoke";
        _vfxPools[VfxType.Smoke] = smokePool;
        Transform smokeStockParent = smokePool.Find("Stock");
        for (int i = 0; i < SMOKE_EFFECT_POOL_SIZE; i++)
        {
            Instantiate(
                smokeEffectPrefabs[Random.Range(0, smokeEffectPrefabs.Length)],
                smokeStockParent);
        }
    }

    public Transform Spawn(VfxType type, Vector3 position, bool randomizeSize = true)
    {
        if (_vfxPools == null) return null;

        Transform pool = _vfxPools[type];
        Transform poolStock = pool.Find("Stock");
        Transform poolInUse = pool.Find("InUse");
        Transform vfx;
        // if there are remaining objects in the pool stock:
        // move the first one to the "in use" pile
        if (poolStock.childCount > 0)
        {
            vfx = poolStock.GetChild(0);
            vfx.SetParent(poolInUse);
        }
        // else re-assign an already "in use" effect
        else
        {
            vfx = poolInUse.GetChild(0);
            vfx.SetAsLastSibling();
        }
        vfx.position = position;
        if (randomizeSize)
            vfx.localScale = Vector3.one * Random.Range(0.3f, 1.5f);

        return vfx;
    }

    public void Unspawn(VfxType type)
    {
        if (_vfxPools == null) return;

        Transform pool = _vfxPools[type];
        Transform poolStock = pool.Find("Stock");
        Transform poolInUse = pool.Find("InUse");
        if (poolInUse.childCount > 0)
            poolInUse.GetChild(0).SetParent(poolStock);
    }

    public void Unspawn(VfxType type, Transform vfx)
    {
        if (_vfxPools == null) return;

        Transform pool = _vfxPools[type];
        Transform poolStock = pool.Find("Stock");
        vfx.SetParent(poolStock);
    }
}
