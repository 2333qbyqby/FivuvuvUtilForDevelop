using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace fivuvuvUtil.common
{
    /// <summary>
    ///    使用Addressable场景切换,需要在Addressable中设置好场景的AssetReference
    ///    是以AssetReference的方式加载场景
    ///    附加模式加载场景
    ///    待定：是否需要把加载界面的动画做成一个单独的类，或者Dotween的方式
    /// </summary>
    public class SceneLoadAddressable : MonoSingleton<SceneLoadAddressable>
    {
        [Serializable]
        public class SceneAssetReference
        {
            public string sceneName;
            public AssetReference sceneAssetReference;
        }
        [Header("场景AssetReference")]
        public List<SceneAssetReference> sceneAssetReference;
        public SceneAssetReference firstLoadScene;
        [Header("加载场景结束事件")]
        public UnityEvent OnNextSceneLoaded;
        [Header("简单动画")]
        public Animator animator;//用来播放简单的淡入淡出
        public Image image;
        private const string APPEAR = "Appear";
        [Header("参数设置")]
        public bool ifNeedSimpleAnimation;
        override public void Awake()
        {
            base.Awake();
        }
        void Start()
        {
            DontDestroyOnLoad(this);
        }
        //封装的加载场景的方法
        public void LoadNextScene(string currentSceneName, string nextSceneName)
        {
            StartCoroutine(LoadNextSceneCR(currentSceneName, nextSceneName));
        }
        //协程的加载场景的方法，先卸载当前场景，再加载下一个场景
        private IEnumerator LoadNextSceneCR(String currentSceneName,String nextSceneName)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, true);//显示加载界面
                yield return new WaitForSeconds(1f);
            }
            AssetReference currentLoadedSceneAssetReference = GetAssetReferenceByName(currentSceneName);
            yield return currentLoadedSceneAssetReference.UnLoadScene();//addressable的卸载方式,等待卸载完成
            Debug.Log("卸载完成");
            LoadSceneAsync(nextSceneName);
        }

        public void LoadSceneAsync(string sceneName)
        {
            AssetReference currentLoadSceneAssetReference = GetAssetReferenceByName(sceneName);
            var loadSceneOperation = currentLoadSceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive, true);//addressable的加载方式
            loadSceneOperation.Completed += OnLoadCompleted;
        }
        //通过名字获取AssetReference
        public AssetReference GetAssetReferenceByName(string sceneName)
        {
            AssetReference currentLoadSceneAssetReference = null;
            foreach (var item in sceneAssetReference)//遍历查找
            {
                if (item.sceneName == sceneName)
                {
                    currentLoadSceneAssetReference = item.sceneAssetReference;
                    break;
                }
            }
            return currentLoadSceneAssetReference;
        }
        //加载完成后的事件
        private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, false);
            }
            OnNextSceneLoaded?.Invoke();//加载完成后的事件

            //TODO:其他的加载完成后的事件
        }

    }
}
