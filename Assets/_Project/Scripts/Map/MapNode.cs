using UnityEngine;
using System.Collections.Generic;

public enum NodeType
{
    Combat,
    Event,
    Rest,
    Boss
}

[System.Serializable]
public class MapNode
{
    public NodeType nodeType;
    public Vector2 position;
    public List<MapNode> outgoingNodes;

    [HideInInspector]
    public GameObject nodeGameObject;
    public bool isCompleted = false;

    public MapNode(NodeType type, Vector2 pos) {
        nodeType = type;
        position = pos;
        outgoingNodes = new List<MapNode>();
    }
}
