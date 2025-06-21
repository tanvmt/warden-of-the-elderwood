using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MapController : MonoBehaviour
{
    [Header("Map Config")]
    public int layers = 5;

    [Header("Node Prefabs")]
    public GameObject combatNodePrefab;
    public GameObject eventNodePrefab;
    public GameObject restNodePrefab;
    public GameObject bossNodePrefab;
    public LineRenderer linePrefab;
    public GameObject defaultEnemyPrefab;

    [Header("Containers")]
    public RectTransform nodeContainer;
    public Transform lineContainer;

    private List<List<MapNode>> map;
    private MapNode currentNode;

    void Start()
    {
        GenerateMap();
        DrawMap();
        SetStartingNode();
    }

    void GenerateMap()
    {
        map = new List<List<MapNode>>();

        for (int i = 0; i < layers; i++)
        {
            List<MapNode> layerNodes = new List<MapNode>();
            int nodesInLayer = UnityEngine.Random.Range(2, 4);

            for (int j = 0; j < nodesInLayer; j++)
            {
                float x = (j / (float)(nodesInLayer - 1) - 0.5f) * 400f;
                float y = i * 150f - (layers * 150f / 2f);

                MapNode node = new MapNode(NodeType.Combat, new Vector2(x, y));
                layerNodes.Add(node);
            }
            map.Add(layerNodes);
        }

        for (int i = 0; i < map.Count - 1; i++)
        {
            foreach (MapNode node in map[i])
            {
                int connections = UnityEngine.Random.Range(1, 3);
                for (int j = 0; j < connections; j++)
                {
                    MapNode nextNode = map[i + 1][UnityEngine.Random.Range(0, map[i + 1].Count)];
                    if (!node.outgoingNodes.Contains(nextNode))
                    {
                        node.outgoingNodes.Add(nextNode);
                    }
                }
            }
        }
    }

    void DrawMap()
    {
        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                GameObject prefab;
                switch (node.nodeType)
                {
                    case NodeType.Event:
                        prefab = eventNodePrefab;
                        break;
                    case NodeType.Rest:
                        prefab = restNodePrefab;
                        break;
                    case NodeType.Boss:
                        prefab = bossNodePrefab;
                        break;
                    default:
                        prefab = combatNodePrefab;
                        break;
                }

                GameObject nodeGO = Instantiate(prefab, nodeContainer);
                nodeGO.GetComponent<RectTransform>().anchoredPosition = node.position;
                node.nodeGameObject = nodeGO;

                Button button = nodeGO.GetComponent<Button>();
                button.onClick.AddListener(() => OnNodeClicked(node));
            }
        }

        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                foreach (var outgoingNode in node.outgoingNodes)
                {
                    LineRenderer line = Instantiate(linePrefab, lineContainer);

                    line.SetPosition(0, node.nodeGameObject.transform.position);
                    line.SetPosition(1, outgoingNode.nodeGameObject.transform.position);
                }
            }
        }
    }

    void SetStartingNode()
    {
        currentNode = null;
        UpdateNodeStates();
    }

    void OnNodeClicked(MapNode clickedNode)
    {
        bool isFirstMove = currentNode == null && map[0].Contains(clickedNode);
        bool isConnected = currentNode != null && currentNode.outgoingNodes.Contains(clickedNode);

        if (isFirstMove || isConnected)
        {
            currentNode = clickedNode;
            currentNode.isCompleted = true;

            switch (clickedNode.nodeType)
            {
                case NodeType.Combat:
                    Debug.Log($"Entering combat at node {clickedNode.position}");
                    GameManager.Instance.enemyToBattle = defaultEnemyPrefab;
                    SceneManager.LoadScene("CombatScene");
                    break;
                case NodeType.Event:
                    Debug.Log($"Encountered event at node {clickedNode.position}");
                    UpdateNodeStates();
                    break;
                case NodeType.Rest:
                    Debug.Log($"Resting at node {clickedNode.position}");
                    UpdateNodeStates();
                    break;
                case NodeType.Boss:
                    Debug.Log($"Facing boss at node {clickedNode.position}");
                    UpdateNodeStates();
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Cannot move to this node. It is not connected to the current node.");
        }
    }

    void UpdateNodeStates()
    {
        foreach (var layer in map)
        {
            foreach (var node in layer)
            {
                Button button = node.nodeGameObject.GetComponent<Button>();
                bool isClickable = (currentNode == null && map[0].Contains(node)) ||
                                   (currentNode != null && currentNode.outgoingNodes.Contains(node));
                button.interactable = isClickable && !node.isCompleted;

                // Update visual state of the node
            }
        }
    }
}
