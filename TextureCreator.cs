using System.Collections.Generic;
using csDelaunay;
using UnityEngine;

public static class TextureCreator
{
    public static Texture2D CreateVoronoiTexture(int textureSize, Dictionary<Vector2f, Site> sites,List<Edge> edges, Track track)
    {
        Texture2D tx = new Texture2D(textureSize, textureSize);
        foreach (KeyValuePair<Vector2f, Site> kv in sites)
        {
            tx.SetPixel((int) kv.Key.x, (int) kv.Key.y, Color.red);
        }

        foreach (Vector2f vert in track.baseVerts)
        {
            tx.SetPixel((int) vert.x - 1, (int) vert.y - 1, Color.red);
            tx.SetPixel((int) vert.x - 1, (int) vert.y + 1, Color.red);
            tx.SetPixel((int) vert.x + 1, (int) vert.y - 1, Color.red);
            tx.SetPixel((int) vert.x + 1, (int) vert.y + 1, Color.red);
        }

        foreach (Edge edge in edges)
        {
            if (edge.ClippedEnds == null)
            {
                continue;
            }

            DrawLine(edge.ClippedEnds[LR.LEFT], edge.ClippedEnds[LR.RIGHT], tx, Color.black);
        }


        for (int i = 0; i < track.splinePoints.Count; i++)
        {
            if (i < track.splinePoints.Count - 1)
            {
                DrawLine(track.splinePoints[i], track.splinePoints[i + 1], tx, Color.blue);
            }
            else
            {
                DrawLine(track.splinePoints[i], track.splinePoints[0], tx, Color.blue);
            }
        }

        tx.Apply();
        return tx;
        
    } 
    
    
    public static Texture2D CreateHeightTexture(float[,] map)
    {
        int tileDepth = map.GetLength(0);
        int tileWidth = map.GetLength(1);

        Color[] colorMap = new Color[tileDepth * tileWidth];
        for (int zIndex = 0; zIndex < tileDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < tileWidth; xIndex++)
            {
                int colorIndex = zIndex * tileWidth + xIndex;
                float height = map[zIndex, xIndex];


                colorMap[colorIndex] = Color.Lerp(Color.black, Color.white, height);
            }
        }

        Texture2D tileTexture = new Texture2D(tileWidth, tileDepth);
        tileTexture.wrapMode = TextureWrapMode.Clamp;
        tileTexture.SetPixels(colorMap);
        tileTexture.Apply();

        return tileTexture;
    }
    
    private static void DrawLine(Vector2f p0, Vector2f p1, Texture2D tx, Color c, int offset = 0)
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

}