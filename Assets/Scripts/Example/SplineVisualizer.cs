using Math.Spline;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineVisualizer : MonoBehaviour
{
    [Header("Input")]
    public SplineBehaviour splineBehaviour;

    [Header("Spline")]
    public bool drawSpline = true;
    public Color splineColor = Color.green;

    [Header("Normals")]
    public bool drawNormals = false;
    public float normalExtrusion = 1f;
    public Color normalColor = Color.red;

    [Header("Tangents")]
    public bool drawTangents = false;
    public float tangentExtrusion = 1f;
    public Color tangentColor = Color.blue;

    [Header("Control Points")]
    public bool drawControlPoints = false;
    public float controlPointRadius = 0.1f;
    public Color controlPointColor = Color.yellow;

    [Header("Spline Points")]
    public bool drawSplinePoints = false;
    public float splinePointRadius = 0.1f;
    public Color splinePointColor = Color.magenta;
    
    private void OnDrawGizmos()
    {
        if(splineBehaviour == null)
        {
            return;
        }

        if(drawSpline)
        {
            CatmullRomSplineGizmoDrawer.DrawSpline(splineBehaviour.GeneratedSplinePoints, splineBehaviour.closedLoop, splineColor);
        }

        if(drawNormals)
        {
            CatmullRomSplineGizmoDrawer.DrawNormals(splineBehaviour.GeneratedSplinePoints, normalExtrusion, normalColor);
        }

        if(drawTangents)
        {
            CatmullRomSplineGizmoDrawer.DrawTangents(splineBehaviour.GeneratedSplinePoints, tangentExtrusion, tangentColor);
        }

        if (drawControlPoints)
        {
            CatmullRomSplineGizmoDrawer.DrawControlPoints(splineBehaviour.ControlPoints, controlPointRadius, controlPointColor);
        }

        if (drawSplinePoints)
        {
            CatmullRomSplineGizmoDrawer.DrawSplinePoints(splineBehaviour.GeneratedSplinePoints, splinePointRadius, splinePointColor);
        }
    }
}
