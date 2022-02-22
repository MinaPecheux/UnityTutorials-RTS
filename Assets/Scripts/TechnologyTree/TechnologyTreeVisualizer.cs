using System.Collections.Generic;
using UnityEngine;

public class TechnologyTreeVisualizer
{
    private static Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> _NODES_MAPPING;

    private static void _PrepareNodeVisualizer_Rec(TechnologyNodeData node, int depth = 0)
    {
        _NODES_MAPPING[node] = new TechnologyNodeVisualizer(node, depth);
        foreach (TechnologyNodeData child in node.children)
        {
            _PrepareNodeVisualizer_Rec(child, depth + 1);
        }
    }

    private static void _DrawNode_Rec(
        TechnologyNodeData node,
        Transform parent,
        float xOffset)
    {
        _NODES_MAPPING[node].Draw(parent, xOffset);
        foreach (TechnologyNodeData child in node.children)
            _DrawNode_Rec(child, parent, xOffset);
    }

    public static Dictionary<string, UnityEngine.UI.Image> DrawTree(Transform parent, float viewVidth)
    {
        TechnologyNodeData root = TechnologyNodeData.TECH_TREE_NODES[
            TechnologyNodeData.ROOT_NODE_CODE];

        if (_NODES_MAPPING != null)
            _NODES_MAPPING.Clear();
        else
            _NODES_MAPPING = new Dictionary<TechnologyNodeData, TechnologyNodeVisualizer>();
        _PrepareNodeVisualizer_Rec(root);

        TechnologyTreeLayouter.ComputeInitialXPosition(_NODES_MAPPING, root);
        TechnologyTreeLayouter.ComputeFinalXPosition(_NODES_MAPPING, root, 0);

        Vector2 contentsSize = TechnologyTreeLayouter.GetBoundingBox(_NODES_MAPPING);
        float xOffset = (viewVidth - contentsSize.x) / 2f;
        TechnologyTreeLayouter.DrawEdges(_NODES_MAPPING, root, parent, xOffset);
        _DrawNode_Rec(root, parent, xOffset);

        Dictionary<string, UnityEngine.UI.Image> progressBars =
            new Dictionary<string, UnityEngine.UI.Image>();
        foreach (KeyValuePair<TechnologyNodeData, TechnologyNodeVisualizer> p in _NODES_MAPPING)
            progressBars[p.Key.code] = p.Value.ProgressBar;

        return progressBars;
    }
}
