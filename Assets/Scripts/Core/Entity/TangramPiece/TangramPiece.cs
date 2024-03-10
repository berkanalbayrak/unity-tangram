using System;
using System.Collections.Generic;
using _3rdParty.git_amend;
using Core.Entity.Grid;
using DG.Tweening;
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
        public bool Snapped { get; private set; }


        private static int NextPieceSortingOrder = 1;

        private const float MoveDuration = 1f;
        private const float SnapThreshold = 1f;
        private const int DefaultSortingOrder = 0;
        
        private EventBinding<NextLevelEvent> _nextLevelEventBinding;

        private void Awake()
        {
            SetupComponentReferences();
        }

        private void OnEnable()
        {
            _nextLevelEventBinding = new EventBinding<NextLevelEvent>(OnNextLevel);
            EventBus<NextLevelEvent>.Register(_nextLevelEventBinding);
        }

        private void OnNextLevel(NextLevelEvent obj)
        {
            Destroy(this.gameObject);
        }

        private void OnDisable()
        {
            EventBus<NextLevelEvent>.Deregister(_nextLevelEventBinding);
        }

        private void SetupComponentReferences()
        {
            _polygon = GetComponent<Polygon>();
            _polygonCollider = GetComponent<PolygonCollider2D>();
            ResetPieceProperties();
        }

        public void InitializePiece(GameGrid gameGrid, Vector2[] points, Vector3 centroidSpot, Color color, Vector3 startPosition, Vector3 movePosition)
        {
            _grid = gameGrid;
            ResetPieceProperties();
            ConfigurePolygon(points, color);
            InitializeCentroid(centroidSpot);
            IdentifySnapPoints();
            transform.position = startPosition;
            StartMoveTween(movePosition);
        }
        
        private void ResetPieceProperties()
        {
            ResetPolygon();
            _polygonCollider.enabled = false;
            _snapPointOffsets.Clear();
        }
        
        private void ResetPolygon()
        {
            _polygon.points.Clear();
            _polygon.Color = Color.black;
            _polygon.enabled = false;
            _polygon.SortingOrder = DefaultSortingOrder;
        }
        
        private void ConfigurePolygon(Vector2[] points, Color color)
        {
            _polygon.points = new List<Vector2>(points);
            _polygon.Color = color;
            _polygon.enabled = true;
            _polygonCollider.points = points;
            _polygonCollider.enabled = true;
        }
        
        private void IdentifySnapPoints()
        {
            const float RayStartZOffset = -5f; 
            const float RaycastRadius = 0.1f; 
            const float RaycastDistance = 10f; 
            
            var raycastLayer = LayerMask.NameToLayer("PieceSetup");
            var raycastLayerMask = 1 << raycastLayer;

            // Temporarily change the gameObject's layer to avoid self-collision
            var originalLayer = gameObject.layer;
            gameObject.layer = raycastLayer;
        
            var rayDirection = Vector3.forward;

            // Iterate through each grid node to check for potential snap points
            foreach (var node in _grid.Nodes)
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
        
        private void StartMoveTween(Vector3 movePosition)
        {
            transform.DOMove(movePosition, MoveDuration, false).SetEase(Ease.OutQuart);
        }
    
        public void OnPickedUp()
        {
            _polygon.SortingOrder = NextPieceSortingOrder++;
        }

        public void OnDropped()
        {
            TrySnapToGrid(_grid, _grid.Spacing / 2);
        }

        private void TrySnapToGrid(Grid.GameGrid gameGrid, float snapThreshold)
        {
            List<Vector2> newSnapPoints = DetermineNewSnapPoints(gameGrid, snapThreshold);

            if (newSnapPoints.Count == _snapPointOffsets.Count)
            {
                AdjustPositionBasedOnSnapPoints(newSnapPoints, out var activeAdjustment);
                UpdateLayerBasedOnOverlap(activeAdjustment);
            }
            else
            {
                InvalidSnap();
            }
        }

        private List<Vector2> DetermineNewSnapPoints(Grid.GameGrid gameGrid, float snapThreshold)
        {
            var newSnapPoints = new List<Vector2>();

            var allSnapPointsValid = true;

            foreach (var snapPointOffset in _snapPointOffsets)
            {
                Vector2 worldSnapPoint = transform.position + snapPointOffset;
                Vector2? closestNode = GridUtils.FindClosestGridNodePosition(worldSnapPoint, gameGrid.Nodes, snapThreshold);

                if (!closestNode.HasValue)
                {
                    allSnapPointsValid = false;
                    break; // One of the snap points is out of range, no need to continue
                }

                newSnapPoints.Add(closestNode.Value);
            }

            // Only return the new snap points if all of them are valid
            return allSnapPointsValid ? newSnapPoints : new List<Vector2>();
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
                InvalidSnap();
                RestorePositionAndLayer(activeAdjustment);
            }
            else
            {
                //Valid Snap
                Snapped = true;
                IsInCorrectPlace = IsSnappedToCorrectPlace();
                gameObject.layer = LayerMask.NameToLayer("SnappedPiece");
                EventBus<PieceSnappedEvent>.Raise(new PieceSnappedEvent { piece = this });
            }
        }

        private void InvalidSnap()
        {
            Snapped = false;
            IsInCorrectPlace = false;
            gameObject.layer = LayerMask.NameToLayer("Default");
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
