using UnityEngine;

public class SecondOrderDynamics
{
    private Vector3 xp; // previous input
    private Vector3 y, yd; // state variables
    private float _w, _z, _d, k1, k2, k3; // constants

    public SecondOrderDynamics(float f, float z, float r, Vector3 x0)
    {
        // compute constants
        _w = 2 * Mathf.PI * f;
        _z = z;
        _d = _w * Mathf.Sqrt(Mathf.Abs(z * z - 1));
        k1 = z / (Mathf.PI * f);
        k2 = 1 / (_w * _w);
        k3 = r * z / _w;
        // initialize variables
        xp = x0;
        y = x0;
        yd = Vector3.zero;
    }

    public Vector3 Update(float T, Vector3 x, Vector3 xd)
    {
        xp = x;
        float k1_stable, k2_stable;
        if (_w * T < _z) { // clamp k2 to guarantee stability without jitter
            k1_stable = k1;
            k2_stable = Mathf.Max(k2, T*T/2 + T*k1/2, T*k1);
        } else { // use pole matching when the system is very fast
            float t1 = Mathf.Exp(-_z * _w * T);
            float alpha = 2 * t1 * (_z <= 1 ? Mathf.Cos(T * _d) : Mathf.Cos(T * _d));
            float beta = t1 * t1;
            float t2 = T / (1 + beta - alpha);
            k1_stable = (1 - beta) * t2;
            k2_stable = T * t2;
        }
        y = y + T * yd; // integrate position by velocity
        yd = yd + T * (x + k3*xd - y - k1*yd) / k2_stable; // integrate velocity by acceleration
        return y;
    }
}
