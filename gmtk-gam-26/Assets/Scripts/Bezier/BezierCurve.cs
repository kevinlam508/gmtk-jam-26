using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;

    [HideInInspector]
    public Vector3 A, B;

    public float length;


    private void Awake()
    {
        A = startPoint.position;
        B = endPoint.position;

        length = CalculateCurveLength();
    }


    private void OnDrawGizmos()
    {
        if (!startPoint || !endPoint)
            return;

        A = startPoint.position;
        B = endPoint.position;

        Gizmos.color = Color.white;

        Vector3 lastPos = A;

        float resolution = 0.02f;

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
    }

    private Vector3 DeCasteljausAlgorithm(float t)
    {
        // Linear interpolation = lerp = (1 - t) * A + t * B
        // Could use Vector3.lerp(A, B, t)

        float oneMinusT = 1f - t;

        Vector3 Q = oneMinusT * A + t * B;

        return Q;
    }

    public virtual Vector3 BezPos(float time)
    {
        return DeCasteljausAlgorithm(time);
    }

    public virtual float CalculateCurveLength()
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
