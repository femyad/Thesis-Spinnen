#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpiderScaler))]
public class SpiderScalerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SpiderScaler script = (SpiderScaler)target;
        if (GUILayout.Button("Save to JSON"))
        {
            script.SaveScaleData();
        }
    }
}
#endif
