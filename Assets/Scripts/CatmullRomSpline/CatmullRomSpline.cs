using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Math.Spline
{
    /// <summary>
    /// BASED ON: https://github.com/JPBotelho/Catmull-Rom-Splines/blob/master/CatmullRom.cs
    /// Catmull-Rom splines are Hermite curves with special tangent values.
    /// Hermite curve formula:
    /// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
    /// For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
    /// Tangent M[k] = (P[k + 1] - P[k - 1]) / 2
    /// </summary>
    public static class CatmullRomSpline
    {
        public static List<CatmullRomSplinePoint> GenerateSplinePoints(List<Vector3> controlPoints, bool closedLoop, int resolution)
        {
            int pointsToGenerate = CalculateGeneratedPointsToGenerate(controlPoints.Count, closedLoop, resolution);
            List<CatmullRomSplinePoint> splinePoints = new List<CatmullRomSplinePoint>(pointsToGenerate);
            GenerateSplinePointsNonAlloc(ref splinePoints, controlPoints, closedLoop, resolution);
            return splinePoints;
        }
        
        public static void GenerateSplinePointsNonAlloc(ref List<CatmullRomSplinePoint> splinePoints, List<Vector3> controlPoints, bool closedLoop, int resolution)
        {
            if (resolution <= 0)
            {
                throw new ArgumentException("Resolution must be > 0");
            }

            splinePoints.Clear();
            foreach(var point in GenerateSplinePointsSequence(controlPoints, closedLoop, resolution))
            {
                splinePoints.Add(point);
            }
        }

        /// <summary>
        /// Catmull-Rom splines are Hermite curves with special tangent values.
        /// Hermite curve formula:
        /// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        /// For points p0 and p1 passing through points m0 and m1 interpolated over t = [0, 1]
        /// Tangent M[k] = (P[k + 1] - P[k - 1]) / 2
        /// </summary>
        public static IEnumerable<CatmullRomSplinePoint> GenerateSplinePointsSequence(List<Vector3> controlPoints, bool closedLoop, int resolution)
        {
            if (resolution <= 0)
            {
                throw new ArgumentException("Resolution must be > 0");
            }

            // Start and end points
            Vector3 p0;
            Vector3 p1;

            //Tangents
            Vector3 m0;
            Vector3 m1;

            // First for loop goes through each individual control point and connects it to the next, so 0-1, 1-2, 2-3 and so on
            int closedAdjustment = closedLoop ? 0 : 1;
            for (int currentPoint = 0; currentPoint < controlPoints.Count - closedAdjustment; currentPoint++)
            {
                bool closedLoopFinalPoint = (closedLoop && currentPoint == controlPoints.Count - 1);
                p0 = controlPoints[currentPoint];
                if (closedLoopFinalPoint)
                {
                    p1 = controlPoints[0];
                }
                else
                {
                    p1 = controlPoints[currentPoint + 1];
                }

                // m0
                if (currentPoint == 0) // Tangent M[k] = (P[k+1] - P[k-1]) / 2
                {
                    if (closedLoop)
                    {
                        m0 = p1 - controlPoints[controlPoints.Count - 1];
                    }
                    else
                    {
                        m0 = p1 - p0;
                    }
                }
                else
                {
                    m0 = p1 - controlPoints[currentPoint - 1];
                }

                // m1
                if (closedLoop)
                {
                    if (currentPoint == controlPoints.Count - 1) //Last point case
                    {
                        m1 = controlPoints[(currentPoint + 2) % controlPoints.Count] - p0;
                    }
                    else if (currentPoint == 0) //First point case
                    {
                        m1 = controlPoints[currentPoint + 2] - p0;
                    }
                    else
                    {
                        m1 = controlPoints[(currentPoint + 2) % controlPoints.Count] - p0;
                    }
                }
                else
                {
                    if (currentPoint < controlPoints.Count - 2)
                    {
                        m1 = controlPoints[(currentPoint + 2) % controlPoints.Count] - p0;
                    }
                    else
                    {
                        m1 = p1 - p0;
                    }
                }

                m0 *= 0.5f; // Doing this here instead of  in every single above statement
                m1 *= 0.5f;

                float pointStep = 1.0f / resolution;
                if ((currentPoint == controlPoints.Count - 2 && !closedLoop) || closedLoopFinalPoint) //Final point
                {
                    pointStep = 1.0f / (resolution - 1);  // last point of last segment should reach p1
                }

                // Creates [resolution] points between this control point and the next
                for (int tesselatedPoint = 0; tesselatedPoint < resolution; tesselatedPoint++)
                {
                    float t = tesselatedPoint * pointStep;
                    CatmullRomSplinePoint point = Evaluate(p0, p1, m0, m1, t);
                    yield return point;
                }
            }
        }
        
        /// <summary>
        /// Calculates the amount of points that will be generated with the given inputs.
        /// </summary>
        public static int CalculateGeneratedPointsToGenerate(int controlPoints, bool closedLoop, int resolution)
        {
            int pointsToCreate;
            if (closedLoop)
            {
                // Loops back to the beggining, so no need to adjust for arrays starting at 0
                pointsToCreate = resolution * controlPoints;
            }
            else
            {
                pointsToCreate = resolution * (controlPoints - 1);
            }
            return pointsToCreate;
        }

        /// <summary>
        /// Evaluates curve at t[0, 1]. Returns point/normal/tan struct. [0, 1] means clamped between 0 and 1.
        /// </summary>
        private static CatmullRomSplinePoint Evaluate(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
        {
            Vector3 position = CalculatePosition(start, end, tanPoint1, tanPoint2, t);
            Vector3 tangent = CalculateTangent(start, end, tanPoint1, tanPoint2, t);
            Vector3 normal = NormalFromTangent(tangent);

            return new CatmullRomSplinePoint(position, tangent, normal);
        }

        /// <summary>
        /// Calculates curve position at t[0, 1]
        /// Hermite curve formula:
        /// (2t^3 - 3t^2 + 1) * p0 + (t^3 - 2t^2 + t) * m0 + (-2t^3 + 3t^2) * p1 + (t^3 - t^2) * m1
        /// </summary>
        private static Vector3 CalculatePosition(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
        {
            return (2.0f * t * t * t - 3.0f * t * t + 1.0f) * start // (2t^3 - 3t^2 + 1) * p0
                + (t * t * t - 2.0f * t * t + t) * tanPoint1 // (t^3 - 2t^2 + t) * m0
                + (-2.0f * t * t * t + 3.0f * t * t) * end // (-2t^3 + 3t^2) * p1
                + (t * t * t - t * t) * tanPoint2 // (t^3 - t^2) * m1
            ;
        }

        /// <summary>
        /// Calculates tangent at t[0, 1]
        /// p'(t) = (6t² - 6t) * p0 + (3t² - 4t + 1) * m0 + (-6t² + 6t) * p1 + (3t² - 2t) * m1
        /// </summary>
        private static Vector3 CalculateTangent(Vector3 start, Vector3 end, Vector3 tanPoint1, Vector3 tanPoint2, float t)
        {
            return Vector3.Normalize(
                (6 * t * t - 6 * t) * start // (6t² - 6t) * p0
                + (3 * t * t - 4 * t + 1) * tanPoint1 // (3t² - 4t + 1) * m0
                + (-6 * t * t + 6 * t) * end // (-6t² + 6t) * p1
                + (3 * t * t - 2 * t) * tanPoint2 // (3t² - 2t) * m1
            );
        }

        /// <summary>
        /// Calculates normal vector from tangent
        /// </summary>
        private static Vector3 NormalFromTangent(Vector3 tangent)
        {
            return Vector3.Cross(tangent, Vector3.up).normalized / 2;
        }
    }
}
