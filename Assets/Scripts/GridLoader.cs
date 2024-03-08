using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shapes;
using UnityEngine;

public class GridLoader : MonoBehaviour
{
    [SerializeField] private DefaultNamespace.Grid gridPrefab;
    
    private DefaultNamespace.Grid _grid;

    private const int TEST_GRID_SIZE = 3;
    private float maxSizeUnits = 4.25f;     
    private void Start()
    {
        var grid = Instantiate(gridPrefab, Vector3.zero, Quaternion.identity);
        grid.Initialize(TEST_GRID_SIZE, maxSizeUnits);
        grid.transform.position = new Vector3(-maxSizeUnits / 2, maxSizeUnits / 2, 0);
    }
}
