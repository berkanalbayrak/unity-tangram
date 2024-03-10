using System.Collections.Generic;
using Core.Entity.Grid;
using UnityEngine;

namespace Utils
{
    public static class GridUtils
    {
        public static Vector3? FindClosestGridNodePosition(Vector3 point, IEnumerable<GridNode> gridNodes, float snapThreshold)
        {
            Vector3? closestNodePosition = null;
            var closestDistance = snapThreshold;

            foreach (var node in gridNodes)
            {
                float distance = Vector3.Distance(point, node.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestNodePosition = node.transform.position;
                }
            }

            return closestNodePosition;
        }
    }
}