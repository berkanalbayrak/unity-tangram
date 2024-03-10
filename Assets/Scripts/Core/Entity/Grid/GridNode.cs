using Shapes;
using UnityEngine;

namespace Core.Entity.Grid
{
    public class GridNode : MonoBehaviour
    {
        [SerializeField] private Disc disc;
        
        public int X;
        public int Y;

        public void Initialize(int x, int y, bool isPivotPoint)
        {
            X = x;
            Y = y;

            disc.enabled = isPivotPoint;
        }
    }
}