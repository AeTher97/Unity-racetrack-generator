using System;
using System.Collections.Generic;
using System.Linq;
using csDelaunay;
using UnityEngine;

public class CyclesFinder
{
    private bool[,] adjacency;

    private List<Vector2f> vertices;

    private List<VertexData> vertexDatas;


    public CyclesFinder(List<Edge> edges)
    {
        SetVertices(edges);
    }

    private void SetVertices(List<Edge> edges)
    {
        float threshold = 0.5f;
        vertexDatas = new List<VertexData>();


        vertices = new List<Vector2f>();
        foreach (var edge in edges)
        {
            if (edge.ClippedEnds == null)
            {
                continue;
            }

            if (!vertices.Contains(edge.ClippedEnds[LR.LEFT]))
            {
                bool add = true;
                foreach (var vertex in vertices)
                {
                    if (IsWithinThreshold(vertex, edge.ClippedEnds[LR.LEFT], threshold))
                    {
                        edge.ClippedEnds[LR.LEFT] = vertex;
                        add = false;
                    }
                }

                if (add)
                {
                    vertices.Add(edge.ClippedEnds[LR.LEFT]);
                    vertexDatas.Add(new VertexData(vertexDatas.Count));
                }
            }

            if (!vertices.Contains(edge.ClippedEnds[LR.RIGHT]))
            {
                bool add = true;
                foreach (var vertex in vertices)
                {
                    if (IsWithinThreshold(vertex, edge.ClippedEnds[LR.RIGHT], threshold))
                    {
                        edge.ClippedEnds[LR.RIGHT] = vertex;
                        add = false;
                    }
                }

                if (add)
                {
                    vertices.Add(edge.ClippedEnds[LR.RIGHT]);
                    vertexDatas.Add(new VertexData(vertexDatas.Count));
                }
            }
        }

        adjacency = new bool[vertices.Count, vertices.Count];


        foreach (var edge in edges)
        {
            if (edge.ClippedEnds == null)
            {
                continue;
            }

            int indexOfFirstVert = vertices.IndexOf(edge.ClippedEnds[LR.LEFT]);
            int indexOfSecondVert = vertices.IndexOf(edge.ClippedEnds[LR.RIGHT]);

            adjacency[indexOfFirstVert, indexOfSecondVert] = true;
            adjacency[indexOfSecondVert, indexOfFirstVert] = true;
        }

        Debug.Log(adjacency);
    }

    private int CalculateVertexDatas(List<int> bannedStartPoints)
    {
        foreach(VertexData vertexData in vertexDatas)
        {
            vertexData.parentHistory = new List<int>();
            vertexData.stepFromHome = null;
        }
        int startingVertex = 0;
        for (int i = 0; i < vertices.Count; i++)
        {
            int connections = NumberOfConnections(i);

            if (connections == 3)
            {
                startingVertex = i;
                if (!bannedStartPoints.Contains(startingVertex))
                {
                    Debug.Log("Starting vertex " + startingVertex);
                    break;
                }
            }
        }

        int stepsFromHome = 0;
        vertexDatas[startingVertex].stepFromHome = stepsFromHome;
        List<int> processedVertices = new List<int>();
        processedVertices.Add(startingVertex);

        while (processedVertices.Count > 0)
        {
            List<int> newProcessedVertices = new List<int>();
            stepsFromHome++;
            foreach (var vertex in processedVertices)
            {
                for (int i = 0; i < vertices.Count; i++)
                {
                    if (adjacency[vertex, i] != null)
                    {
                        if (adjacency[vertex, i] && i != vertex)
                        {
                            if (vertexDatas[i].stepFromHome == null)
                            {
                                vertexDatas[i].stepFromHome = stepsFromHome;
                                vertexDatas[i].parentHistory = new List<int>(vertexDatas[vertex].parentHistory);
                                vertexDatas[i].parentHistory.Add(vertex);
                                newProcessedVertices.Add(i);
                            }
                        }
                    }
                }
            }

            processedVertices = newProcessedVertices;
        }

        return startingVertex;
    }

    public Track FindSuitableTrack()
    {
        List<int> bannedStartingPoints = new List<int>();
        int currentStartingPoint = CalculateVertexDatas(bannedStartingPoints);
        List<Track> tracks = FindPossibleTracks();

        while (tracks.Count == 0 && bannedStartingPoints.Count < vertices.Count)
        {
            Debug.Log("Cannot find tracks starting in this vertex, changing start vertex");
            bannedStartingPoints.Add(currentStartingPoint);
            currentStartingPoint = CalculateVertexDatas(bannedStartingPoints);
            tracks = FindPossibleTracks();
        }

        if (tracks.Count == 0)
        {
            Debug.Log("Cannot find track with this pool of voronoi cells, try with different settings");
            return null;
        }

        Debug.Log("Successfully found " + tracks.Count + " viable tracks");

        return tracks.OrderByDescending(i => i.baseLength).FirstOrDefault();
    }

    private List<Track> FindPossibleTracks()
    {
        List<List<int>> tracks = new List<List<int>>();
        List<Track> realTracks = new List<Track>();
        List<VertexData> copy = new List<VertexData>(vertexDatas);

        VertexData furthestVertex = copy.OrderByDescending(i => i.stepFromHome).FirstOrDefault();
        bool searching = true;

        while (searching)
        {
            List<int> neighbours = GetNeighbours(furthestVertex.index);

            foreach (var neighbour in neighbours)
            {
                if (vertexDatas[neighbour].parentHistory.Count > 1 && furthestVertex.parentHistory.Count > 1)
                {
                    if (vertexDatas[neighbour].parentHistory[1] != furthestVertex.parentHistory[1])
                    {
                        List<List<int>> possibleWays1 = FindPossibleWays(furthestVertex.index);
                        List<List<int>> possibleWays2 = FindPossibleWays(neighbour);

                        foreach (List<int> way1 in possibleWays1)
                        {
                            foreach (List<int> way2 in possibleWays2)
                            {
                                List<int> newTrack = new List<int>(way1);
                                for (int i = 0; i < way2.Count - 1; i++)
                                {
                                    newTrack.Add(way2[way2.Count - i - 1]);
                                }

                                if (newTrack.Count == newTrack.Distinct().Count())
                                {
                                    tracks.Add(newTrack);
                                }
                            }
                        }
                    }
                }
            }

            copy.Remove(furthestVertex);
            if (copy.Count == 1)
            {
                break;
            }

            furthestVertex = copy.OrderByDescending(i => i.stepFromHome).FirstOrDefault();
        }

        foreach (List<int> subTrack in tracks)
        {
            List<Vector2f> realTrack = new List<Vector2f>();
            foreach (var z in subTrack)
            {
                realTrack.Add(vertices[z]);
            }

            Track trackClass = new Track(realTrack);
            realTracks.Add(trackClass);
        }

        return realTracks;
    }

    private List<List<int>> FindPossibleWays(int index)
    {
        List<List<int>> results = new List<List<int>>();
        VertexData checkedVert = vertexDatas[index];

        List<int> vertsToCheck = new List<int>(checkedVert.parentHistory);
        vertsToCheck.Add(index);

        for (int i = vertsToCheck.Count - 1; i > -1; i--)
        {
            List<int> neighbours = GetNeighbours(vertsToCheck[i]);
            VertexData checkedSubVert = vertexDatas[vertsToCheck[i]];
            if (checkedSubVert.parentHistory.Count == 0)
            {
                continue;
            }

            List<int> historyWithoutLast =
                checkedSubVert.parentHistory.GetRange(0, checkedSubVert.parentHistory.Count - 1);

            foreach (int neighbour in neighbours)
            {
                VertexData neighbourVertexData = vertexDatas[neighbour];
                if (neighbourVertexData.parentHistory.Count == 0 || checkedVert.parentHistory.Count == 0)
                {
                    continue;
                }

                if (neighbourVertexData.parentHistory[0] == checkedVert.parentHistory[0]
                    && neighbourVertexData.parentHistory != historyWithoutLast)
                {
                    List<int> subResult = new List<int>();
                    subResult.AddRange(neighbourVertexData.parentHistory);
                    subResult.Add(neighbourVertexData.index);

                    for (int j = i; j < vertsToCheck.Count; j++)
                    {
                        subResult.Add(vertsToCheck[j]);
                    }

                    if (subResult.Count == subResult.Distinct().Count())
                    {
                        results.Add(subResult);
                    }
                }
            }
        }

        return results;
    }

    private int NumberOfConnections(int index)
    {
        int connections = 0;
        for (int j = 0; j < vertices.Count; j++)
        {
            if (adjacency[index, j] != null)
            {
                if (adjacency[index, j])
                {
                    connections++;
                }
            }
        }

        return connections;
    }

    private List<int> GetNeighbours(int index)
    {
        List<int> result = new List<int>();
        for (int j = 0; j < vertices.Count; j++)
        {
            if (adjacency[index, j] != null)
            {
                if (adjacency[index, j])
                {
                    result.Add(j);
                }
            }
        }

        return result;
    }


    private class VertexData
    {
        public int index;
        public List<int> parentHistory;
        public Nullable<int> stepFromHome;

        public VertexData(int index)
        {
            this.index = index;
            parentHistory = new List<int>();
            stepFromHome = null;
        }
    }


    private bool IsWithinThreshold(Vector2f a, Vector2f b, double threshold)
    {
        float xDistance = Mathf.Abs(a.x - b.x);
        float yDistance = Mathf.Abs(a.y - b.y);

        float distance = Mathf.Sqrt(xDistance * xDistance + yDistance * yDistance);
        return distance < threshold;
    }
}