# Tangram Case Study For Unity

This repository showcases a Unity-based project featuring a procedurally generated Tangram game, created as a 3-day case study. The project reflects a dedicated and earnest effort to explore and understand Unity and procedural generation techniques. The game was developed using Unity 2022.3.15f1.

![Movie_001-ezgif com-video-to-gif-converter](https://github.com/berkanalbayrak/unity-tangram/assets/40705909/f99cde79-281a-4b66-8257-8ae7ab3aee23)

## Features

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

Each shape retains a reference to its centroid point and overlapping grid points, enabling precise grid snapping.


