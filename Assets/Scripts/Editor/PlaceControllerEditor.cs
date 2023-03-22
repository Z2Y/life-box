using Controller;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(PlaceController))]
    public class PlaceControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var placeController = target as PlaceController;

            if (placeController != null && GUILayout.Button("Update Bounds"))
            {
                placeController.UpdateBoundsFormEditor();
            }
        }
    }
}