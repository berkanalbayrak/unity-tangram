using System.Collections.Generic;
using Core.Entity.Grid;
using Core.Entity.TangramPiece;
using Data;
using UnityEngine;

namespace _3rdParty.git_amend
{
    public interface IEvent {}

    #region LoadingEvents

    public struct GenerateLevelEvent : IEvent
    {
        public LevelParametersDTO LevelParameters { get; init; }
    }

    public struct LevelGenerationCompleteEvent : IEvent {}
    
    public struct GridGenerationCompleteEvent : IEvent
    {
        public GameGrid GameGrid { get; init; }
    }

    public struct PieceGenerationCompleteEvent : IEvent
    {
        public List<TangramPiece> GamePieces { get; init; }
    }

    #endregion

    #region GameEvents

    public struct LevelStartEvent : IEvent {}
    
    public struct PieceSnappedEvent : IEvent
    {
        public TangramPiece piece { get; init; }
    }
    
    public struct LevelCompletedEvent : IEvent {}
    
    #endregion
}