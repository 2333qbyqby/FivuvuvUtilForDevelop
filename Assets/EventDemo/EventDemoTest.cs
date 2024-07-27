using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FivuvuvUtil.Common;
public class EventDemoTest : MonoBehaviour
{
    public Button registerButton;
    public Button unRegisterButton;
    public Button triggerButton;

    private IUnRegister testEvent;
    void Start()
    {
        registerButton.onClick.AddListener(RegisterEvent);
        unRegisterButton.onClick.AddListener(UnRegisterEvent);
        triggerButton.onClick.AddListener(TriggerEvent);
    }

    public void RegisterEvent()
    {
        var temp = testEvent;
        testEvent = EventSystem_QF.Register<OnTriggerEvent>(Test);
        if(temp != null)
        Debug.Log(temp == testEvent);
    }

    public void UnRegisterEvent()
    {
        testEvent.UnRegister();
    }

    public void TriggerEvent()
    {
        EventSystem_QF.Send(new OnTriggerEvent() { count = 1 });
    }
    private void Test(OnTriggerEvent e)
    {
        Debug.Log("Test: " + e.count);
    }
}
