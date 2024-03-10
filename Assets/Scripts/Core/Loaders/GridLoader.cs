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

        private const int TEST_GRID_SIZE = 3;
        private float maxSizeUnits = 4.25f;     
        private void Start()
        {
            var grid = Instantiate(gameGridPrefab, Vector3.zero, Quaternion.identity);
            grid.Initialize(TEST_GRID_SIZE, maxSizeUnits);
            grid.transform.position = new Vector3(-maxSizeUnits / 2, maxSizeUnits / 2, 0);
            EventBus<GridGenerationCompleteEvent>.Raise(new GridGenerationCompleteEvent{ GameGrid = grid });
        }
    }
}
