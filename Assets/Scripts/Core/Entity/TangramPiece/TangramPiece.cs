using System.Collections.Generic;
using _3rdParty.git_amend;
using Core.Entity.Grid;
using Shapes;
using UnityEngine;
using Utils;

namespace Core.Entity.TangramPiece
{
    [RequireComponent(typeof(Polygon))]
    public class TangramPiece : MonoBehaviour
    {
        private GameGrid _grid; 
        private Polygon _polygon;
        private PolygonCollider2D _polygonCollider;
        private List<Vector3> _snapPointOffsets = new List<Vector3>();
        
        private GameObject localCentroidObject;
        private Vector3 gridCentroidPos;

        public bool IsDraggable { get; private set; }
        public bool IsInCorrectPlace { get; private set; }

        private static int NextPieceSortingOrder = 1;
    
        private const float SnapThreshold = 1f;
        private const int DefaultSortingOrder = 0;

        private void Awake()
        {
            SetupComponentReferences();
        }

        private void SetupComponentReferences()
        {
            _polygon = GetComponent<Polygon>();
            _polygonCollider = GetComponent<PolygonCollider2D>();
            ResetPieceProperties();
        }

        public void InitializePiece(global::Core.Entity.Grid.GameGrid gameGrid, Vector2[] points, Vector3 centroidSpot, Color color)
        {
            _grid = gameGrid;
            ResetPieceProperties();
            ConfigurePolygon(points, color);
            InitializeCentroid(centroidSpot);
            IdentifySnapPoints();
        }

        private void IdentifySnapPoints()
        {
            const float RayStartZOffset = -5f; // Offset for the Z-axis to start the raycast behind the object
            const float RaycastRadius = 0.1f; // Radius for the circle cast, adjust based on your needs
            const float RaycastDistance = 10f; // Max distance for the raycast, adjust as needed

            // Setup for the raycast layer and mask
            var raycastLayer = LayerMask.NameToLayer("PieceSetup");
            var raycastLayerMask = 1 << raycastLayer;

            // Temporarily change the gameObject's layer to avoid self-collision
            var originalLayer = gameObject.layer;
            gameObject.layer = raycastLayer;
        
            var rayDirection = Vector3.forward;

            // Iterate through each grid node to check for potential snap points
            foreach (var node in _grid.nodes)
            {
                // Create the start position for the raycast from the node's position
                var rayStart = new Vector3(node.transform.position.x, node.transform.position.y, RayStartZOffset);

                var hit = Physics2D.CircleCast(rayStart, RaycastRadius, rayDirection, RaycastDistance, raycastLayerMask);

                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    _snapPointOffsets.Add(node.transform.position - transform.position);
                }
            }

            // Restore the original layer of the gameObject
            gameObject.layer = originalLayer;
        }


        private void ResetPieceProperties()
        {
            ResetPolygon();
            _polygonCollider.enabled = false;
            _snapPointOffsets.Clear();
        }

        private void ConfigurePolygon(Vector2[] points, Color color)
        {
            _polygon.points = new List<Vector2>(points);
            _polygon.Color = color;
            _polygon.enabled = true;
            _polygonCollider.points = points;
            _polygonCollider.enabled = true;
        }
    
        private void ResetPolygon()
        {
            _polygon.points.Clear();
            _polygon.Color = Color.black;
            _polygon.enabled = false;
            _polygon.SortingOrder = DefaultSortingOrder;
        }

        private void InitializeCentroid(Vector3 centroidSpot)
        {
            gridCentroidPos = centroidSpot;
            CreateCentroidObject(centroidSpot);
        }

        private void CreateCentroidObject(Vector3 centroidSpot)
        {
            localCentroidObject = new GameObject("Centroid");
            localCentroidObject.transform.position = centroidSpot;
            localCentroidObject.transform.SetParent(transform);
        }
    
        public void OnPickedUp()
        {
            _polygon.SortingOrder = NextPieceSortingOrder++;
        }

        public void OnDropped()
        {
            TrySnapToGrid(_grid, SnapThreshold);
        }

        private void TrySnapToGrid(Grid.GameGrid gameGrid, float snapThreshold)
        {
            List<Vector2> newSnapPoints = DetermineNewSnapPoints(gameGrid, snapThreshold);

            if (newSnapPoints.Count == _snapPointOffsets.Count)
            {
                AdjustPositionBasedOnSnapPoints(newSnapPoints, out var activeAdjustment);
                UpdateLayerBasedOnOverlap(activeAdjustment);
            }
        }

        private List<Vector2> DetermineNewSnapPoints(Grid.GameGrid gameGrid, float snapThreshold)
        {
            List<Vector2> newSnapPoints = new List<Vector2>();

            foreach (Vector3 snapPointOffset in _snapPointOffsets)
            {
                Vector2 worldSnapPoint = transform.position + snapPointOffset;
                Vector2? closestNode = GridUtils.FindClosestGridNodePosition(worldSnapPoint, gameGrid.nodes, snapThreshold);

                if (!closestNode.HasValue)
                {
                    return new List<Vector2>(); // Empty list indicates no snap
                }

                newSnapPoints.Add(closestNode.Value);
            }

            return newSnapPoints;
        }

        private void AdjustPositionBasedOnSnapPoints(List<Vector2> newSnapPoints, out Vector3 adjustment)
        {
            Vector2 totalOffset = Vector2.zero;
            for (int i = 0; i < _snapPointOffsets.Count; i++)
            {
                Vector2 currentWorldSnapPoint = transform.position + (Vector3)_snapPointOffsets[i];
                totalOffset += newSnapPoints[i] - currentWorldSnapPoint;
            }
            adjustment = totalOffset / _snapPointOffsets.Count;
            transform.position += adjustment;
        }

        private void UpdateLayerBasedOnOverlap(Vector3 activeAdjustment)
        {
            if (IsOverlapping())
            {
                RestorePositionAndLayer(activeAdjustment);
            }
            else
            {
                //Valid Snap
                IsInCorrectPlace = IsSnappedToCorrectPlace();
                gameObject.layer = LayerMask.NameToLayer("SnappedPiece");
                EventBus<PieceSnappedEvent>.Raise(new PieceSnappedEvent { piece = this });
            }
        }

        private bool IsSnappedToCorrectPlace()
        {
            var centroidEquality = gridCentroidPos == localCentroidObject.transform.position;
            Debug.Log($"Centroid Equality: {centroidEquality}");
            return centroidEquality;
        }
        
        private void RestorePositionAndLayer(Vector3 activeAdjustment)
        {
            transform.position -= activeAdjustment;
            gameObject.layer = LayerMask.NameToLayer("Default");
        }

    
        private bool IsOverlapping()
        {
            var snappedPieceLayerMask = 1 << LayerMask.NameToLayer("SnappedPiece");
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var isOverlapping = CheckForOverlap(snappedPieceLayerMask);

            gameObject.layer = LayerMask.NameToLayer("Default");
            return isOverlapping;
        }

        private bool CheckForOverlap(int layerMask)
        {
            const float scaleFactor = 0.6f;
            ScaleObjectAroundCenter(scaleFactor);

            foreach (Vector3 snapPoint in _snapPointOffsets)
            {
                var rayStart = transform.position + (snapPoint * scaleFactor);
                if (Physics2D.CircleCast(rayStart, 0.1f * scaleFactor, Vector3.forward, 5, layerMask).collider != null)
                {
                    ScaleObjectAroundCenter(1); // Undo scaling
                    return true;
                }
            }

            ScaleObjectAroundCenter(1); // Undo scaling if no overlap found
            return false;
        }

        private static Vector2 CalculateCentroid(List<Vector2> vertices)
        {
            var sum = Vector2.zero;

            foreach (var vertex in vertices)
            {
                sum += vertex;
            }

            return sum / vertices.Count;
        }

        private void ScaleObjectAroundCenter(float scaleFactor)
        {
            // Calculate the centroid
            Vector3 centroid = CalculateCentroid(new List<Vector2>(_polygon.points));

            // Convert centroid to world space (since the vertices are in local space)
            Vector3 worldCentroid = transform.TransformPoint(centroid);

            // Scale the object
            transform.localScale = Vector3.one * scaleFactor;

            // After scaling, the centroid might not be in the original position relative to the world
            // So, we calculate its new position
            var newWorldCentroid = transform.TransformPoint(centroid);

            // We move the object so that the centroid appears to stay in the same place
            transform.position += (worldCentroid - newWorldCentroid);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(localCentroidObject.transform.position, 0.1f);

            foreach (var point in _snapPointOffsets)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(transform.position * transform.localScale.x, 0.1f);
            }
        
            Gizmos.DrawSphere(CalculateCentroid(_polygon.points), 0.1f);
        }
    }
}
