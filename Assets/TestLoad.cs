using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using fivuvuvUtil.common;
public class TestLoad : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneLoadAddressable.Instance.LoadSceneAsync("Level1");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown)
        {
            SceneLoadAddressable.Instance.LoadNextScene("Level1", "Level2");
        }
            
    }
}
