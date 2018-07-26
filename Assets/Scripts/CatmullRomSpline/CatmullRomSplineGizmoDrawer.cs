using System.Collections.Generic;
using UnityEngine;

namespace Math.Spline
{
    public static class CatmullRomSplineGizmoDrawer
    {
        public static void DrawSpline(IList<CatmullRomSplinePoint> splinePoints, bool closedLoop, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < splinePoints.Count; i++)
            {
                if (i == splinePoints.Count - 1 && closedLoop)
                {
                    Gizmos.DrawLine(splinePoints[i].position, splinePoints[0].position);
                    //Debug.DrawLine(splinePoints[i].position, splinePoints[0].position, color);
                }
                else if (i < splinePoints.Count - 1)
                {
                    Gizmos.DrawLine(splinePoints[i].position, splinePoints[i + 1].position);
                    //Debug.DrawLine(splinePoints[i].position, splinePoints[i + 1].position, color);
                }
            }
        }
        
        public static void DrawNormals(IList<CatmullRomSplinePoint> splinePoints, float extrusion, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < splinePoints.Count; i++)
            {
                Gizmos.DrawLine(splinePoints[i].position, splinePoints[i].position + splinePoints[i].normal * extrusion);
                //Debug.DrawLine(splinePoints[i].position, splinePoints[i].position + splinePoints[i].normal * extrusion, color);
            }
        }

        public static void DrawTangents(IList<CatmullRomSplinePoint> splinePoints, float extrusion, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < splinePoints.Count; i++)
            {
                Gizmos.DrawLine(splinePoints[i].position, splinePoints[i].position + splinePoints[i].tangent * extrusion);
                //Debug.DrawLine(splinePoints[i].position, splinePoints[i].position + splinePoints[i].tangent * extrusion, color);
            }
        }

        public static void DrawSplinePoints(IList<CatmullRomSplinePoint> splinePoints, float radius, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < splinePoints.Count; i++)
            {
                Gizmos.DrawWireSphere(splinePoints[i].position, radius);
            }
        }

        public static void DrawControlPoints(IList<Vector3> controlPoints, float radius, Color color)
        {
            Gizmos.color = color;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                Gizmos.DrawSphere(controlPoints[i], radius);
            }
        }
    }
}
