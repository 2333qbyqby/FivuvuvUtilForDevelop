using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
namespace FivuvuvUtil
{
    public class FivDialoguePlayer : MonoBehaviour
    {
        public FivDialogueAsset asset;
        public bool playOnStart = false;
        [Header("组件")]
        public UIPanel rootDialoguePanel;
        public TextAnimator textAnimator;
        public TextMeshProUGUI contentText;
        public TextMeshProUGUI speakerText;
        public UIPanel choicePanel;
        public GameObject[] choiceObjects;
        public TextMeshProUGUI[] choiceTexts;
        public Image speakerImage;
        [Header("Buttons")]
        public Button contentButton;
        public Button[] choiceButtons;
        [Header("Events")]
        public UnityEvent onDialogueEnded;
        public UnityEvent onDialogueStart;

        private FivDialogueEditorNode curNode, curSentence;
        private List<FivDialogueEditorNode> curChoices;

        private bool curSentenceLinkExecuted = false;
        private bool typewriterFinishExecuted = false;

        #region unity生命周期
        private void Awake()
        {

        }
        private void OnEnable()
        {
            contentButton.onClick.AddListener(OnContentButtonPressed);
            textAnimator.outputEndCallback += OnTypewriterFinished;

        }
        private void Start()
        {
            rootDialoguePanel.Hide();
            choicePanel.Hide();
            if (playOnStart)
            {
                Play();
            }
        }
        private void OnApplicationQuit()
        {
            if (asset != null)
            {
                asset.eventProperties = new List<FivDE_EventProperty>();
                asset.intProperties = new List<FivDE_IntProperty> { };
            }
        }
        #endregion

        #region Public Methods
        public void Play()
        {
            if (asset == null) return;
            FivDialogueManager.curEditorAsset = asset;
            curNode = asset.editorNodes[0];

            //UI
            choicePanel.Hide();

            if (curChoices != null)
            {
                curChoices.Clear();
            }

            if (curNode.type != FivDialogueEditorNodeType.Start)
            {

                Debug.LogError("对话开始节点不是Start");
                return;
            }
            rootDialoguePanel.Show();
            //UI

            StartPlay();
        }
        #region 外部调用
        public void AddListenerToEvent(string eventName, System.Action<float, float> callback)
        {
            bool isNew = true;
            foreach (var item in asset.eventProperties)
            {
                if (item.name.Equals(eventName))
                {
                    isNew = false;
                    item.action += callback;
                }
            }
            if (isNew)
            {
                FivDE_EventProperty e = new FivDE_EventProperty()
                {
                    name = eventName,
                    action = callback,
                };
                asset.eventProperties.Add(e);
                e.action += callback;
            }
        }
        
        public void UpdateIntProperty(string propertyName, int value)
        {
            bool isNew = true;
            foreach (var item in asset.intProperties)
            {
                if (item.name.Equals(propertyName))
                {
                    isNew = false;
                    item.value = value;
                }
            }
            if (isNew)
            {
                asset.intProperties.Add(new FivDE_IntProperty()
                {
                    name = propertyName,
                    value = value,
                });
            }
        }
        #endregion
        #endregion
        #region Private Methodss

        private void StartPlay()
        {
            rootDialoguePanel.Show();
            onDialogueStart.Invoke();
            ExecuteNode(curNode);
        }
        private void OnEndSequence()
        {
            rootDialoguePanel.Hide();
            onDialogueEnded.Invoke();
        }
        private void ExecuteNode(FivDialogueEditorNode node)
        {
            curNode = node;
            switch (node.type)
            {
                case FivDialogueEditorNodeType.Sentence:
                    if (node != curSentence)
                    {
                        curSentenceLinkExecuted = false;
                    }

                    curSentence = node;
                    //打字机
                    typewriterFinishExecuted = false;
                    if (textAnimator)
                    {
                        textAnimator.OutputText(node.info.content);
                    }
                    else
                    {
                        contentText.text = node.info.content;
                    }
                    speakerText.text = node.info.speaker;

                    if (node.info.avatar != null && speakerImage != null)
                    {
                        speakerImage.sprite = Sprite.Create(node.info.avatar, new Rect(0, 0, node.info.avatar.width, node.info.avatar.height), new Vector2(0.5f, 0.5f));
                    }
                    else
                    {
                        if(speakerImage)
                        {
                        speakerImage.sprite = null;
                        }
                    }
                    break;
                case FivDialogueEditorNodeType.Choice:
                    break;
                case FivDialogueEditorNodeType.Random:
                    int count = node.linkedNodesID.Count;
                    ExecuteNode(node.LinkedNodes[Random.Range(0, count)]);
                    break;
                case FivDialogueEditorNodeType.Event:
                    bool isNew = true;
                    foreach (var item in asset.eventProperties)
                    {
                        if (item.name.Equals(node.event_name))
                        {
                            isNew = false;
                            item.action.Invoke(node.event_arg0, node.event_arg1);
                        }
                    }
                    if (isNew)
                    {
                        FivDE_EventProperty e = new FivDE_EventProperty()
                        {
                            name = node.event_name,
                            action = new System.Action<float, float>((a, b) => { }),
                        };
                        asset.eventProperties.Add(e);
                        e.action.Invoke(node.event_arg0, node.event_arg1);
                    }
                    break;
                case FivDialogueEditorNodeType.Start:
                    ExecuteLinkedNodes(curNode);
                    break;
                case FivDialogueEditorNodeType.Set:
                    isNew = true;
                    foreach (var item in asset.intProperties)
                    {
                        if (item.name.Equals(node.int_property_name))
                        {
                            isNew = false;
                            item.value = node.int_property_value;
                        }
                    }
                    if (isNew)
                    {
                        asset.intProperties.Add(new FivDE_IntProperty()
                        {
                            name = node.int_property_name,
                            value = node.int_property_value,
                        });
                    }
                    break;
                case FivDialogueEditorNodeType.If:
                    bool success = true;
                    foreach (var item in asset.intProperties)
                    {
                        if (item.name.Equals(node.if_property_name))
                        {
                            switch (node.if_comparator)
                            {
                                case FivDE_Comparator.Equals:
                                    if (item.value != node.if_property_value)
                                        success = false;
                                    break;
                                case FivDE_Comparator.GreaterThan:
                                    if (item.value <= node.if_property_value)
                                        success = false;
                                    break;
                                case FivDE_Comparator.LessThan:
                                    if (item.value >= node.if_property_value)
                                        success = false;
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    if (success)
                    {
                        ExecuteLinkedNodes(node);
                    }
                    break;
                case FivDialogueEditorNodeType.End:
                    OnEndSequence();
                    break;
                default:
                    break;
            }
        }

        private void ExecuteLinkedNodes(FivDialogueEditorNode node)
        {
            List<FivDialogueEditorNode> list = new List<FivDialogueEditorNode>();
            foreach (var link in node.LinkedNodes)
            {
                if (link.type == FivDialogueEditorNodeType.Set || link.type == FivDialogueEditorNodeType.Event)
                {
                    list.Insert(0, link);
                }
                else if (link.type != FivDialogueEditorNodeType.Choice)
                {
                    list.Add(link);
                }
            }
            foreach (var link in list)
            {
                ExecuteNode(link);
            }
        }
        private void ExecuteLinkedNodesExclueInstant(FivDialogueEditorNode node)
        {
            List<FivDialogueEditorNode> list = new List<FivDialogueEditorNode>();
            foreach (var link in node.LinkedNodes)
            {
                if (link.type == FivDialogueEditorNodeType.Set || link.type == FivDialogueEditorNodeType.Event)
                {

                }
                else if (link.type != FivDialogueEditorNodeType.Choice)
                {
                    list.Add(link);
                }
            }
            foreach (var link in list)
            {
                ExecuteNode(link);
            }
        }

        private void ExecuteInstantNodes(FivDialogueEditorNode node)
        {
            if (node.type != FivDialogueEditorNodeType.Sentence) return;
            if (node.linkedNodesID.Count == 0)
            {
                return;
            }
            var executeList = new List<FivDialogueEditorNode>();
            foreach (var link in node.LinkedNodes)
            {
                if (link.type == FivDialogueEditorNodeType.Set || link.type == FivDialogueEditorNodeType.Event)
                {
                    executeList.Add(link);
                }
            }
            foreach (var link in executeList)
            {
                ExecuteNode(link);
            }
        }

        private void ExecuteChoices(List<FivDialogueEditorNode> choices)
        {
            curChoices = choices;
            if (choices.Count != 0)
            {
                choicePanel.Show();
            }
            foreach (var item in choiceObjects)
            {
                item.SetActive(false);
            }
            for (int i = 0; i < choices.Count; i++)
            {
                choiceObjects[i].SetActive(true);
                choiceTexts[i].text = choices[i].info.content;
                choiceButtons[i].onClick.RemoveAllListeners();
                int id = i;
                choiceButtons[i].onClick.AddListener(() => { OnChoiceButtonPressed(id); });
            }
        }
        #endregion
        #region 中间执行
        private void OnContentButtonPressed()
        {
            if (curSentence.linkedNodesID.Count == 0)
            {
                OnEndSequence();
            }
            if (!typewriterFinishExecuted)
            {

                textAnimator.CompleteOutput();
                OnTypewriterFinished();
            }
            else
            {
                if (!curSentenceLinkExecuted)
                {
                    curSentenceLinkExecuted = true;
                    ExecuteLinkedNodesExclueInstant(curSentence);
                }
            }
        }

        private void OnChoiceButtonPressed(int id)
        {
            choicePanel.Hide();
            ExecuteLinkedNodes(curChoices[id]);
        }
        private void OnTypewriterFinished()
        {
            if (!typewriterFinishExecuted)
            {
                Debug.Log("Typewriter Finished");
                typewriterFinishExecuted = true;
                GotoChoices(curNode);
                ExecuteInstantNodes(curNode);
            }
        }

        private void GotoChoices(FivDialogueEditorNode node)
        {
            if (node.linkedNodesID.Count == 0)
            {
                return;
            }
            List<FivDialogueEditorNode> choices = new List<FivDialogueEditorNode>();
            foreach (var link in node.LinkedNodes)
            {
                if (link.type == FivDialogueEditorNodeType.Choice)
                {
                    choices.Add(link);
                }
            }
            ExecuteChoices(choices);
        }
        #endregion
    }
}