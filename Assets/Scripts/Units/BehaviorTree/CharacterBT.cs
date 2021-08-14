using System.Collections.Generic;

using BehaviorTree;

[UnityEngine.RequireComponent(typeof(CharacterManager))]
public class CharacterBT : Tree
{
    CharacterManager manager;

    private void Awake()
    {
        manager = GetComponent<CharacterManager>();
    }

    protected override Node SetupTree()
    {
        Node _root;

        // prepare our subtrees...
        Sequence trySetDestinationOrTargetSequence = new Sequence(new List<Node> {
            new CheckUnitIsMine(manager),
            new TaskTrySetDestinationOrTarget(manager),
        });

        Sequence moveToDestinationSequence = new Sequence(new List<Node> {
            new CheckHasDestination(),
            new TaskMoveToDestination(manager),
        });

        Sequence attackSequence = new Sequence(new List<Node> {
            new Inverter(new List<Node>
            {
                new CheckTargetIsMine(manager),
            }),
            new CheckEnemyInAttackRange(manager),
            new Timer(
                manager.Unit.Data.attackRate,
                new List<Node>()
                {
                    new TaskAttack(manager)
                }
            ),
        });

        Sequence moveToTargetSequence = new Sequence(new List<Node> {
            new CheckHasTarget()
        });
        if (manager.Unit.Data.attackDamage > 0)
        {
            moveToTargetSequence.Attach(new Selector(new List<Node> {
                attackSequence,
                new TaskFollow(manager),
            }));
        }
        else
        {
            moveToTargetSequence.Attach(new TaskFollow(manager));
        }

        // ... then stitch them together under the root
        _root = new Selector(new List<Node> {
            new Parallel(new List<Node> {
                trySetDestinationOrTargetSequence,
                new Selector(new List<Node>
                {
                    moveToDestinationSequence,
                    moveToTargetSequence,
                }),
            }),
            new CheckEnemyInFOVRange(manager),
        });

        return _root;
    }
}
