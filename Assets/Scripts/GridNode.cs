using System;
using Shapes;
using UnityEngine;

namespace DefaultNamespace
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