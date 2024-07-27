using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FivuvuvUtil.Common
{
    public class AudioManagerForGamejam : MonoSingleton<AudioManagerForGamejam>
    {
        [Header("SFXSource")]
        public AudioSource SFXaudioSource;
        [Header("BGMSource")]
        public AudioSource BGMaudioSource;
        [Header("音频文件")]
        public AudioClip[] SFXClips;
        public AudioClip[] BGMClips;
        public Dictionary<string, AudioClip> audioClipDict = new Dictionary<string, AudioClip>();
        protected override void Awake()
        {
            base.Awake();
            foreach (AudioClip audioClip in SFXClips)//将音频文件放入字典
            {
                audioClipDict.Add(audioClip.name, audioClip);
            }
            foreach (AudioClip audioClip in BGMClips)//将音频文件放入字典
            {
                audioClipDict.Add(audioClip.name, audioClip);
            }
        }
        private void Start()
        {
            DontDestroyOnLoad(this);
        }
        public void PlaySFX(string audioName)
        {
            SFXaudioSource.PlayOneShot(audioClipDict[audioName]);
        }
        //播放BGM
        public void PlayBGM(string audioName)
        {
            BGMaudioSource.clip = audioClipDict[audioName];
            BGMaudioSource.Play();
        }
        
    }
}

