using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridLayout))]
public class GridLayoutEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GridLayout layout = (GridLayout)target;
        layout.rows = EditorGUILayout.IntField("Rows", layout.rows);
        layout.columns = EditorGUILayout.IntField("Columns", layout.columns);
        if (GUILayout.Button("Layout"))
        {
            GridLayoutWindow.Init(layout);
        }
    }
}