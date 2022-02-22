/* Adapted from:
 * https://rachel53461.wordpress.com/2014/04/20/algorithm-for-drawing-trees/
 * 
 * Graph layout utils for placing tree nodes using the Reingold-Tilford algorithm */
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class TechnologyTreeLayouter
{
    // shared variables
    private static GameObject _uiEdgePrefab;
    private static GameObject _UI_EDGE_PREFAB
    {
        get
        {
            if (_uiEdgePrefab == null)
            {
                _uiEdgePrefab = Resources.Load<GameObject>(
                    "Prefabs/UI/TechnologyTree/UITechnologyTreeEdge");
            }
            return _uiEdgePrefab;
        }
    }
    private static float NODE_WIDTH = 140f;
    private static float NODE_HEIGHT = 64f;
    private static float SIBLING_DISTANCE = 30f;
    private static float LEVEL_DISTANCE = 24f;
    private static float TREE_DISTANCE = 20f;

    public static (Vector2, Vector2) InitializeNodeVisualizer(int depth)
    {
        Vector2 position = new Vector2(0, depth * (NODE_HEIGHT + LEVEL_DISTANCE));
        Vector2 size = new Vector2(NODE_WIDTH, NODE_HEIGHT);
        return (position, size);
    }

    public static void ComputeInitialXPosition(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node
    )
    {
        foreach (TechnologyNodeData child in node.children)
            ComputeInitialXPosition(nodesMapping, child);

        // if there is a previous sibling in this set, 
        // set X to prevous sibling + designated distance
        int nodeIndex = node.Parent != null ? node.Parent.children.IndexOf(node) : 0;
        bool isLeftMost = nodeIndex == 0;

        // if there is no children
        if (node.children.Count == 0)
        {
            // check for previous sibling at this level
            // - if there is one: place on the right with given spacing
            // - else set X to 0 because node is the first
            if (!isLeftMost)
                nodesMapping[node].X =
                    nodesMapping[node.Parent.children[nodeIndex - 1]].X +
                    NODE_WIDTH +
                    SIBLING_DISTANCE;
            else
                nodesMapping[node].X = 0;
        }

        // if there is only 1 child
        else if (node.children.Count == 1)
        {
            // if it's the first node at this level,
            // set its X value to the one of the child
            if (isLeftMost)
                nodesMapping[node].X = nodesMapping[node.children[0]].X;
            else
            {
                nodesMapping[node].X =
                    nodesMapping[node.Parent.children[nodeIndex - 1]].X +
                    NODE_WIDTH +
                    SIBLING_DISTANCE;
                nodesMapping[node].Mod = nodesMapping[node].X - nodesMapping[node.children[0]].X;
            }
        }

        // if there is > 1 child
        else
        {
            TechnologyNodeVisualizer leftChild = nodesMapping[node.children[0]];
            TechnologyNodeVisualizer rightChild = nodesMapping[node.children[node.children.Count - 1]];
            float mid = (leftChild.X + rightChild.X) / 2f;

            if (isLeftMost)
                nodesMapping[node].X = mid;
            else
            {
                nodesMapping[node].X =
                    nodesMapping[node.Parent.children[nodeIndex - 1]].X +
                    NODE_WIDTH +
                    SIBLING_DISTANCE;
                nodesMapping[node].Mod = nodesMapping[node].X - mid;
            }
        }

        if (node.children.Count > 0 && !isLeftMost)
            _CheckForNodeConflicts(nodesMapping, node);
    }

    public static void ComputeFinalXPosition(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node,
        float modSum
    )
    {
        nodesMapping[node].X += modSum;
        modSum += nodesMapping[node].Mod;

        foreach (TechnologyNodeData child in node.children)
            ComputeFinalXPosition(nodesMapping, child, modSum);
    }

    public static void DrawEdges(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node,
        Transform parent,
        float xOffset)
    {
        if (node.children.Count == 0) return;

        GameObject edge;

        // vertical line from parent to children middle
        edge = GameObject.Instantiate(_UI_EDGE_PREFAB, parent);
        edge.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            nodesMapping[node].X + NODE_WIDTH / 2f + xOffset,
            -(nodesMapping[node].Y + NODE_HEIGHT));
        edge.GetComponent<UILineRenderer>().end = new Vector2(0f, -LEVEL_DISTANCE / 2f);

        // horizontal line above children
        float leftX = nodesMapping[node.children[0]].X;
        float rightX = nodesMapping[node.children[node.children.Count - 1]].X;
        edge = GameObject.Instantiate(_UI_EDGE_PREFAB, parent);
        edge.GetComponent<RectTransform>().anchoredPosition = new Vector2(
            leftX + NODE_WIDTH / 2f + xOffset,
            -(nodesMapping[node].Y + NODE_HEIGHT + LEVEL_DISTANCE / 2f));
        edge.GetComponent<UILineRenderer>().end = new Vector2(rightX - leftX, 0f);

        foreach (TechnologyNodeData child in node.children)
        {
            // vertical lines above each child
            edge = GameObject.Instantiate(_UI_EDGE_PREFAB, parent);
            edge.GetComponent<RectTransform>().anchoredPosition = new Vector2(
                nodesMapping[child].X + NODE_WIDTH / 2f + xOffset,
                -(nodesMapping[child].Y - LEVEL_DISTANCE / 2f));
            edge.GetComponent<UILineRenderer>().end = new Vector2(0f, -LEVEL_DISTANCE / 2f);
            DrawEdges(nodesMapping, child, parent, xOffset);

        }
    }

    public static Vector2 GetBoundingBox(Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping)
    {
        IEnumerable<float> xs = nodesMapping.Values.Select(x => x.X);
        IEnumerable<float> ys = nodesMapping.Values.Select(x => x.Y);

        return new Vector2(xs.Max() + NODE_WIDTH - xs.Min(), ys.Max() + NODE_HEIGHT - ys.Min());
    }

    private static void _CheckForNodeConflicts(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node
    )
    {
        float minDistance = NODE_WIDTH + TREE_DISTANCE;
        float shiftValue = 0f;

        Dictionary<float, float> nodeContour = new Dictionary<float, float>();
        _GetLeftContour(nodesMapping, node, 0, ref nodeContour);

        TechnologyNodeData sibling = node.Parent.children[0];
        while (sibling != null && sibling != node)
        {
            Dictionary<float, float> siblingContour = new Dictionary<float, float>();
            _GetRightContour(nodesMapping, sibling, 0, ref siblingContour);

            float step = NODE_HEIGHT + LEVEL_DISTANCE;
            for (
                float level = nodesMapping[node].Y + step;
                level <= Mathf.Min(siblingContour.Keys.Max(), nodeContour.Keys.Max());
                level += step
            )
            {
                float distance = nodeContour[level] - siblingContour[level];
                if (distance + shiftValue < minDistance)
                    shiftValue = minDistance - distance;
            }

            if (shiftValue > 0)
            {
                nodesMapping[node].X += shiftValue;
                nodesMapping[node].Mod += shiftValue;

                _CenterNodesBetween(nodesMapping, node, sibling);

                shiftValue = 0;
            }

            int index = sibling.Parent.children.IndexOf(sibling);
            sibling = sibling.Parent.children[index + 1];
        }
    }

    private static void _CenterNodesBetween(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData leftNode,
        TechnologyNodeData rightNode
    )
    {
        int leftIndex = leftNode.Parent.children.IndexOf(rightNode);
        int rightIndex = leftNode.Parent.children.IndexOf(leftNode);

        int nNodesBetween = rightIndex - leftIndex - 1;

        if (nNodesBetween > 0)
        {
            float distanceBetweenNodes = (nodesMapping[leftNode].X - nodesMapping[rightNode].X) / (nNodesBetween + 1);
            int count = 1;
            for (int i = leftIndex + 1; i < rightIndex; i++)
            {
                TechnologyNodeData middleNode = leftNode.Parent.children[i];

                float desiredX = nodesMapping[rightNode].X + (distanceBetweenNodes * count);
                float offset = desiredX - nodesMapping[middleNode].X;
                nodesMapping[middleNode].X += offset;
                nodesMapping[middleNode].Mod += offset;

                count++;
            }

            _CheckForNodeConflicts(nodesMapping, leftNode);
        }
    }

    private static void _GetLeftContour(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node,
        float modSum,
        ref Dictionary<float, float> values
    )
    {
        float Y = nodesMapping[node].Y;
        if (!values.ContainsKey(Y))
            values[Y] = nodesMapping[node].X + modSum;
        else
            values[Y] = Mathf.Min(values[Y], nodesMapping[node].X + modSum);

        modSum += nodesMapping[node].Mod;
        foreach (TechnologyNodeData child in node.children)
            _GetLeftContour(nodesMapping, child, modSum, ref values);
    }

    private static void _GetRightContour(
        Dictionary<TechnologyNodeData, TechnologyNodeVisualizer> nodesMapping,
        TechnologyNodeData node,
        float modSum,
        ref Dictionary<float, float> values
    )
    {
        float Y = nodesMapping[node].Y;
        if (!values.ContainsKey(Y))
            values[Y] = nodesMapping[node].X + modSum;
        else
            values[Y] = Mathf.Max(values[Y], nodesMapping[node].X + modSum);

        modSum += nodesMapping[node].Mod;
        foreach (TechnologyNodeData child in node.children)
            _GetRightContour(nodesMapping, child, modSum, ref values);
    }

    private static TechnologyNodeData _GetLeftmostLeaf(TechnologyNodeData node)
    {
        if (node.children.Count == 0) return node;
        return _GetLeftmostLeaf(node.children[0]);
    }

    private static TechnologyNodeData _GetRightmostLeaf(TechnologyNodeData node)
    {
        if (node.children.Count == 0) return node;
        return _GetRightmostLeaf(node.children[node.children.Count - 1]);
    }

    private static int _GetDepth(TechnologyNodeData node)
    {
        if (node.children.Count == 0) return 1;
        return 1 + Mathf.Max(node.children.Select(x => _GetDepth(x)).ToArray());
    }
}
