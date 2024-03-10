using UnityEngine;

namespace Utils
{
    public static class VoronoiExtensions
    {
        public static Vector3 SnapToGrid(this Vector3 position, float gridSpacing)
        {
            position.x = Mathf.Round(position.x / gridSpacing) * gridSpacing;
            position.y = Mathf.Round(position.y / gridSpacing) * gridSpacing;
            return position;
        }
    }
}