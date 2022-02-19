using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "Technology Node",
    menuName = "Scriptable Objects/Technology Tree/Node")]
public class TechnologyNodeData : ScriptableObject
{
    public static string ROOT_NODE_CODE = "root";
    public static Dictionary<string, TechnologyNodeData> TECH_TREE_NODES;

    public string code;
    public string displayName;
    public float researchTime;
    public List<ResourceValue> researchCost;
    public List<TechnologyNodeData> children;

    private bool _unlocked;
    public bool Unlocked => _unlocked;

    private TechnologyNodeData _parent;
    public TechnologyNodeData Parent => _parent;

    public static void LoadTechnologyTree()
    {
        TECH_TREE_NODES = new Dictionary<string, TechnologyNodeData>();
        TechnologyNodeData[] nodesData = Resources.LoadAll<TechnologyNodeData>(
            "ScriptableObjects/TechnologyTree");

        // (record parents for further re-assign)
        Dictionary<string, string> parents = new Dictionary<string, string>();
        foreach (TechnologyNodeData nodeData in nodesData)
        {
            // (reset unlocked flag, because the Unity Editor keeps the changes)
            nodeData._unlocked = false;

            TECH_TREE_NODES[nodeData.code] = nodeData;
            foreach (TechnologyNodeData child in nodeData.children)
                parents[child.code] = nodeData.code;
        }

        // re-assign parents after all nodes have been loaded and referenced
        foreach (KeyValuePair<string, string> p in parents)
            TECH_TREE_NODES[p.Key]._parent = TECH_TREE_NODES[p.Value];

        // auto-unlock root
        TECH_TREE_NODES[ROOT_NODE_CODE]._unlocked = true;
    }

    public static string[] GetUnlockedNodeCodes()
    {
        List<string> unlockedCodes = new List<string>();
        foreach (KeyValuePair<string, TechnologyNodeData> p in TECH_TREE_NODES)
            if (p.Value._unlocked)
                unlockedCodes.Add(p.Key);
        return unlockedCodes.ToArray();
    }

    public static void SetUnlockedNodes(string[] unlockedNodeCodes)
    {
        foreach (string code in unlockedNodeCodes)
            if (TECH_TREE_NODES.ContainsKey(code))
                TECH_TREE_NODES[code].Unlock();
    }

    public void Unlock()
    {
        // parent node not unlocked: this node is unreachable - abort!
        if (_parent != null && !_parent._unlocked) return;
        // node already unlocked - abort!
        if (_unlocked) return;

        _unlocked = true;
        TechnologyNodeActioners.Apply(code);
    }

}
