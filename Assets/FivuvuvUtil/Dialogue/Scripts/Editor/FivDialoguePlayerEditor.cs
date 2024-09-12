using UnityEditor;
using UnityEngine;
namespace FivuvuvUtil
{
    [CustomEditor(typeof(FivDialoguePlayer))]
    public class FivDialoguePlayerEditor : Editor
    {
        FivDialoguePlayer player;
        FivDialogueEditor editor;
        private void OnEnable()
        {
            player = (FivDialoguePlayer)target;
        }

        public override void OnInspectorGUI()
        {
            GUI.color = new Color(.9f, .8f, .7f);
            if (GUILayout.Button("Open Editor"))
            {
                editor = (FivDialogueEditor)EditorWindow.GetWindow<FivDialogueEditor>("SK Dialogue Editor", typeof(UnityEditor.SceneView));
                editor.LoadAsset(player.asset);
            }
            GUI.color = Color.white;
            if (GUILayout.Button("New Dialogue Asset"))
            {
                string path = EditorUtility.SaveFilePanel("Create new dialogue asset", "Assets/", "New Dialogue Asset", "asset");
                AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<FivDialogueAsset>(), path.Substring(path.IndexOf("Assets")));
                player.asset = AssetDatabase.LoadAssetAtPath<FivDialogueAsset>(path.Substring(path.IndexOf("Assets")));
                editor = (FivDialogueEditor)EditorWindow.GetWindow(typeof(FivDialogueEditor));
                editor.LoadAsset(player.asset);
            }
            base.OnInspectorGUI();
        }
    }
}
