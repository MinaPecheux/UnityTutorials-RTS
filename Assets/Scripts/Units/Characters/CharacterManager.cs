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
}
