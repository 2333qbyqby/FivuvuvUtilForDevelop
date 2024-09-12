using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class UIPanel : MonoBehaviour
{
    private CanvasGroup canvasGroup;
    private void Awake()
    {
        this.canvasGroup = this.GetComponent<CanvasGroup>();
        
    }
    public void Show()
    {
        this.canvasGroup.alpha = 1;
        this.canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        this.canvasGroup.alpha = 0;
        this.canvasGroup.blocksRaycasts = false;
    }
}
