using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubicBezier : BezierCurve
{
    // public Transform startPoint;
    // public Transform endPoint;
    public Transform controlPointStart;
    public Transform controlPointEnd;
     
    [HideInInspector]
    Vector3 C, D;

    private void Awake()
    {
        if (!startPoint || !controlPointStart || !controlPointEnd || !endPoint)
            return;

        A = startPoint.position;
        B = controlPointStart.position;
        C = controlPointEnd.position;
        D = endPoint.position;

        length = CalculateCurveLength();
    }


    private void OnDrawGizmos()
    {
        if (!startPoint || !controlPointStart || !controlPointEnd || !endPoint)
            return;

        A = startPoint.position;
        B = controlPointStart.position;
        C = controlPointEnd.position;
        D = endPoint.position;

        //The Bezier curve's color
        Gizmos.color = Color.white;

        //The start position of the line
        Vector3 lastPos = A;

        //The resolution of the line
        //Make sure the resolution is adding up to 1, so 0.3 will give a gap at the end, but 0.2 will work
        float resolution = 0.02f;

        // How many loops?
        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {
            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinates between the control points with a Catmull-Rom spline
            Vector3 newPos = DeCasteljausAlgorithm(t);

            //Draw this line segment
            Gizmos.DrawLine(lastPos, newPos);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }

        //Also draw lines between the control points and endpoints
        Gizmos.color = Color.green;

        Gizmos.DrawLine(A, B);
        Gizmos.DrawLine(C, D);
    }

    Vector3 DeCasteljausAlgorithm(float t)
    {
        // Linear interpolation = lerp = (1 - t) * A + t * B
        // Could use Vector3.lerp(A, B, t)

        float oneMinusT = 1f - t;

        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;
        Vector3 S = oneMinusT * C + t * D;

        Vector3 P = oneMinusT * Q + t * R;
        Vector3 T = oneMinusT * R + t * S;

        Vector3 U = oneMinusT * P + t * T;

        return U;
    }

    public override Vector3 BezPos(float time)
    {
        return DeCasteljausAlgorithm(time);
    }

    public override float CalculateCurveLength()
    {
        float len = 0;

        Vector3 lastPos = A;

        float resolution = 0.02f;

        int loops = Mathf.FloorToInt(1f / resolution);

        for (int i = 1; i <= loops; i++)
        {

            //Which t position are we at?
            float t = i * resolution;

            //Find the coordinates between the control points with a Catmull-Rom spline
            Vector3 newPos = DeCasteljausAlgorithm(t);

            len += Vector3.Distance(lastPos, newPos);
            Debug.Log("New Pos = " + newPos + " Last Pos = " + lastPos);

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }

        return len;
    }
}

