using System;
using System.Collections.Generic;
using csDelaunay;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class VoronoiGenerator : MonoBehaviour
{
    [SerializeField] private MeshRenderer tileRenderer;

    [SerializeField] private MeshFilter meshFilter;

    [SerializeField] private MeshCollider meshCollider;

    [SerializeField] private int edgeDensity;

    [SerializeField] private int polygonNumber;

    [SerializeField] private CyclesFinder cyclesFinder;
    
    private Dictionary<Vector2f, Site> sites;
    private List<Edge> edges;
    private Spline spline;

    private int iteration = 0;
    private int trackNumber = 0;

    // Start is called before the first frame update
    void Start()
    {
        // MultiplyVertices(edgeDensity);
        List<Vector2f> points = CreateRandomPoint();
        
        Rectf bounds = new Rectf(0,0,512,512);
        
        Voronoi voronoi = new Voronoi(points,bounds,5);

        sites = voronoi.SitesIndexedByLocation;
        edges = voronoi.Edges;
        
        cyclesFinder.SetVertices(edges);
        cyclesFinder.CalculateVertexDatas();
        
        spline = new Spline(cyclesFinder.track);
        
        DisplayVoronoiDiagram();

    }

    // Update is called once per frame
    void Update()
    {
        iteration += 1;
        if (iteration % 10 == 0)
        {
            trackNumber += 1;
            cyclesFinder.incrementTrack(trackNumber);
            spline = new Spline(cyclesFinder.track);
       
        }

        if (spline != null)
        {
            DisplayVoronoiDiagram();
        }
    }
    

    private void DisplayVoronoiDiagram()
    {
        Texture2D tx = new Texture2D(512, 512);
        foreach (KeyValuePair<Vector2f, Site> kv in sites)
        {
            tx.SetPixel((int) kv.Key.x, (int) kv.Key.y, Color.red);
            
        }
        
        foreach (Vector2f vert in cyclesFinder.track)
        {
            tx.SetPixel((int) vert.x-1, (int) vert.y-1, Color.blue);
            tx.SetPixel((int) vert.x-1, (int) vert.y+1, Color.blue);
            tx.SetPixel((int) vert.x+1, (int) vert.y-1, Color.blue);
            tx.SetPixel((int) vert.x+1, (int) vert.y+1, Color.blue);
        }

        foreach (Edge edge in edges)
        {
            if (edge.ClippedEnds == null)
            {
                continue;
            }

            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }
        
        for(int i=0;i<cyclesFinder.track.Count;i++)
        {
            if (i < cyclesFinder.track.Count - 1)
            {
                DrawLine(cyclesFinder.track[i], cyclesFinder.track[i + 1], tx, Color.red);
            }
            else
            {
                DrawLine(cyclesFinder.track[i], cyclesFinder.track[0], tx, Color.red);
            }
        }
        
        
        for(int i=0;i<spline.splinePoints.Count;i++)
        {
            if (i < spline.splinePoints.Count - 1)
            {
                DrawLine(spline.splinePoints[i], spline.splinePoints[i + 1], tx, Color.blue);
            }
            else
            {
                DrawLine(spline.splinePoints[i], spline.splinePoints[0], tx, Color.blue);
            }
        }

        tx.Apply();
        this.tileRenderer.material.mainTexture = tx;
    }

 

    private List<Vector2f> CreateRandomPoint()
    {
        List<Vector2f> points = new List<Vector2f>();
        for (int i = 0; i < polygonNumber; i++)
        {
            points.Add(new Vector2f(Random.Range(0, 512), Random.Range(0, 512)));
        }

        return points;
    }

    private void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0)
    {
        int x0 = (int) p0.x;
        int y0 = (int) p0.y;
        int x1 = (int) p1.x;
        int y1 = (int) p1.y;

        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;
        int err = dx - dy;

        while (true)
        {
            tx.SetPixel(x0 + offset, y0 + offset, c);

            if (x0 == x1 && y0 == y1) break;
            int e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }

    private class VertexData
    {
        Nullable<Double> distance;
        Nullable<int> step;

        public VertexData()
        {
            this.step = null;
            this.distance = null;
        }
    }
}