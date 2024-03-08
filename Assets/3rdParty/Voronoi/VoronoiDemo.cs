using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using Voronoi;
using Cell = Voronoi.Cell;

public class VoronoiDemo : MonoBehaviour
{
    [SerializeField] private TangramPiece tangramPiecePrefab;

    public int numSites = 5;
    public Bounds bounds;

    private List<Point> sites;
    private FortuneVoronoi voronoi;
    private VoronoiGraph graph;

    // [SerializeField] private Polygon polygon;

    private static bool mode = false;

    void Start()
    {
        sites = new List<Point>();
        voronoi = new FortuneVoronoi();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreateSites(true, false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RelaxSites(1);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach (var cell in graph.cells)
            {
                var vertices = new List<Vector2>();
                foreach (HalfEdge halfEdge in cell.halfEdges)
                {
                    Edge edge = halfEdge.edge;

                    if (edge.va || edge.vb)
                    {
                        Gizmos.color = Color.red;

                        var newVertexA = new Vector2(Mathf.RoundToInt(edge.va.x), Mathf.RoundToInt(edge.va.y));
                        var newVertexB = new Vector2(Mathf.RoundToInt(edge.vb.x), Mathf.RoundToInt(edge.vb.y));

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
                newTangramPiece.InitializePiece(vertices.ToArray(), new Vector3(cell.site.x, cell.site.y, 0));
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
            mode = !mode;

    }

    void Compute(List<Point> sites)
    {
        this.sites = sites;
        this.graph = this.voronoi.Compute(sites, this.bounds);
    }

    void CreateSites(bool clear = true, bool relax = false, int relaxCount = 2)
    {
        List<Point> sites = new List<Point>();
        if (!clear)
        {
            sites = this.sites.Take(this.sites.Count).ToList();
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

    void RelaxSites(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (!this.graph)
            {
                return;
            }

            Point site;
            List<Point> sites = new List<Point>();
            float dist = 0;

            float p = 1 / graph.cells.Count * 0.1f;

            for (int iCell = graph.cells.Count - 1; iCell >= 0; iCell--)
            {
                Voronoi.Cell cell = graph.cells[iCell];
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

    float Distance(Point a, Point b)
    {
        float dx = a.x - b.x;
        float dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    Point CellCentroid(Voronoi.Cell cell)
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

    float CellArea(Voronoi.Cell cell)
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

    void OnDrawGizmos()
    {
        if (graph)
        {
            foreach (Voronoi.Cell cell in graph.cells)
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

                    Debug.Log(Mathf.RoundToInt(edge.va.x));

                    Gizmos.DrawLine(new Vector3(Mathf.RoundToInt(edge.va.x), Mathf.RoundToInt(edge.va.y), 0),
                        new Vector3(Mathf.RoundToInt(edge.vb.x), Mathf.RoundToInt(edge.vb.y), 0));
                }
            }
        }
    }
}   