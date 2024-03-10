using System;
using System.Collections.Generic;
using System.Linq;
using _3rdParty.git_amend;
using Core.Entity.TangramPiece;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Managers
{
    public class GameManager : MonoBehaviour
    {
        private EventBinding<PieceGenerationCompleteEvent> _pieceGenerationCompleteEventBinding;
        private EventBinding<PieceSnappedEvent> _pieceSnappedEventBinding;

        public List<TangramPiece> _gamePieces = new List<TangramPiece>();
        
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
            _gamePieces = @event.GamePieces.ToList();
            StartLevelAsync();
        }
        
        private async UniTaskVoid StartLevelAsync()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2));
            EventBus<LevelStartEvent>.Raise(new LevelStartEvent());
        }
        
        private void OnPieceSnapped(PieceSnappedEvent obj)
        {
            var arePiecesInCorrectSpot = _gamePieces.Any() && _gamePieces.TrueForAll(piece => piece.IsInCorrectPlace);
            
            if (arePiecesInCorrectSpot || _gamePieces.TrueForAll(piece => piece.Snapped))
            {
                Debug.Log("Level Completed");
                EventBus<LevelCompletedEvent>.Raise(new LevelCompletedEvent());
            }
        }
    }
}