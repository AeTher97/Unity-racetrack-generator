using System;
using System.Collections.Generic;
using UnityEngine;

public class Track
{
    public List<Vector2f> baseVerts;
    public double baseLength;
    public List<Vector2f> splinePoints;
    public List<Vector2f> splineGradients;


    public Track(List<Vector2f> track, int subdivisionLevel = 1)
    {
        baseVerts = new List<Vector2f>(track);
        splinePoints = new List<Vector2f>();
        splineGradients = new List<Vector2f>();
        CalculateLength();
        SubdivideTrack(subdivisionLevel);
        GenerateTrackSpline();
    }

    public void CalculateLength()
    {
        baseLength = 0;
        if (baseVerts.Count < 3)
        {
            return;
        }

        for (int i = 1; i < baseVerts.Count; i++)
        {
            baseLength += DistanceBetweenVerts(baseVerts[i - 1], baseVerts[i]);
            if (i == baseVerts.Count - 1)
            {
                baseLength += DistanceBetweenVerts(baseVerts[i], baseVerts[0]);
            }
        }
    }

    private void SubdivideTrack(int segmentsMultiplier = 1)
    {
        if (baseVerts.Count < 3)
        {
            return;
        }

        double averageDistance = 0;
        for (int i = 1; i < baseVerts.Count; i++)
        {
            double distance = DistanceBetweenVerts(baseVerts[i - 1], baseVerts[i]);
            averageDistance += distance;
            if (i == baseVerts.Count - 1)
            {
                distance = DistanceBetweenVerts(baseVerts[i], baseVerts[0]);
                averageDistance += distance;
            }
        }

        averageDistance = averageDistance / baseVerts.Count;
        List<Vector2f> newTrack = new List<Vector2f>();

        for (int i = 1; i <= baseVerts.Count; i++)
        {
            int lowerIndex = i - 1;
            newTrack.Add(baseVerts[lowerIndex]);
            if (i == baseVerts.Count)
            {
                i = 0;
            }

            double distance = DistanceBetweenVerts(baseVerts[lowerIndex], baseVerts[i]);
            if (distance > averageDistance)
            {
                int segments = (int) Math.Ceiling(distance / averageDistance) * segmentsMultiplier;
                float dx = baseVerts[i].x - baseVerts[lowerIndex].x;
                float dy = baseVerts[i].y - baseVerts[lowerIndex].y;

                dx = dx / segments;
                dy = dy / segments;
                for (int j = 0; j < segments - 1; j++)
                {
                    newTrack.Add(new Vector2f(baseVerts[lowerIndex].x + dx * (j + 1),
                        baseVerts[lowerIndex].y + dy * (j + 1)));
                }
            }

            if (i == 0)
            {
                break;
            }
        }
        newTrack.Insert(0,newTrack[newTrack.Count-1]);
        newTrack.Add(newTrack[1]);
        newTrack.Add(newTrack[2]);

        baseVerts = newTrack;
    }

    private double DistanceBetweenVerts(Vector2f a, Vector2f b)
    {
        float dx = b.x - a.x;
        float dy = b.y - a.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }


    private Vector2f GetSplinePoint(float t)

    {
        int p0, p1, p2, p3;

        p1 = (int) t + 1;
        p2 = p1 + 1;
        p3 = p2 + 1;
        p0 = p1 - 1;

        t = t - (int) t;

        float tt = t * t;
        float ttt = tt * t;

        float q1 = -ttt + 2.0f * tt - t;
        float q2 = 3.0f * ttt - 5.0f * tt + 2.0f;
        float q3 = -3.0f * ttt + 4.0f * tt + t;
        float q4 = ttt - tt;

        float tx = baseVerts[p0].x * q1 + baseVerts[p1].x * q2 + baseVerts[p2].x * q3 + baseVerts[p3].x * q4;
        float ty = baseVerts[p0].y * q1 + baseVerts[p1].y * q2 + baseVerts[p2].y * q3 + baseVerts[p3].y * q4;


        return new Vector2f(tx / 2, ty / 2);
    }

    private Vector2f GetSplineGradient(float t)

    {
        int p0, p1, p2, p3;

        p1 = (int) t + 1;
        p2 = p1 + 1;
        p3 = p2 + 1;
        p0 = p1 - 1;

        t = t - (int) t;

        float tt = t * t;
        float ttt = tt * t;

        float q1 = -3.0f * tt + 4.0f * t - 1;
        float q2 = 9.0f * tt - 10.0f * t;
        float q3 = -9.0f * tt + 8.0f * t + 1;
        float q4 = 3.0f * tt - 2.0f * t;

        float tx = baseVerts[p0].x * q1 + baseVerts[p1].x * q2 + baseVerts[p2].x * q3 + baseVerts[p3].x * q4;
        float ty = baseVerts[p0].y * q1 + baseVerts[p1].y * q2 + baseVerts[p2].y * q3 + baseVerts[p3].y * q4;

        return new Vector2f(tx / 2, ty / 2);
    }

    private void GenerateTrackSpline()
    {
        for (float t = 0.0f; t < (float) baseVerts.Count - 3.0f; t += 0.005f)
        {
            splinePoints.Add(GetSplinePoint(t));
            splineGradients.Add(GetSplineGradient(t));
        }
    }
}