using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
public enum TypeWriterState
{
    Completed,
    Outputting,
}
public class TextAnimator : MonoBehaviour
{
    [Header("字符输出速度")]
    [Range(1, 255)]
    public byte outputSpeed = 20;
    [Header("字符淡化范围（字数）")]
    public byte fadeRange = 10;

    public TextMeshProUGUI tmpText;

    private Coroutine outputCoroutine;

    public UnityAction outputEndCallback;


    public TypeWriterState State { get; private set; } = TypeWriterState.Completed;


    public byte OutputSpeed
    {
        get => outputSpeed;
        set
        {
            outputSpeed = value;
            CompleteOutput();
        }
    }
    public byte FadeRange
    {
        get => fadeRange;
        set
        {
            fadeRange = value;
            CompleteOutput();
        }
    }
    #region unity生命周期

    private void OnDisable()
    {
        if (State == TypeWriterState.Outputting)
        {
            StopCoroutine(outputCoroutine);
            OnOutputEnd(true);
        }
    }
    #endregion

    public void OutputText(string text)
    {
        if (State == TypeWriterState.Outputting)
        {
            StopCoroutine(outputCoroutine);
        }

        tmpText.text = text;

        outputCoroutine = StartCoroutine(OutputTextWithoutFade());

    }
    /// <summary>
    // 给外部调用的方法，用于立即显示所有字符
    /// </summary>
    public void CompleteOutput()
    {

        if (State == TypeWriterState.Outputting)
        {
            StopCoroutine(outputCoroutine);
            State = TypeWriterState.Completed;
            OnOutputEnd(true);
        }
    }
    private IEnumerator OutputTextWithoutFade(bool skipFirstCharacter = true)
    {
        State = TypeWriterState.Outputting;

        tmpText.maxVisibleCharacters = skipFirstCharacter ? 1 : 0;
        tmpText.ForceMeshUpdate();

        float timer = 0f;
        var interval = 1f / outputSpeed;
        var textInfo = tmpText.textInfo;
        while (tmpText.maxVisibleCharacters < textInfo.characterCount)
        {
            timer += Time.deltaTime;
            if (timer >= interval)
            {
                timer = 0;
                tmpText.maxVisibleCharacters++;
            }
            yield return null;
        }

        State = TypeWriterState.Completed;
        OnOutputEnd(false);
    }
    private void SetCharacterAlpha(int index, byte alpha)
    {
        var materialIndex = tmpText.textInfo.characterInfo[index].materialReferenceIndex;
        var vertexIndex = tmpText.textInfo.characterInfo[index].vertexIndex;
        var vertexColors = tmpText.textInfo.meshInfo[materialIndex].colors32;

        vertexColors[vertexIndex + 0].a = alpha;
        vertexColors[vertexIndex + 1].a = alpha;
        vertexColors[vertexIndex + 2].a = alpha;
        vertexColors[vertexIndex + 3].a = alpha;
    }
    private void OnOutputEnd(bool isShowAllCharacters)
    {
        outputCoroutine = null;
        if (isShowAllCharacters)
        {
            var textInfo = tmpText.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                SetCharacterAlpha(i, 255);
            }

            tmpText.maxVisibleCharacters = textInfo.characterCount;
            tmpText.ForceMeshUpdate();
        }

        outputEndCallback?.Invoke();
        Debug.Log("OutputEnd");
    }
}
