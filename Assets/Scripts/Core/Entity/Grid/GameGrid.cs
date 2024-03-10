using System;
using System.Collections.Generic;
using System.Linq;
using _3rdParty.git_amend;
using Shapes;
using UnityEngine;

namespace Core.Entity.Grid
{
    public class GameGrid : MonoBehaviour
    {
        [SerializeField] private GridNode gridNodePrefab;
        [SerializeField] private Polyline borderPolyline;
        [SerializeField] private Quad fillQuad;

        public float Spacing { get; private set; }
        public List<GridNode> Nodes { get; private set; } = new List<GridNode>();
        private int _gridSize;
        
        private EventBinding<NextLevelEvent> _nextLevelBinding;

        private void OnEnable()
        {
            _nextLevelBinding = new EventBinding<NextLevelEvent>(OnNextLevel);
            EventBus<NextLevelEvent>.Register(_nextLevelBinding);
        }
        
        private void OnDisable()
        {
            EventBus<NextLevelEvent>.Deregister(_nextLevelBinding);
        }
        
        private void OnNextLevel(NextLevelEvent evt)
        {
            Destroy(gameObject);
        }

        public void Initialize(int gridExtent, float maxSize)
        {
            _gridSize = CalculateGridSize(gridExtent);
            Spacing = CalculateSpacing(maxSize);
            
            GameObject nodeParent = InitializeNodeParent();
            GenerateGridNodes(nodeParent);
            UpdateBorderPolyline();
            UpdateFillQuad();
        }

        private int CalculateGridSize(int gridExtent)
        {
            return (gridExtent * 2) + 1;
        }

        private float CalculateSpacing(float maxSize)
        {
            return maxSize / (_gridSize - 1);
        }

        private GameObject InitializeNodeParent()
        {
            var nodeParent = new GameObject("Nodes");
            nodeParent.transform.SetParent(transform);
            return nodeParent;
        }

        private void GenerateGridNodes(GameObject parent)
        {
            for (var y = 0; y < _gridSize; y++)
            {
                for (var x = 0; x < _gridSize; x++)
                {
                    var node = Instantiate(gridNodePrefab, new Vector3(x * Spacing, y * -Spacing, 0), Quaternion.identity, parent.transform);
                    node.Initialize(x, y, x % 2 != 0 && y % 2 != 0);
                    node.name = $"Node({x},{y})";
                    Nodes.Add(node);
                }
            }
        }

        private void UpdateBorderPolyline()
        {
            borderPolyline.points.Clear();
            var edgePoints = GetOrderedEdgeNodes();
            foreach (var point in edgePoints)
            {
                borderPolyline.AddPoint(point.transform.position);
            }
            borderPolyline.enabled = true;
        }

        private List<GridNode> GetOrderedEdgeNodes()
        {
            return Nodes.Where(IsEdgeNode).OrderBy(EdgeNodeSortOrder).ToList();
        }

        private bool IsEdgeNode(GridNode node)
        {
            return node.X == 0 || node.X == _gridSize - 1 || node.Y == 0 || node.Y == _gridSize - 1;
        }

        private int EdgeNodeSortOrder(GridNode node)
        {
            //Top edge left to right
            //Right edge top to bottom
            //Bottom edge right to left
            //Left edge bottom to top
            
            if (node.Y == 0) return node.X;
            if (node.X == _gridSize - 1) return node.Y + _gridSize;
            if (node.Y == _gridSize - 1) return 3 * _gridSize - node.X;
            return 4 * _gridSize - node.Y;
        }

        private void UpdateFillQuad()
        {
            var cornerNodes = GetCornerNodes();
            for (int i = 0; i < 4; i++)
            {
                fillQuad.SetQuadVertex(i, cornerNodes[i].transform.position);
            }
            fillQuad.enabled = true;
        }

        private List<GridNode> GetCornerNodes()
        {
            return new List<GridNode>
            {
                //Top-Left, Top-Right, Bottom-Right, Bottom-Left
                
                Nodes.First(p => p.X == 0 && p.Y == 0),
                Nodes.First(p => p.X == _gridSize - 1 && p.Y == 0),
                Nodes.First(p => p.X == _gridSize - 1 && p.Y == _gridSize - 1),
                Nodes.First(p => p.X == 0 && p.Y == _gridSize - 1)
            };
        }
    }
}