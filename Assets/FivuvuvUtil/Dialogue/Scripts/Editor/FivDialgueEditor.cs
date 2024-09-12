using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FivuvuvUtil
{

    public class FivDialogueEditor : EditorWindow
    {
        private static Rect BG_RECT = new Rect(-1000, -1000, 10000, 10000);
        private static Color COL_BACKGROUND = new Color(.1f, .1f, .1f);
        private static Color COL_WIRE = new Color(1, 1, 1, .07f);
        private static Color COL_TITLE = new Color(1, 1, 1, .6f);
        private static Color COL_YELLOW = new Color(1f, .95f, .6f, 0.9f);
        private static Color COL_BLUE = new Color(.4f, .7f, .9f, 0.8f);
        private static Color COL_ORANGE = new Color(1f, .7f, .7f, 0.8f);
        #region GUIStyles
        private static GUIStyle titleStyle;
        private static GUIStyle nodeWhiteStyle, nodeYellowStyle, nodeGreenStyle, nodeGreyStyle, nodePurpleStyle, nodeBlueStyle;
        private static GUIStyle nodeTitleStyle, nodeSubtitleStyle, nodeSubtitle2Style, nodeCenterStyle, nodeCenterStyle2, nodeCenterStyle3;
        private static GUIStyle inspectorButtonStyle, inspectorTitleStyle, inspectorBgStyle;
        private static GUIStyle textOrangeStyle;
        #endregion
        public FivDialogueAsset asset = null;
        public List<FivDialogueEditorNode> nodes = new List<FivDialogueEditorNode>();
        public FivDialogueEditorNode inspectorNode;


        private bool isConnecting = false;
        private FivDialogueEditorNode connectFromNode;

        private FivDialogueAsset m_asset;
        private float panX = 0;
        private float panY = 0;
        private float zoom = 1f; // Add a zoom variable

        private Vector2 inspectorScrollPos0, inspectorScrollPos1, inspectorScrollPos2;

        [MenuItem("Tools/FivuvuvUtil/Diagolue Editor", priority = 2)]
        static void OpenWindow()
        {
            FivDialogueEditor editor = EditorWindow.GetWindow<FivDialogueEditor>("Dialogue Editor", typeof(UnityEditor.SceneView));
        }
        #region unity生命周期
        private void OnEnable()
        {
            Init();
        }

        private void OnBecameVisible()
        {
            InitializedGUIStyles();
        }

        private void OnLostFocus()
        {
            if (asset != null)
            {
                EditorUtility.SetDirty(asset);
            }
        }
        private void Update()
        {

        }

        private void OnGUI()
        {
            GUI.depth = 0;
            DrawBackground();
            HandleMouseInput();
            HandleConnectionState();

            GUI.BeginGroup(new Rect(panX - 1000, panY - 1000, 100000, 100000));
            BeginWindows();
            if (nodes != null && FivDialogueManager.curEditorAsset != null)
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    DrawNode(nodes[i], i);
                }
            }
            EndWindows();
            GUI.EndGroup();

            DrawInspector();

            if (EditorWindow.mouseOverWindow == this)
            {
                Repaint();
            }

        }
        #endregion
        public void Init()
        {
            if (asset != null)
            {
                LoadAsset(asset);
            }
            else
            {
                LoadAsset(null);
            }
            inspectorNode = null;
        }
        public void LoadAsset(FivDialogueAsset asset)
        {
            if (asset == null)
            {
                this.asset = null;
                nodes = null;
                FivDialogueManager.curEditorAsset = null;
                return;
            }
            this.asset = asset;
            this.m_asset = asset;
            nodes = asset.editorNodes;
            FivDialogueManager.curEditorAsset = asset;

            if (asset.editorNodes.Count == 0)
            {
                FivDialogueEditorNode start = AddNewNode(FivDialogueEditorNodeType.Start, new Vector2(100, 100));
                FivDialogueEditorNode end = AddNewNode(FivDialogueEditorNodeType.End, new Vector2(400, 100));

            }
        }


        private void HandleMouseInput()
        {
            if (asset == null || IsMouseInInspector())
            {
                return;
            }

            Event e = Event.current;
            //右键
            if (e.type == EventType.MouseUp && e.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                FivDialogueEditorNode node = GetMouseOverNode();

                if (node == null || node.type != FivDialogueEditorNodeType.End)
                {
                    //create nodes
                    menu.AddItem(new GUIContent("Create New Node/Sentence"), false, menuAddNodeSentenceNode, (e.mousePosition, node));
                    menu.AddItem(new GUIContent("Create New Node/Choice"), false, menuAddNodeOptionNode, (e.mousePosition, node));
                    menu.AddItem(new GUIContent("Create New Node/Event"), false, menuAddNodeActionNode, (e.mousePosition, node));
                    menu.AddItem(new GUIContent("Create New Node/Random"), false, menuAddNodeRandomNode, (e.mousePosition, node));
                    menu.AddSeparator("Create New Node/");
                    menu.AddItem(new GUIContent("Create New Node/Set"), false, menuAddNodeSetNode, (e.mousePosition, node));
                    menu.AddItem(new GUIContent("Create New Node/If"), false, menuAddNodeIfNode, (e.mousePosition, node));
                    menu.AddSeparator("Create New Node/");
                    menu.AddItem(new GUIContent("Create New Node/End"), false, menuAddNodeEndNode, (e.mousePosition, node));
                }

                if (node != null)
                {
                    if (node.type != FivDialogueEditorNodeType.End)
                    {
                        menu.AddItem(new GUIContent("Link To..."), false, menuConnectTo, node);
                    }
                    if (node.type != FivDialogueEditorNodeType.End)
                    {
                        foreach (var link in node.LinkedNodes)
                        {
                            menu.AddItem(new GUIContent($"Unlink.../{link.name}"), false, menuDetach, (node, link));
                        }
                    }
                    if (node.type != FivDialogueEditorNodeType.Start)
                    {
                        menu.AddItem(new GUIContent("Delete"), false, menuDeleteNode, node);
                    }

                }
                menu.ShowAsContext();

            }
        }

        private void HandleConnectionState()
        {
            if (isConnecting)
            {
                Event e = Event.current;

                if (e != null && connectFromNode != null)
                {
                    Rect from = new Rect(connectFromNode.rect);
                    from.x -= 1000 - panX;
                    from.y -= 1000 - panY;
                    DrawNodeCurve(from, new Rect(e.mousePosition.x, e.mousePosition.y, 1, 1));
                }
                if (e != null && e.type == EventType.MouseUp)
                {

                    FivDialogueEditorNode toNode = GetMouseOverNode();
                    if (toNode != null && toNode != connectFromNode)
                    {
                        LinkNodes(connectFromNode, toNode);
                    }
                    isConnecting = false;
                    connectFromNode = null;
                }
            }
        }
        #region 右键菜单功能
        private void menuDetach(object obj)
        {
            (FivDialogueEditorNode, FivDialogueEditorNode) nodes = ((FivDialogueEditorNode, FivDialogueEditorNode))obj;
            UnLinkNodes(nodes.Item1, nodes.Item2);
        }
        private void menuConnectTo(object obj)
        {
            var node = (FivDialogueEditorNode)obj;
            connectFromNode = node;
            isConnecting = true;

        }

        private void menuDeleteNode(object obj)
        {
            var node = (FivDialogueEditorNode)obj;
            DeleteNode(node);
        }
        private void menuAddNodeSentenceNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.Sentence, value.Item1, value.Item2);
        }
        private void menuAddNodeOptionNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.Choice, value.Item1, value.Item2);
        }
        private void menuAddNodeActionNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.Event, value.Item1, value.Item2);
        }
        private void menuAddNodeRandomNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.Random, value.Item1, value.Item2);
        }
        private void menuAddNodeSetNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.Set, value.Item1, value.Item2);
        }
        private void menuAddNodeIfNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.If, value.Item1, value.Item2);
        }
        private void menuAddNodeEndNode(object obj)
        {
            (Vector2, FivDialogueEditorNode) value = ((Vector2, FivDialogueEditorNode))obj;
            AddNewNode(FivDialogueEditorNodeType.If, value.Item1, value.Item2);
        }
        #endregion

        private void DrawBackground()
        {
            GUI.backgroundColor = new Color(.8f, .8f, .8f);
            EditorGUI.DrawRect(BG_RECT, COL_BACKGROUND);
            zoom = Mathf.Clamp(zoom, 1f, 10f); // Clamp the zoom value to a minimum of 0.1 and a maximum of 10
            for (int i = 0; i < 4000; i += 30)
            {
                EditorGUI.DrawRect(new Rect((panX - 1000) * zoom, (panY - 1000 + i) * zoom, 10000, 1), COL_WIRE);
                EditorGUI.DrawRect(new Rect((panX - 1000 + i) * zoom, (panY - 1000) * zoom, 1, 10000), COL_WIRE);


            }

            Event e = Event.current;
            if (e.type == EventType.MouseDrag)
            {
                if (GetMouseOverNode() != null)
                {
                    return;
                }
                panX += e.delta.x;
                panY += e.delta.y;
                Repaint();
            }
            //if (e.type == EventType.ScrollWheel) // Handle scroll wheel event
            //{
            //    Debug.Log(e.delta);
            //    zoom += e.delta.y * 0.1f; // Adjust the zoom based on the scroll wheel delta
            //    Repaint();
            //}
        }

        private void DrawInspector()
        {
            GUI.Label(new Rect(2, 2, 100, 50), "Dialogue Editor", titleStyle);
            m_asset = EditorGUI.ObjectField(new Rect(2, 30, 150, 20), m_asset, typeof(FivDialogueAsset), false) as FivDialogueAsset;
            if (GUI.Button(new Rect(160, 30, 40, 20), "Load"))
            {
                if (asset == null || m_asset != asset)
                {
                    LoadAsset(m_asset);
                }
            }

            if (asset == null)
            {
                GUI.Label(new Rect(2, 60, 200, 20), "No Asset selected", titleStyle);
            }

            if (inspectorNode == null)
            {
                return;
            }

            var node = inspectorNode;
            float sizeX = 200, sizeY = 300;


            //人物
            if (node.type == FivDialogueEditorNodeType.Sentence)
            {
                GUILayout.BeginArea(new Rect(sizeX + 20, this.position.yMax - sizeY - 20, sizeX * 0.7f, sizeY / 3 + 20));
                GUILayout.Box("", inspectorBgStyle, GUILayout.Width(sizeX), GUILayout.Height(sizeY / 3));
                GUILayout.Space(-sizeY / 3);
                GUILayout.Label("Avatar sprite: ");
                //node.info.avatar = (Texture2D)EditorGUILayout.ObjectField(node.info.avatar, typeof(Texture2D), false);
                GUILayout.Label("Audio: ");
                node.info.audio = (AudioClip)EditorGUILayout.ObjectField(node.info.audio, typeof(AudioClip), false);
                GUILayout.EndArea();
            }

            GUILayout.BeginArea(new Rect(0, this.position.yMax - sizeY, sizeX, sizeY));
            GUILayout.Box("", inspectorBgStyle, GUILayout.Width(sizeX), GUILayout.Height(sizeY));
            GUILayout.Space(-sizeY);

            GUILayout.Label($"{node.type.ToString()} Node", inspectorTitleStyle);
            node.name = GUILayout.TextField(node.name);
            //main inspector
            if (node.type == FivDialogueEditorNodeType.Sentence || node.type == FivDialogueEditorNodeType.Choice)
            {


                GUILayout.Space(5);


                GUILayout.Label("Speaker: ");
                inspectorScrollPos0 = EditorGUILayout.BeginScrollView(inspectorScrollPos0, GUILayout.Height(30));
                node.info.speaker = GUILayout.TextArea(node.info.speaker);
                EditorGUILayout.EndScrollView();
                GUILayout.Space(5);
                GUILayout.Label("Content: ");
                inspectorScrollPos1 = EditorGUILayout.BeginScrollView(inspectorScrollPos1, GUILayout.Height(60));
                node.info.content = GUILayout.TextArea(node.info.content);
                EditorGUILayout.EndScrollView();

            }
            else if (node.type == FivDialogueEditorNodeType.Event)
            {
                GUILayout.Space(8);
                GUILayout.Label("Event name: ");

                node.event_name = GUILayout.TextField(node.event_name);
                GUILayout.Label("Arg 0 (float): ");
                node.event_arg0 = EditorGUILayout.FloatField(node.event_arg0);
                GUILayout.Label("Arg 1 (float): ");
                node.event_arg1 = EditorGUILayout.FloatField(node.event_arg1);
            }
            else if (node.type == FivDialogueEditorNodeType.Set)
            {
                GUILayout.Space(8);
                GUILayout.Label("Variable name: ");

                node.int_property_name = GUILayout.TextField(node.int_property_name);
                GUILayout.Space(8);
                GUILayout.Label("Variable value: ");
                node.int_property_value = EditorGUILayout.IntField(node.int_property_value);
            }
            else if (node.type == FivDialogueEditorNodeType.If)
            {
                GUILayout.Space(8);
                GUILayout.Label("Variable name: ");
                node.if_property_name = GUILayout.TextField(node.if_property_name);
                GUILayout.Space(8);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Comparator: ");
                node.if_comparator = (FivDE_Comparator)EditorGUILayout.EnumPopup(node.if_comparator);
                GUILayout.EndHorizontal();
                GUILayout.Space(8);
                GUILayout.Label("Variable value: ");
                node.if_property_value = EditorGUILayout.IntField(node.if_property_value);
            }

            GUILayout.BeginArea(new Rect(sizeX / 2 - 25, sizeY - 100, 50, 120));
            if (GUILayout.Button("Close"))
            {
                inspectorNode = null;
            }
            GUILayout.EndArea();

            GUILayout.EndArea();
        }
        #region 绘制线和绘制节点

        private void DrawNode(FivDialogueEditorNode node, int index)
        {
            if (node == null)
            {
                return;
            }
            node.rect = GUI.Window(index, node.rect, DrawNodeWindow, node.name);

            if (node.type != FivDialogueEditorNodeType.Start &&
                node.type != FivDialogueEditorNodeType.End &&
                node.type != FivDialogueEditorNodeType.Random &&
                GUI.Button(new Rect(node.rect.xMin, node.rect.yMax + 5, node.rect.width, 15), "Edit", inspectorButtonStyle))
            {
                inspectorNode = node;
            }

            foreach (var link in node.LinkedNodes)
            {
                DrawNodeCurve(node.rect, link.rect);
            }
        }
        private void DrawNodeWindow(int id)
        {
            var node = nodes[id];
            GUI.DragWindow();

            if (node.type == FivDialogueEditorNodeType.Sentence)
            {
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.Box("", nodeYellowStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.Space(8);
                GUILayout.Label(node.GetSpeakerString(), nodeSubtitle2Style);
                GUILayout.Space(8);
                GUILayout.TextArea(node.GetContentString(), nodeSubtitleStyle);
            }
            else if (node.type == FivDialogueEditorNodeType.Choice)
            {
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeGreenStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
                GUILayout.Space(5);
                GUILayout.Label(node.GetSpeakerString(), nodeSubtitle2Style);
                GUILayout.Space(5);

                GUILayout.TextArea(node.GetContentString(), nodeSubtitleStyle);
            }
            else if (node.type == FivDialogueEditorNodeType.Start)
            {
                GUI.contentColor = new Color(1, 1, 1, 0.07f);
                GUI.backgroundColor = Color.clear;
                Rect iconRect = new Rect(node.rect.width / 2 - 27.5f, node.rect.height / 2 - 15, 55, 55);
                GUI.Box(iconRect, "Start");
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeGreenStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
            }
            else if (node.type == FivDialogueEditorNodeType.End)
            {
                GUI.contentColor = new Color(1, 1, 1, 0.07f);
                GUI.backgroundColor = Color.clear;
                Rect iconRect = new Rect(node.rect.width / 2 - 27.5f, node.rect.height / 2 - 15, 55, 55);
                GUI.Box(iconRect, "End");
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeGreenStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
            }
            else if (node.type == FivDialogueEditorNodeType.Event)
            {
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodePurpleStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
                GUILayout.Space(5);
                GUILayout.Label(node.event_name, nodeSubtitle2Style);
                GUILayout.Space(3);
                GUILayout.BeginHorizontal();
                GUILayout.Label(node.event_arg0.ToString("f2"), nodeSubtitleStyle);
                GUILayout.Label(node.event_arg1.ToString("f2"), nodeSubtitleStyle);
                GUILayout.EndHorizontal();
            }
            else if (node.type == FivDialogueEditorNodeType.Random)
            {
                GUI.contentColor = new Color(1, 1, 1, 0.07f);
                GUI.backgroundColor = Color.clear;
                Rect iconRect = new Rect(node.rect.width / 2 - 27.5f, node.rect.height / 2 - 15, 55, 55);
                GUI.Box(iconRect, "Random");
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeBlueStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
            }
            else if (node.type == FivDialogueEditorNodeType.Set)
            {
                GUI.contentColor = new Color(1, 1, 1, .6f);
                GUI.backgroundColor = Color.clear;
                Rect iconRect = new Rect(node.rect.width / 2 - 8f, node.rect.height / 2 + 1, 16, 16);
                GUI.Box(iconRect, "Set");
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeGreyStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
                GUILayout.Space(2);
                GUILayout.Label(node.int_property_name, nodeCenterStyle3);
                GUILayout.Space(18);
                GUILayout.Label(node.int_property_value.ToString(), nodeCenterStyle2);
            }
            else if (node.type == FivDialogueEditorNodeType.If)
            {
                GUI.backgroundColor = new Color(.8f, .8f, .8f);
                GUI.contentColor = Color.white;

                GUILayout.BeginVertical();
                GUILayout.Box("", nodeGreyStyle, GUILayout.Width(500), GUILayout.Height(2));
                GUILayout.EndVertical();
                GUILayout.Space(2);
                GUILayout.Label(node.if_property_name, nodeCenterStyle3);
                GUILayout.Space(3);
                GUILayout.Label(node.if_comparator.ToString(), nodeCenterStyle);
                GUILayout.Space(3);
                GUILayout.Label(node.if_property_value.ToString(), nodeCenterStyle2);
            }

        }
        private void DrawNodeCurve(Rect startRect, Rect endRect)
        {
            Vector3 startPos = new Vector3(startRect.x + startRect.width, startRect.y + startRect.height / 2, 0);
            Vector3 endPos = new Vector3(endRect.x, endRect.y + endRect.height / 2, 0);
            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;
            Color shadowCol = new Color(1, 1, 1, 0.06f);
            for (int i = 0; i < 3; i++) // Draw a shadow
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }
            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.grey, null, 1);
        }
        #endregion
        private FivDialogueEditorNode AddNewNode(FivDialogueEditorNodeType type, Vector2 position, FivDialogueEditorNode fromNode = null)
        {
            FivDialogueEditorNode node = new FivDialogueEditorNode();
            node.uid = Random.Range(1000, 1000000);
            if (type == FivDialogueEditorNodeType.Start)
                node.name = "Start";
            else if (type == FivDialogueEditorNodeType.Random)
                node.name = "Random";
            else if (type == FivDialogueEditorNodeType.Set)
                node.name = "Set";
            else if (type == FivDialogueEditorNodeType.If)
                node.name = "If";
            else if (type == FivDialogueEditorNodeType.End)
                node.name = "End";
            else
                node.name = type.ToString() + " " + nodes.Count;

            node.type = type;
            node.rect.x = position.x + 1000 - panX;
            node.rect.y = position.y + 1000 - panY;
            Vector2 size = node.GetSize();
            node.rect.width = size.x;
            node.rect.height = size.y;

            if (fromNode != null)
            {
                LinkNodes(fromNode, node);
            }
            nodes.Add(node);

            return node;
        }

        private void DeleteNode(FivDialogueEditorNode node)
        {
            if (node == null)
            {
                return;
            }
            foreach (var to in node.LinkedNodes)
            {
                if (to.linkedNodesID.Contains(node.uid))
                {
                    to.linkedNodesID.Remove(node.uid);

                }
            }
            foreach (var from in node.LinkedFromNodes)
            {
                if (from.linkedNodesID.Contains(node.uid))
                {
                    from.linkedNodesID.Remove(node.uid);
                }
            }
            nodes.Remove(node);
        }

        private void LinkNodes(FivDialogueEditorNode fromNode, FivDialogueEditorNode toNode)
        {
            if (!fromNode.linkedNodesID.Contains(toNode.uid))
            {
                fromNode.linkedNodesID.Add(toNode.uid);
            }
            if (!toNode.linkedFromNodesID.Contains(fromNode.uid))
            {
                toNode.linkedFromNodesID.Add(fromNode.uid);
            }
        }
        private void UnLinkNodes(FivDialogueEditorNode fromNode, FivDialogueEditorNode toNode)
        {
            if (fromNode.linkedNodesID.Contains(toNode.uid))
            {
                fromNode.linkedNodesID.Remove(toNode.uid);
            }
            if (toNode.linkedFromNodesID.Contains(fromNode.uid))
            {
                toNode.linkedFromNodesID.Remove(fromNode.uid);
            }
        }

        private void InitializedGUIStyles()
        {
            titleStyle = new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 21,
            };
            titleStyle.normal.textColor = COL_TITLE;

            nodeTitleStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 10,
            };
            nodeTitleStyle.normal.textColor = COL_TITLE;

            textOrangeStyle = new GUIStyle();
            textOrangeStyle.normal.textColor = COL_ORANGE;
            textOrangeStyle.alignment = TextAnchor.MiddleLeft;

            nodeSubtitleStyle = new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11,
                wordWrap = true
            };
            nodeSubtitleStyle.normal.textColor = COL_TITLE;
            nodeCenterStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true
            };
            nodeCenterStyle.normal.textColor = COL_TITLE;
            nodeCenterStyle2 = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true
            };
            nodeCenterStyle2.normal.textColor = COL_BLUE;
            nodeCenterStyle3 = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                wordWrap = true
            };
            nodeCenterStyle3.normal.textColor = COL_ORANGE;

            nodeSubtitle2Style = new GUIStyle()
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 11,
                wordWrap = true
            };
            nodeSubtitle2Style.normal.textColor = COL_YELLOW;

            nodeWhiteStyle = new GUIStyle();
            nodeWhiteStyle.normal.background = MakeTex(2, 2, new Color(1, 1, 1, 0.6f));

            nodeYellowStyle = new GUIStyle();
            nodeYellowStyle.normal.background = MakeTex(2, 2, new Color(1f, .95f, .5f, 0.6f));
            nodeGreyStyle = new GUIStyle();
            nodeGreyStyle.normal.background = MakeTex(2, 2, new Color(1f, 1f, 1f, 0.6f));
            nodePurpleStyle = new GUIStyle();
            nodePurpleStyle.normal.background = MakeTex(2, 2, new Color(.7f, .85f, .25f, 0.8f));
            nodeGreenStyle = new GUIStyle();
            nodeGreenStyle.normal.background = MakeTex(2, 2, new Color(.6f, 1f, .8f, 0.6f));
            nodeBlueStyle = new GUIStyle();
            nodeBlueStyle.normal.background = MakeTex(2, 2, new Color(.4f, .7f, .9f, 0.8f));

            inspectorTitleStyle = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
            };
            inspectorTitleStyle.normal.textColor = new Color(1f, .95f, .7f, 0.75f);

            inspectorBgStyle = new GUIStyle();
            inspectorBgStyle.fontSize = 11;
            inspectorBgStyle.alignment = TextAnchor.MiddleCenter;
            inspectorBgStyle.normal.background = MakeTex(2, 2, new Color(.18f, .18f, .18f, 0.9f));
            inspectorBgStyle.normal.textColor = COL_TITLE;

            inspectorButtonStyle = new GUIStyle();
            inspectorButtonStyle.fontSize = 11;
            inspectorButtonStyle.alignment = TextAnchor.MiddleCenter;
            inspectorButtonStyle.normal.background = MakeTex(2, 2, new Color(.18f, .18f, .18f, 0.9f));
            inspectorButtonStyle.normal.textColor = COL_TITLE;
            inspectorButtonStyle.hover.background = MakeTex(2, 2, new Color(.3f, .3f, .3f, 0.9f));
            inspectorButtonStyle.hover.textColor = new Color(1f, .95f, .5f, 0.6f);

        }




        private FivDialogueEditorNode GetMouseOverNode()
        {
            if (nodes == null)
            {
                return null;
            }
            foreach (var item in nodes)
            {
                //if (item.rect.Contains(Event.current.mousePosition))
                //{
                //    return item;
                //}
                if (IsMouseInRect(item.rect))
                {
                    return item;
                }
            }
            return null;
        }
        private bool IsMouseInRect(Rect rect)
        {
            Event e = Event.current;
            return rect.Contains(e.mousePosition - new Vector2(panX - 1000, panY - 1000));
        }
        private bool IsMouseInInspector()
        {
            Rect rect = new Rect(-500, position.yMax - 300, 700, 300);
            Event e = Event.current;
            return rect.Contains(e.mousePosition);
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}



