using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CharacterManager : UnitManager
{
    public NavMeshAgent agent;

    private Character _character;
    public override Unit Unit
    {
        get { return _character; }
        set { _character = value is Character ? (Character)value : null; }
    }

    public BoxCollider characterCollider;

    private bool _isConstructor = false;
    public bool IsConstructor => _isConstructor;

    private void Start()
    {
        _character.Place();
    }

    public bool MoveTo(Vector3 targetPosition, bool playSound = true)
    {
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(targetPosition, path);
        if (path.status == NavMeshPathStatus.PathInvalid)
        {
            if (playSound)
                contextualSource.PlayOneShot(((CharacterData)Unit.Data).onMoveInvalidSound);
            return false;
        }

        agent.destination = targetPosition;
        if (playSound)
            contextualSource.PlayOneShot(((CharacterData)Unit.Data).onMoveValidSound);
        return true;
    }

    public void SetRendererVisibility(bool on)
    {
        if (on && !meshRenderer.enabled)
        {
            meshRenderer.enabled = true;
            characterCollider.enabled = true;
        }
        else if (!on && meshRenderer.enabled)
        {
            meshRenderer.enabled = false;
            characterCollider.enabled = false;
        }
    }

    public void SetIsConstructor(bool on)
    {
        _isConstructor = on;
        if (_isConstructor) Deselect();
    }

    protected override bool IsActive()
    {
        return !_isConstructor;
    }
}
