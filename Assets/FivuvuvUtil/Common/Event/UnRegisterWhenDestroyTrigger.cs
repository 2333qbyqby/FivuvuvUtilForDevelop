using System.Collections.Generic;
using UnityEngine;

public class UnRegisterWhenDestroyTrigger : MonoBehaviour
{
    private HashSet<IUnRegister> _unRegisterList = new HashSet<IUnRegister>();

    public void AddUnRegister(IUnRegister ur)
    {
        _unRegisterList.Add(ur);
    }

    public void RemoveUnRegister(IUnRegister ur)
    {
        _unRegisterList.Remove(ur);
    }

    private void OnDestroy()
    {
        foreach (var unRegister in _unRegisterList)
        {
            unRegister.UnRegister();
        }
        _unRegisterList.Clear();
    }
}
