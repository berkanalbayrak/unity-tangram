using System.Collections.Generic;
using System.Linq;
using _3rdParty.git_amend;
using Core.Entity.Grid;
using Core.Entity.TangramPiece;
using Core.Managers;
using Data;
using UnityEngine;
using Utils;
using Voronoi;
using Cell = Voronoi.Cell;
using Random = UnityEngine.Random;

namespace Core.Loaders
{
    public class LevelLoader : MonoBehaviour
    {
        [SerializeField] private TangramPiece tangramPiecePrefab;

        public int numSites = 5;
        public Bounds bounds;

        private List<Point> _sites;
        private FortuneVoronoi _voronoi;
        private VoronoiGraph _graph;

        private EventBinding<GridGenerationCompleteEvent> _gridGenerationCompleteEventBinding;

        private GameGrid _grid;
        
        private static bool mode = false;

        private void Start()
        {
            _sites = new List<Point>();
            _voronoi = new FortuneVoronoi();
            Test();   
        }

        private void OnEnable()
        {
            _gridGenerationCompleteEventBinding = new EventBinding<GridGenerationCompleteEvent>(OnGridGenerationComplete);
            EventBus<GridGenerationCompleteEvent>.Register(_gridGenerationCompleteEventBinding);
        }
    
        private void OnDisable()
        {
            EventBus<GridGenerationCompleteEvent>.Deregister(_gridGenerationCompleteEventBinding);
        }

        private void Test()
        {
            var levelParams = new LevelParametersDTO { GridSize = 4, PieceAmount = numSites };
            var generateLevelEvent = new GenerateLevelEvent { LevelParameters = levelParams };

            EventBus<GenerateLevelEvent>.Raise(generateLevelEvent);
        }

        private void Update()
        {
        
            // if (Input.GetKeyDown(KeyCode.R))
            // {
            //     RelaxSites(1);
            // }
            // if (Input.GetKeyDown(KeyCode.G))
            // {
            //     CreateSites(true, false);
            // }
            //
            // if (Input.GetKeyDown(KeyCode.P))
            //     mode = !mode;

        }

        private void OnGridGenerationComplete(GridGenerationCompleteEvent @event)
        {
            _grid = @event.GameGrid;
            CreateSites(true, false);
            RelaxSites(1);
            GeneratePieces(_grid);
        }

        private void GeneratePieces(GameGrid gameGrid)
        {
            foreach (var cell in _graph.cells)
            {
                var vertices = new List<Vector2>();
                foreach (HalfEdge halfEdge in cell.halfEdges)
                {
                    Edge edge = halfEdge.edge;

                    if (edge.va || edge.vb)
                    {
                        Gizmos.color = Color.red;

                        var newVertexA = edge.va.ToVector3().SnapToGrid(gameGrid.Spacing);
                        var newVertexB = edge.vb.ToVector3().SnapToGrid(gameGrid.Spacing);

                        Debug.Log(newVertexA);
                        Debug.Log(newVertexB);
                    
                        if (!vertices.Contains(newVertexA))
                            vertices.Add(newVertexA);

                        if (!vertices.Contains(newVertexB))
                            vertices.Add(newVertexB);

                    }

                    vertices.Sort((a, b) =>
                    {
                        var angleA = Mathf.Atan2(a.y - cell.site.y, a.x - cell.site.x);
                        var angleB = Mathf.Atan2(b.y - cell.site.y, b.x - cell.site.x);
                        return angleA.CompareTo(angleB);
                    });
                }

                var newTangramPiece = Instantiate(tangramPiecePrefab, Vector3.zero, Quaternion.identity);
                newTangramPiece.InitializePiece(gameGrid, vertices.ToArray(), new Vector3(cell.site.x, cell.site.y, 0), ColorManager.Instance.GetAvailableColor());
            }
        }

        private void Compute(List<Point> sites)
        {
            this._sites = sites;
            this._graph = this._voronoi.Compute(sites, this.bounds);
        }

        private void CreateSites(bool clear = true, bool relax = false, int relaxCount = 2)
        {
            List<Point> sites = new List<Point>();
            if (!clear)
            {
                sites = this._sites.Take(this._sites.Count).ToList();
            }

            // create vertices
            for (int i = 0; i < numSites; i++)
            {
                Point site = new Point(Random.Range(bounds.min.x, bounds.max.x), Random.Range(bounds.min.y, bounds.max.y),
                    0);
                sites.Add(site);
            }

            Compute(sites);

            if (relax)
            {
                RelaxSites(relaxCount);
            }
        }

        private void RelaxSites(int iterations)
        {
            for (int i = 0; i < iterations; i++)
            {
                if (!this._graph)
                {
                    return;
                }

                Point site;
                List<Point> sites = new List<Point>();
                float dist = 0;

                float p = 1 / _graph.cells.Count * 0.1f;

                for (int iCell = _graph.cells.Count - 1; iCell >= 0; iCell--)
                {
                    Voronoi.Cell cell = _graph.cells[iCell];
                    float rn = Random.value;

                    // probability of apoptosis
                    if (rn < p)
                    {
                        continue;
                    }

                    site = CellCentroid(cell);
                    dist = Distance(site, cell.site);

                    // don't relax too fast
                    if (dist > 2)
                    {
                        site.x = (site.x + cell.site.x) / 2;
                        site.y = (site.y + cell.site.y) / 2;
                    }

                    // probability of mytosis
                    if (rn > (1 - p))
                    {
                        dist /= 2;
                        sites.Add(new Point(site.x + (site.x - cell.site.x) / dist,
                            site.y + (site.y - cell.site.y) / dist));
                    }

                    sites.Add(site);
                }

                Compute(sites);
            }
        }

        static float Distance(Point a, Point b)
        {
            float dx = a.x - b.x;
            float dy = a.y - b.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        private Point CellCentroid(Voronoi.Cell cell)
        {
            float x = 0f;
            float y = 0f;
            Point p1, p2;
            float v;

            for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
            {
                HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
                p1 = halfEdge.GetStartPoint();
                p2 = halfEdge.GetEndPoint();
                v = p1.x * p2.y - p2.x * p1.y;
                x += (p1.x + p2.x) * v;
                y += (p1.y + p2.y) * v;
            }

            v = CellArea(cell) * 6;
            return new Point(x / v, y / v);
        }

        private float CellArea(Voronoi.Cell cell)
        {
            float area = 0.0f;
            Point p1, p2;

            for (int iHalfEdge = cell.halfEdges.Count - 1; iHalfEdge >= 0; iHalfEdge--)
            {
                HalfEdge halfEdge = cell.halfEdges[iHalfEdge];
                p1 = halfEdge.GetStartPoint();
                p2 = halfEdge.GetEndPoint();
                area += p1.x * p2.y;
                area -= p1.y * p2.x;
            }

            area /= 2;
            return area;
        }

        private void OnDrawGizmos()
        {
            if (_graph)
            {
                foreach (Voronoi.Cell cell in _graph.cells)
                {
                    DrawCell(cell);
                }
            }

            Gizmos.color = Color.blue;
            // Draw a wire cube with the given bounds
            Gizmos.DrawWireCube(bounds.center, bounds.size);
        }

        private void DrawCell(Cell cell)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawCube(new Vector3(cell.site.x, cell.site.y), Vector3.one * 0.2f);

            if (cell.halfEdges.Count > 0)
            {
                foreach (HalfEdge halfEdge in cell.halfEdges)
                {
                    Edge edge = halfEdge.edge;

                    if (edge.va && edge.vb)
                    {
                        Gizmos.color = Color.red;
                        
                        var va = edge.va.ToVector3().SnapToGrid(_grid.Spacing);
                        var vb = edge.vb.ToVector3().SnapToGrid(_grid.Spacing);
                    
                        Gizmos.DrawLine(va, vb);
                    }
                }
            }
        }
    }
}   