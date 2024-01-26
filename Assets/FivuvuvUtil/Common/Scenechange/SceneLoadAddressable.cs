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
    ///    ʹ��Addressable�����л�,��Ҫ��Addressable�����úó�����AssetReference
    ///    ����AssetReference�ķ�ʽ���س���
    ///    ����ģʽ���س���
    ///    �������Ƿ���Ҫ�Ѽ��ؽ���Ķ�������һ���������࣬����Dotween�ķ�ʽ
    /// </summary>
    public class SceneLoadAddressable : MonoSingleton<SceneLoadAddressable>
    {
        [Serializable]
        public class SceneAssetReference
        {
            public string sceneName;
            public AssetReference sceneAssetReference;
        }
        [Header("����AssetReference")]
        public List<SceneAssetReference> sceneAssetReference;
        public SceneAssetReference firstLoadScene;
        [Header("���س��������¼�")]
        public UnityEvent OnNextSceneLoaded;
        [Header("�򵥶���")]
        public Animator animator;//�������ż򵥵ĵ��뵭��
        public Image image;
        private const string APPEAR = "Appear";
        [Header("��������")]
        public bool ifNeedSimpleAnimation;
        override public void Awake()
        {
            base.Awake();
        }
        void Start()
        {
            DontDestroyOnLoad(this);
        }
        //��װ�ļ��س����ķ���
        public void LoadNextScene(string currentSceneName, string nextSceneName)
        {
            StartCoroutine(LoadNextSceneCR(currentSceneName, nextSceneName));
        }
        //Э�̵ļ��س����ķ�������ж�ص�ǰ�������ټ�����һ������
        private IEnumerator LoadNextSceneCR(String currentSceneName,String nextSceneName)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, true);//��ʾ���ؽ���
                yield return new WaitForSeconds(1f);
            }
            AssetReference currentLoadedSceneAssetReference = GetAssetReferenceByName(currentSceneName);
            yield return currentLoadedSceneAssetReference.UnLoadScene();//addressable��ж�ط�ʽ,�ȴ�ж�����
            Debug.Log("ж�����");
            LoadSceneAsync(nextSceneName);
        }

        public void LoadSceneAsync(string sceneName)
        {
            AssetReference currentLoadSceneAssetReference = GetAssetReferenceByName(sceneName);
            var loadSceneOperation = currentLoadSceneAssetReference.LoadSceneAsync(LoadSceneMode.Additive, true);//addressable�ļ��ط�ʽ
            loadSceneOperation.Completed += OnLoadCompleted;
        }
        //ͨ�����ֻ�ȡAssetReference
        public AssetReference GetAssetReferenceByName(string sceneName)
        {
            AssetReference currentLoadSceneAssetReference = null;
            foreach (var item in sceneAssetReference)//��������
            {
                if (item.sceneName == sceneName)
                {
                    currentLoadSceneAssetReference = item.sceneAssetReference;
                    break;
                }
            }
            return currentLoadSceneAssetReference;
        }
        //������ɺ���¼�
        private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, false);
            }
            OnNextSceneLoaded?.Invoke();//������ɺ���¼�

            //TODO:�����ļ�����ɺ���¼�
        }

    }
}
