using Core.Entity.TangramPiece;
using UnityEngine;

namespace Core.Input
{
    public class PlayerDragHandler : MonoBehaviour
    {
        private Camera mainCamera;
        private TangramPiece currentDraggedPiece;
        private Vector2 offset;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleDragInput();
        }

        private void HandleDragInput()
        {
            if (UnityEngine.Input.GetMouseButtonDown(0))
            {
                TryStartDrag();
            }

            if (UnityEngine.Input.GetMouseButton(0) && currentDraggedPiece != null)
            {
                ContinueDrag();
            }

            if (UnityEngine.Input.GetMouseButtonUp(0))
            {
                EndDrag();
            }
        }

        private void TryStartDrag()
        {
            var mousePosition2D = mainCamera.ScreenToWorldPoint(UnityEngine.Input.mousePosition);
            var hit = Physics2D.Raycast(mousePosition2D, Vector2.zero);

            if (hit.collider != null && hit.collider.CompareTag("TangramPiece"))
            {
                currentDraggedPiece = hit.collider.GetComponent<TangramPiece>();
                offset = (Vector2)(currentDraggedPiece.transform.position - mainCamera.ScreenToWorldPoint(new Vector3(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, mainCamera.WorldToScreenPoint(currentDraggedPiece.transform.position).z)));
                currentDraggedPiece.OnPickedUp();
            }
        }

        private void ContinueDrag()
        {
            var mouseScreenPosition = new Vector3(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, mainCamera.WorldToScreenPoint(currentDraggedPiece.transform.position).z);
            var newWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition) + (Vector3)offset;
    
            newWorldPosition.z = currentDraggedPiece.transform.position.z;
            currentDraggedPiece.transform.position = newWorldPosition;
        }
    
        private void EndDrag()
        {
            if (currentDraggedPiece == null) return;
        
            currentDraggedPiece.OnDropped();
            currentDraggedPiece = null;
        }
    }
}

