using System;
using System.Collections;
using System.Collections.Generic;
using Shapes;
using UnityEngine;

[RequireComponent(typeof(Polygon))]
public class TangramPiece : MonoBehaviour
{
    private Vector3 _centroidSpot;
    private Polygon _polygon;
    
    public bool IsDraggable { get; private set; }

    private void Awake()
    {
        _polygon = GetComponent<Polygon>();
        _polygon.enabled = false;
    }
    
    public void InitializePiece(Vector2[] points, Vector3 centroidSpot)
    {
        _polygon.enabled = true;
        _centroidSpot = centroidSpot;
        _polygon.AddPoints(points);
        
        var centroid = new GameObject("Centroid");
        centroid.transform.position = centroidSpot;
        centroid.transform.SetParent(transform);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_centroidSpot, 0.1f);
    }
}
