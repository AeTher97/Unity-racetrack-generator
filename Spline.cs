using System.Collections.Generic;

public class Spline
{
    // Start is called before the first frame update

    private List<Vector2f> basePoints;


    public List<Vector2f> splinePoints;

    public Spline(List<Vector2f> basePoints)
    {
        this.basePoints = new List<Vector2f>();
        this.basePoints.Add(basePoints[0]);
        this.basePoints.AddRange(basePoints);
        this.basePoints.Add(basePoints[basePoints.Count - 1]);
        this.splinePoints = new List<Vector2f>();
        GenerateSplinePoints();
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

        float tx = basePoints[p0].x * q1 + basePoints[p1].x * q2 + basePoints[p2].x * q3 + basePoints[p3].x * q4;
        float ty = basePoints[p0].y * q1 + basePoints[p1].y * q2 + basePoints[p2].y * q3 + basePoints[p3].y * q4;


        return new Vector2f(tx / 2, ty / 2);
    }

    private void GenerateSplinePoints()
    {
        for (float t = 0.0f; t < (float) basePoints.Count - 3.0f; t += 0.005f)
        {
            splinePoints.Add(GetSplinePoint(t));
        }
    }
}