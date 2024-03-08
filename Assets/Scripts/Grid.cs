using System;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

namespace DefaultNamespace
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private GridNode gridNodePrefab;
        [SerializeField] private Polyline polyLine;
        
        public readonly List<GridNode> nodes = new List<GridNode>();
        private int actualSize;

        public void Initialize(int relativeSize, float maxSizeUnits)
        {
            actualSize = (relativeSize * 2) + 1;
            Debug.Log(actualSize);
            var offsetPerNode = maxSizeUnits / (actualSize - 1);
            
            var nodeParentObject = new GameObject("Nodes");
            nodeParentObject.transform.SetParent(transform);
            
            for (var y = 0; y < actualSize; y++)
            {
                for (var x = 0; x < actualSize; x++)
                {
                    var newNodeName = $"Node({x},{y})";
                    var newNode = Instantiate(gridNodePrefab);
                    newNode.Initialize(x, y, x % 2 != 0 && y % 2 != 0);
                    newNode.transform.position = new Vector3(x * offsetPerNode, y * -offsetPerNode, 0);
                    newNode.transform.SetParent(nodeParentObject.transform);
                    newNode.name = newNodeName;
                    nodes.Add(newNode);
                }
            }
            
            polyLine.points.Clear();

            var edgePoints = GetOrderedOutermostPoints();
            
            foreach (var edgePoint in edgePoints)
            {
                polyLine.AddPoint(new Vector3(edgePoint.transform.position.x, edgePoint.transform.position.y, 0));
            }
        
            polyLine.enabled = true;
        }
        
        public List<GridNode> GetOrderedOutermostPoints()
        {
            // First, filter out the outermost points
            var outerPoints = nodes.Where(p => p.X == 0 || p.X == actualSize - 1 || p.Y == 0 || p.Y == actualSize - 1).ToList();

            // Then, order them starting from the top-left corner and moving clockwise
            return outerPoints.OrderBy(p =>
            {
                if (p.Y == 0) return p.X; // Top edge, order left to right
                if (p.X == actualSize - 1) return p.Y + actualSize; // Right edge, order top to bottom
                if (p.Y == actualSize - 1) return 3 * actualSize - p.X; // Bottom edge, order right to left
                return 4 * actualSize - p.Y; // Left edge, order bottom to top
            }).ToList();
        }
    }
}