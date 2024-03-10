using System;
using _3rdParty.git_amend;
using Core.Entity.Grid;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Loaders
{
    public class GridLoader : MonoBehaviour
    {
        [SerializeField] private GameGrid gameGridPrefab;
        [SerializeField] private Transform gridParent;
    
        private GameGrid _gameGrid;

        private float maxSizeUnits = 4.25f;
        
        private EventBinding<GenerateLevelEvent> _generateLevelEventBinding;

        private void OnEnable()
        {
            _generateLevelEventBinding = new EventBinding<GenerateLevelEvent>(OnGenerateLevel);
            EventBus<GenerateLevelEvent>.Register(_generateLevelEventBinding);
        }
        
        private void OnDisable()
        {
            EventBus<GenerateLevelEvent>.Deregister(_generateLevelEventBinding);
        }

        private void OnGenerateLevel(GenerateLevelEvent @event)
        {
            GenerateGrid(@event.LevelParameters.GridSize);
        }

        private void GenerateGrid(int gridSize)
        {
            var grid = Instantiate(gameGridPrefab, Vector3.zero, Quaternion.identity);
            grid.Initialize(gridSize, maxSizeUnits);
            grid.transform.position = new Vector3(-maxSizeUnits / 2, maxSizeUnits / 2, 0);
            EventBus<GridGenerationCompleteEvent>.Raise(new GridGenerationCompleteEvent{ GameGrid = grid });
        }
    }
}
