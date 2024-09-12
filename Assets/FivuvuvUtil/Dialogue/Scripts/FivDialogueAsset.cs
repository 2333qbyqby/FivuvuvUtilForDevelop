using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil
{
    [CreateAssetMenu(fileName = "FivDialogueAsset", menuName = "FivuvuvUtil/Dialogue Asset", order = 5)]
    public class FivDialogueAsset : ScriptableObject
    {
        public string dialogueAssetName;
        public List<FivDialogueEditorNode> editorNodes = new List<FivDialogueEditorNode>();

        public List<FivDE_IntProperty> intProperties = new List<FivDE_IntProperty>();

        public List<FivDE_EventProperty> eventProperties = new List<FivDE_EventProperty>();

    }
}

