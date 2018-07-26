using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SplineBehaviour))]
public class SplineBehaviourEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        bool generate = false;
        
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();
        if (EditorGUI.EndChangeCheck()) // If any setting changed
        {
            if(SplineBehaviour.regenerateOnEditorChange)
            {
                generate = true;
            }
        }

        GUILayout.Space(14f);
        GUILayout.Label(string.Format("Control Points: {0}", SplineBehaviour.ControlPoints.Count));
        GUILayout.Label(string.Format("Spline Points: {0}", SplineBehaviour.GeneratedSplinePoints.Count));
        GUILayout.Label(string.Format("Is Generating Async: {0}", SplineBehaviour.IsGeneratingAsync));
        
        if (GUILayout.Button("Generate Points")) // If pressed generate
        {
            generate = true;
        }

        EditorGUI.BeginDisabledGroup(Application.isPlaying == false); // Disabled when not playing
        if (GUILayout.Button("Generate Points (Async)")) // If pressed generate async
        {
            SplineBehaviour.GenerateAsync();
        }
        EditorGUI.EndDisabledGroup();

        if (generate) // If should generate
        {
            SplineBehaviour.Generate();
            SceneView.RepaintAll(); // Ensure gizmos are redrawn immediately
            Repaint(); // Repaint editor immediately to show new generated value statistics
        }

        // Save changes
        serializedObject.ApplyModifiedProperties();
    }

    SplineBehaviour SplineBehaviour
    {
        get
        {
            return target as SplineBehaviour;
        }
    }

    public override bool RequiresConstantRepaint()
    {
        return SplineBehaviour.IsGeneratingAsync; // Repaint constantly while generating async to show generated value statistics as they are calculated
    }
}
