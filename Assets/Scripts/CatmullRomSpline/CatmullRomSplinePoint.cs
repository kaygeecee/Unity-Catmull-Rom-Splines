using System;
using UnityEngine;

namespace Math.Spline
{
    [Serializable]
    public struct CatmullRomSplinePoint
    {
        public Vector3 position;
        public Vector3 tangent;
        public Vector3 normal;

        public CatmullRomSplinePoint(Vector3 position, Vector3 tangent, Vector3 normal)
        {
            this.position = position;
            this.tangent = tangent;
            this.normal = normal;
        }
    }
}
