using Math.Spline;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class SplineBehaviour : MonoBehaviour
{
    [Header("Input")]
    public List<Transform> controlPointTransforms;

    [Header("Settings")]
    public bool debug = false;
    public bool regenerateEveryFrame = false;
    public bool regenerateOnEditorChange = false;
    public bool closedLoop = true;
    public int resolution = 100;
    public double frameTimeBudget = 16d;

    List<Vector3> controlPoints = new List<Vector3>();
    List<CatmullRomSplinePoint> generatedSplinePoints = new List<CatmullRomSplinePoint>();
    Coroutine asyncGenerateCoroutine = null;

    private void Update()
    {
        if(regenerateEveryFrame)
        {
            Generate();
        }
    }

    public void Generate()
    {
        UpdateControlPoints();
        CatmullRomSpline.GenerateSplinePointsNonAlloc(ref generatedSplinePoints, controlPoints, closedLoop, resolution);
    }

    public void GenerateAsync()
    {
        UpdateControlPoints();
        int pointsToGenerate = CatmullRomSpline.CalculateGeneratedPointsToGenerate(controlPoints.Count, closedLoop, resolution);
        List<CatmullRomSplinePoint> initialResults = new List<CatmullRomSplinePoint>(pointsToGenerate);
        IEnumerable<CatmullRomSplinePoint> sequence = CatmullRomSpline.GenerateSplinePointsSequence(controlPoints, closedLoop, resolution);
        var timeBudgettedCoroutine = CoroutineUtils.FrameTimeBudgettedCoroutine(initialResults, sequence, ProcessSingleResult, ProcessAccumulatedResults, ProcessResults, frameTimeBudget);
        if(asyncGenerateCoroutine != null)
        {
            StopCoroutine(asyncGenerateCoroutine);
            asyncGenerateCoroutine = null;
        }
        asyncGenerateCoroutine = StartCoroutine(timeBudgettedCoroutine);
    }
    
    private void ProcessSingleResult(ref List<CatmullRomSplinePoint> accumulatedResults, ref CatmullRomSplinePoint singleResult)
    {
        accumulatedResults.Add(singleResult);
    }

    private void ProcessAccumulatedResults(ref List<CatmullRomSplinePoint> results)
    {
        if(debug) Debug.LogFormat("ProcessAccumulatedResults: {0} points", results.Count);
        generatedSplinePoints = results;
#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    private void ProcessResults(ref List<CatmullRomSplinePoint> results)
    {
        if (debug) Debug.LogFormat("ProcessResults: {0} points", results.Count);
        generatedSplinePoints = results;
#if UNITY_EDITOR
        UnityEditor.SceneView.RepaintAll();
#endif
    }

    void UpdateControlPoints()
    {
        controlPoints.Clear();
        controlPoints.AddRange(controlPointTransforms.Select(t => t.position));
    }

    public List<Vector3> ControlPoints
    {
        get
        {
            return controlPoints;
        }
    }

    public List<CatmullRomSplinePoint> GeneratedSplinePoints
    {
        get
        {
            return generatedSplinePoints;
        }
    }

    public bool IsGeneratingAsync
    {
        get
        {
            return asyncGenerateCoroutine != null;
        }
    }
}
