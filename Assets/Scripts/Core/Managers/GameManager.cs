using System;
using System.Collections.Generic;
using _3rdParty.git_amend;
using Core.Entity.TangramPiece;
using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        private EventBinding<PieceGenerationCompleteEvent> _pieceGenerationCompleteEventBinding;
        private EventBinding<PieceSnappedEvent> _pieceSnappedEventBinding;

        private List<TangramPiece> _gamePieces = new List<TangramPiece>();
        
        private void OnEnable()
        {
            _pieceGenerationCompleteEventBinding =
                new EventBinding<PieceGenerationCompleteEvent>(OnPieceGenerationComplete);
            EventBus<PieceGenerationCompleteEvent>.Register(_pieceGenerationCompleteEventBinding);

            _pieceSnappedEventBinding = new EventBinding<PieceSnappedEvent>(OnPieceSnapped);
            EventBus<PieceSnappedEvent>.Register(_pieceSnappedEventBinding);
            
        }
        
        private void OnDisable()
        {
            EventBus<PieceGenerationCompleteEvent>.Deregister(_pieceGenerationCompleteEventBinding);
            EventBus<PieceSnappedEvent>.Deregister(_pieceSnappedEventBinding);
            
        }

        private void OnPieceGenerationComplete(PieceGenerationCompleteEvent @event)
        {
            _gamePieces = @event.GamePieces;
        }
        
        private void OnPieceSnapped(PieceSnappedEvent obj)
        {
            if (_gamePieces.TrueForAll(piece => piece.IsInCorrectPlace))
            {
                Debug.Log("Level Completed");
                EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
            }
        }
    }
}