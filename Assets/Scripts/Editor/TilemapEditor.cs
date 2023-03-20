using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
    [CustomEditor(typeof(Tilemap))]
    public class TilemapEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var tilemap = target as Tilemap;

            if (GUILayout.Button("Compress Bounds"))
            {
                var bounds = tilemap.cellBounds;
                tilemap.CompressBounds();
                if (tilemap.cellBounds != bounds) EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}