using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Audio
{
    public class AudioManager:MonoBehaviour
    {
        public List<AudioClip> levelAudioClips; // 存储所有音频剪辑的列表
        public List<AudioClip> effectAudioClips; // 存储所有音效剪辑的列表
        public AudioSource backgroundAudioSource; // 用于播放音频的AudioSource组件
        public AudioSource effectAudioSource; // 用于播放音效的AudioSource组件
        public static AudioManager Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
                DontDestroyOnLoad(gameObject); // 保持在场景切换时不销毁
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，则销毁当前对象
            }
            LoadAudioClips(); // 加载音频剪辑
        }

        private void LoadAudioClips()
        {
            // 从Resources文件夹加载音频剪辑
            levelAudioClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Audio/BackgroundAudio"));
            effectAudioClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Audio/EffectAudio"));
            
            if (levelAudioClips.Count == 0)
            {
                Debug.LogWarning("No level audio clips found in Resources/Audio/LevelAudio");
            }
            if (effectAudioClips.Count == 0)
            {
                Debug.LogWarning("No effect audio clips found in Resources/Audio/EffectAudio");
            }
        }

        public void PlayBackgroundAudio(string levelName)
        {
            string clipName = levelName + "_audio"; // 假设音频剪辑的命名规则为 "关卡名_audio"
            // 查找音频剪辑
            AudioClip clip = levelAudioClips.Find(c => c.name == clipName);
            if (clip != null)
            {
                // 播放音频剪辑
                backgroundAudioSource.clip = clip;
                backgroundAudioSource.Play();
            }
            else
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found!");
            }
        }
        
        public void PlayEffectAudio(string effectName,bool loop = true)
        {
            // 查找音效剪辑
            AudioClip clip = effectAudioClips.Find(c => c.name == effectName);
            if (clip != null)
            {
                // 播放音效剪辑
                effectAudioSource.clip = clip;
                effectAudioSource.loop = loop; // 设置是否循环播放
                effectAudioSource.Play();
            }
            else
            {
                Debug.LogWarning($"Effect audio clip '{effectName}' not found!");
            }
        }
        
        private void StopBackgroundAudio()
        {
            if (backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Stop(); // 停止播放音频
            }
        }
        
        public void StopEffectAudio()
        {
            if (effectAudioSource.isPlaying)
            {
                effectAudioSource.Stop(); // 停止播放音效
            }
        }
        
        public void StopAllAudio()
        {
            StopBackgroundAudio(); // 停止背景音频
            StopEffectAudio(); // 停止所有音效
        }
    }
}