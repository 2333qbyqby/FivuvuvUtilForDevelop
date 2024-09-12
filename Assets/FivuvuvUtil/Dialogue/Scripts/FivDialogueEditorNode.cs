using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil
{
    [System.Serializable]
    public class FivDialogueEditorNode
    {
        public string name;
        public int uid;
        public Rect rect, oRect;
        public FivDialogueEditorNodeType type;
        public FivDialogueNodeInfo info;


        public List<int> linkedNodesID = new List<int>();
        public List<int> linkedFromNodesID = new List<int>();

        public string event_name = "<event name>";
        public float event_arg0, event_arg1;

        public string int_property_name = "<variable>";
        public int int_property_value = 0;

        public string if_property_name = "<variable>";
        public int if_property_value = 0;
        public FivDE_Comparator if_comparator = 0;

        [NonSerialized]
        private List<FivDialogueEditorNode> linkedNodes, linkedFromNodes;
        private int orderInAsset;

        public int OrderInAsset
        {
            get
            {
                int i = 0;
                foreach (var item in FivDialogueManager.curEditorAsset.editorNodes)
                {
                    if (item == this)
                    {
                        break;
                    }
                    i++;
                }
                orderInAsset = i;
                return orderInAsset;
            }
        }

        public List<FivDialogueEditorNode> LinkedNodes
        {
            get
            {
                ConstructLinks();
                return linkedNodes;
            }
        }

        public List<FivDialogueEditorNode> LinkedFromNodes
        {
            get
            {
                ConstructLinks();
                return linkedFromNodes;
            }
        }


        public FivDialogueEditorNode()
        {

            info = new FivDialogueNodeInfo();
            info.speaker = "<Speaker>";
            info.content = "<Content>";
        }

        private void ConstructLinks()
        {
            if (FivDialogueManager.curEditorAsset == null) return;
            linkedNodes = new List<FivDialogueEditorNode>();
            linkedFromNodes = new List<FivDialogueEditorNode>();
            foreach (var item in FivDialogueManager.curEditorAsset.editorNodes)
            {
                if (linkedNodesID.Contains(item.uid))
                {
                    linkedNodes.Add(item);
                }
                if (linkedFromNodesID.Contains(item.uid))
                {
                    linkedFromNodes.Add(item);
                }
            }
        }
        public string GetSpeakerString()
        {
            return info.speaker;
        }
        public string GetContentString()
        {
            return info.content;
        }
        public Vector2 GetSize()
        {
            switch (type)
            {
                case FivDialogueEditorNodeType.Sentence:
                    return new Vector2(100, 130);
                case FivDialogueEditorNodeType.Choice:
                    return new Vector2(100, 80);
                case FivDialogueEditorNodeType.Start:
                    return new Vector2(100, 80);
                case FivDialogueEditorNodeType.Set:
                    return new Vector2(100, 80);
                case FivDialogueEditorNodeType.Event:
                    return new Vector2(100, 80);
                case FivDialogueEditorNodeType.Random:
                    return new Vector2(100, 80);
                case FivDialogueEditorNodeType.If:
                    return new Vector2(100, 80);
                default:
                    return new Vector2(100, 80);
            }
        }
    }
    public enum FivDialogueEditorNodeType
    {
        Sentence,
        Choice,
        Random,
        Event,
        Start,
        Set,
        If,
        End
    }
    public enum FivDE_Comparator
    {
        Equals,
        GreaterThan,
        LessThan,
    }
    [System.Serializable]
    public class FivDE_IntProperty
    {
        public string name;
        public int value;
    }
    [System.Serializable]
    public class FivDE_EventProperty
    {
        public string name;
        public Action<float, float> action;
    }
}




