using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace FivuvuvUtil.common
{
    public class SceneLoader : MonoSingleton<SceneLoader>
    {

        public Animator animator;//用来播放简单的淡入淡出
        public Image image;
        private const string APPEAR = "Appear";
        [Header("可能的新动画事件")]
        public UnityEvent animationEvent;
        [Header("是否需要简单切换动画")]
        public bool ifNeedSimpleAnimation;
        [Header("场景加载结束事件")]
        public UnityEvent OnNextSceneLoaded;
        [Header("游戏加载中事件")]
        public UnityEvent OnGameLoading;
        //TODO:加载进度条
        private AsyncOperation asyncLoad;
        protected override void Awake()
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
                animator.SetBool(APPEAR, true);//显示加载界面
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
