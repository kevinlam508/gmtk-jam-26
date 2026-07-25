using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuadBezier : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    public Transform controlPoint;

    [Range(0, 1)]
    public float progress;

    [HideInInspector]
    Vector3 C;
    Vector3 A;
    Vector3 B;

    [SerializeField]
    private float length;

    private void Awake()
    {
        A = startPoint.position;
        B = controlPoint.position;
        C = endPoint.position;
    }

    private void OnDrawGizmos()
    {
        if (!startPoint || !controlPoint || !endPoint)
            return;

        A = startPoint.position;
        B = controlPoint.position;
        C = endPoint.position;

        float len = 0;
        
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
            len += Vector3.Distance(lastPos, newPos);

            Gizmos.DrawWireCube(lastPos, new Vector3(.01f, .01f, .01f));

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }


        //Also draw lines between the control points and endpoints
        Gizmos.color = Color.green;

        Gizmos.DrawLine(A, B);
        Gizmos.DrawLine(C, B);

        length = len;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(DeCasteljausAlgorithm(progress), .1f);
    }

    private Vector3 DeCasteljausAlgorithm(float t)
    {
        // Linear interpolation = lerp = (1 - t) * A + t * B
        // Could use Vector3.lerp(A, B, t)

        float oneMinusT = 1f - t;

        Vector3 Q = oneMinusT * A + t * B;
        Vector3 R = oneMinusT * B + t * C;

        Vector3 P = oneMinusT * Q + t * R;

        return P;
    }

    public float GetLength()
    {
        length = CalculateCurveLength();
        return length;
    }

    public Vector3 BezPos(float time)
    {
        A = startPoint.position;
        B = controlPoint.position;
        C = endPoint.position;

        return DeCasteljausAlgorithm(time);
    }

    public float CalculateCurveLength()
    {
        A = startPoint.position;
        B = controlPoint.position;
        C = endPoint.position;

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

            //Save this pos so we can draw the next line segment
            lastPos = newPos;
        }

        return len; 
    }
}
