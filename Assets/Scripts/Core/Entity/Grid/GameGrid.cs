using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

namespace Core.Entity.Grid
{
    public class GameGrid : MonoBehaviour
    {
        [SerializeField] private GridNode gridNodePrefab;
        [SerializeField] private Polyline borderPolyLine;
        [SerializeField] private Quad fillQuad;

        public float Spacing { get; private set; }
        
        public readonly List<GridNode> nodes = new List<GridNode>();
        private int _actualSize;

        public void Initialize(int relativeSize, float maxSizeUnits)
        {
            _actualSize = (relativeSize * 2) + 1;
            Spacing = maxSizeUnits / (_actualSize - 1);
            
            var nodeParentObject = new GameObject("Nodes");
            nodeParentObject.transform.SetParent(transform);
            
            for (var y = 0; y < _actualSize; y++)
            {
                for (var x = 0; x < _actualSize; x++)
                {
                    var newNodeName = $"Node({x},{y})";
                    var newNode = Instantiate(gridNodePrefab);
                    newNode.Initialize(x, y, x % 2 != 0 && y % 2 != 0);
                    newNode.transform.position = new Vector3(x * Spacing, y * -Spacing, 0);
                    newNode.transform.SetParent(nodeParentObject.transform);
                    newNode.name = newNodeName;
                    nodes.Add(newNode);
                }
            }
            
            borderPolyLine.points.Clear();

            var edgePoints = GetOrderedOutermostPoints();
            
            foreach (var edgePoint in edgePoints)
            {
                borderPolyLine.AddPoint(new Vector3(edgePoint.transform.position.x, edgePoint.transform.position.y, 0));
            }
        
            borderPolyLine.enabled = true;
            
            var cornerPoints = GetCornerPoints();
            
            for (int i = 0; i < 4; i++)
            {
                fillQuad.SetQuadVertex(i, cornerPoints[i].transform.position);
            }

            fillQuad.enabled = true;
        }
        
        public List<GridNode> GetOrderedOutermostPoints()
        {
            // First, filter out the outermost points
            var outerPoints = nodes.Where(p => p.X == 0 || p.X == _actualSize - 1 || p.Y == 0 || p.Y == _actualSize - 1).ToList();

            // Then, order them starting from the top-left corner and moving clockwise
            return outerPoints.OrderBy(p =>
            {
                if (p.Y == 0) return p.X; // Top edge, order left to right
                if (p.X == _actualSize - 1) return p.Y + _actualSize; // Right edge, order top to bottom
                if (p.Y == _actualSize - 1) return 3 * _actualSize - p.X; // Bottom edge, order right to left
                return 4 * _actualSize - p.Y; // Left edge, order bottom to top
            }).ToList();
        }

        private List<GridNode> GetCornerPoints()
        {
            var cornerPoints = new List<GridNode>
            {
                // Top-left
                nodes.First(p => p.X == 0 && p.Y == 0),
                // Top-right
                nodes.First(p => p.X == _actualSize - 1 && p.Y == 0),
                // Bottom-right
                nodes.First(p => p.X == _actualSize - 1 && p.Y == _actualSize - 1),
                // Bottom-left
                nodes.First(p => p.X == 0 && p.Y == _actualSize - 1)
            };

            return cornerPoints;
        }
    }
}