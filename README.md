# Tangram Case Study For Unity

This repository showcases a Unity-based project featuring a procedurally generated Tangram, created as a 3-day case study inspired by [blocks.ovh](blocks.ovh). The project reflects a dedicated and earnest effort to explore and understand procedural generation techniques. Developed using Unity 2022.3.15f1.

![Movie_001-ezgif com-video-to-gif-converter](https://github.com/berkanalbayrak/unity-tangram/assets/40705909/f99cde79-281a-4b66-8257-8ae7ab3aee23)

## Code Structure

Below is an overview of the key components and their responsibilities:

### Entities
- 
  - `GameGrid.cs` - Manages the game grid, including its initialization and dynamic updates during gameplay.
  - `GridNode.cs` - Represents a single node within the game grid, holding relevant data and state.
- **TangramPiece**
  - `TangramPiece.cs` - Manages the behavior of individual Tangram pieces, including polygon data, snapping logic, and user interactions.

### Input
- `PlayerDragHandler.cs` - Handles the drag & drop mechanic for moving Tangram pieces on the grid.

### Loaders
- `GridLoader.cs` - Responsible for generating the game grid at runtime.
- `LevelLoader.cs` - Reads available level data and generates procedural levels based on predefined parameters, utilizing algorithms for Voronoi diagram generation and Lloyd's relaxation.

### Managers
- `ColorManager.cs` - Manages a pool of unique colors for differentiating Tangram pieces or grid elements.
- `GameManager.cs` - Coordinates game flow, including starting the game, monitoring progress, and determining level completion.

## Level Generation

### Level Configuration

Levels are defined using a JSON format for easy customization and scalability. Each level is specified with a level number, grid size, and piece amount. These parameters are loaded from `Resources/level_data.json` and deserialized into a collection of `LevelParametersDTO` classes.

Example of the JSON level format:
```json
[
    {"levelNumber": 1, "gridSize": 5, "pieceAmount": 6},
    {"levelNumber": 2, "gridSize": 6, "pieceAmount": 7},
    ...
]
```

### Voronoi Diagram Generation

This project incorporates a C# ported version of the JavaScript Voronoi library (https://github.com/gorhill/Javascript-Voronoi) accessible at https://github.com/jerstam/Unity-Voronoi. The library is used to select sites on the grid and generate Voronoi diagrams utilizing Fortune's algorithm, which involves a beach line and a sweep line for efficient computation.

Additionally, the edges of the Voronoi cells are snapped to the grid, enhancing the integration with the game's visual grid structure.

![Fortunes-Algorithm](https://github.com/berkanalbayrak/unity-tangram/assets/40705909/cf1f4940-a8f7-493b-a1b2-59e09d35e31b)
<br>
[Source - Wikipedia - Fortune's Algorithm](https://en.wikipedia.org/wiki/Fortune%27s_algorithm)


### Lloyd's Relaxation

To achieve a more uniform look of the Voronoi cells, one iteration of Lloyd's relaxation algorithm is applied. This process moves each site closer to the centroid of its corresponding cell.

![LLoyds-Relaxation](https://github.com/berkanalbayrak/unity-tangram/assets/40705909/e18273fb-cf7e-4ffb-b4ee-922b505b9809)
<br>
[Source - Wikipedia - LLoyd's Algorithm](https://en.wikipedia.org/wiki/Lloyd%27s_algorithm)


### Polygon Drawing and Sorting

The final step involves drawing the polygons formed by the Voronoi cells. This is done using the Shapes library and the built-in Ear Clipping triangulation for rendering. To ensure the vertices are ordered correctly, they are sorted clockwise based on their angle from the cell's site:

```csharp
vertices.Sort((a, b) =>
{
    var angleA = Mathf.Atan2(a.y - cell.site.y, a.x - cell.site.x);
    var angleB = Mathf.Atan2(b.y - cell.site.y, b.x - cell.site.x);
    return angleA.CompareTo(angleB);
});
```

Each shape retains a reference to its centroid point and overlapping grid points as an offset value, enabling precise grid snapping.

## Snapping

![Screenshot_20](https://github.com/berkanalbayrak/unity-tangram/assets/40705909/d3c04196-41da-4a67-88b4-45e71c6a373b)

The game includes a snapping feature that helps pieces align with the grid. It works by moving pieces close to their nearest grid point if they're within a certain distance.

Key steps include:
- Identifying potential snap points based on the piece's proximity to the grid.
- Trying snapping the piece.
- Handling overlap and out-of-range scenarios using CircleCast2D.
  
See `TangramPiece.cs` for detailed implementation. 

## Communication Between Modules

To facilitate communication between modules, the project utilizes a [struct-based EventBus system](https://github.com/adammyhre/Unity-Event-Bus).

# Closing Thoughts

Thank you for taking the time to explore this project! While I've laid the foundation, there's plenty of room for improvement and new ideas. Whether you're here to browse, contribute, or provide feedback, I'm grateful for your interest.
