using Shapes;
using UnityEngine;

namespace Core.Entity.Grid
{
    public class GridNode : MonoBehaviour
    {
        [SerializeField] private Disc disc;
        
        public float X;
        public float Y;

        public void Initialize(float x, float y, bool isPivotPoint)
        {
            X = x;
            Y = y;

            disc.enabled = isPivotPoint;
        }
    }
}