using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace fivuvuvUtil.common
{
    public class SceneLoader : MonoSingleton<SceneLoader>
    {

        public Animator animator;//�������ż򵥵ĵ��뵭��
        public Image image;
        private const string APPEAR = "Appear";
        [Header("���ܵ��¶����¼�")]
        public UnityEvent animationEvent;
        [Header("�Ƿ���Ҫ���л�����")]
        public bool ifNeedSimpleAnimation;
        [Header("�������ؽ����¼�")]
        public UnityEvent OnNextSceneLoaded;
        [Header("��Ϸ�������¼�")]
        public UnityEvent OnGameLoading;
        //TODO:���ؽ�����
        private AsyncOperation asyncLoad;
        override public void Awake()
        {
            base.Awake();
        }
        private void Start()
        {
            DontDestroyOnLoad(this);
        }

        public void LoadSceneSync(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void LoadSceneAsync(string sceneName)
        {
            StartCoroutine(LoadSceneAsyncCR(sceneName));
        }
        public IEnumerator LoadSceneAsyncCR(string sceneName)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, true);//��ʾ���ؽ���
                yield return new WaitForSeconds(1f);
            }

            asyncLoad = SceneManager.LoadSceneAsync(sceneName);

            asyncLoad.completed += AsyncLoad_completed;
            
        }

        private void AsyncLoad_completed(AsyncOperation obj)
        {
            if (ifNeedSimpleAnimation)
            {
                animator.SetBool(APPEAR, false);
            }
            OnNextSceneLoaded?.Invoke();

        }

        public float GetLoadingProgress()
        {
            if (asyncLoad == null)
            {
                return 0;
            }
            else if (asyncLoad.isDone)
            {
                return 1;
            }
            else
            {
                return asyncLoad.progress;
            }
        }
    }
}
