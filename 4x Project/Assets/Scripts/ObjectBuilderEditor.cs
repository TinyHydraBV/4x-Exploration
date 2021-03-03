using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapEditor))]
public class ObjectBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapEditor myScript = (MapEditor)target;
        if (GUILayout.Button("Revise Map"))
        {
            myScript.ReviseMap();
        }
    }
}
