using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

namespace Audio
{
    [Serializable]
    public class AudioGameData
    {
        public float mainVolume = 0f;
        public float backgroundVolume = 0f;
        public float effectVolume = 0f;
    }
    public class AudioManager:MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Settings")]
        public AudioMixer audioMixer; // 用于控制音频混合器
        public List<AudioClip> levelAudioClips; // 存储所有音频剪辑的列表
        public List<AudioClip> effectAudioClips; // 存储所有音效剪辑的列表
        public AudioSource backgroundAudioSource; // 用于播放音频的AudioSource组件
        public AudioSource effectAudioSource; // 用于播放音效的AudioSource组件
        
        private AudioGameData audioGameData; // 存储音频设置数据
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
            InitializeAudioSources(); // 初始化音频源
        }

        private void InitializeAudioSources()
        {
            // 初始化音量
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
        public void SetMainVolume(float volume = 0f)
        {
            if (audioMixer != null)
            {// 映射-80到20dB的音量范围
                audioMixer.SetFloat("MainVolume", Mathf.Clamp01(volume) * 100 - 80); // 将0-1范围映射到-80到20dB
            }
            else
            {
                Debug.LogWarning("Audio mixer is not assigned!");
            }
        }
        public void SetBackgroundAudioVolume(float volume = 0f)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("BackgroundVolume", Mathf.Clamp01(volume) * 100 - 80); // 将0-1范围映射到-80到0dB
            }
            else
            {
                Debug.LogWarning("Background audio source is not assigned!");
            }
        }
        
        public void SetEffectAudioVolume(float volume = 0f)
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat("EffectVolume", Mathf.Clamp01(volume) * 100 - 80); // 将0-1范围映射到-80到0dB
            }
            else
            {
                Debug.LogWarning("Effect audio source is not assigned!");
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
        
        public AudioGameData GetAudioGameData()
        {
            audioGameData = new AudioGameData
            {
                mainVolume = audioMixer.GetFloat("MainVolume", out float mainVolume) ? mainVolume : 0f,
                backgroundVolume = audioMixer.GetFloat("BackgroundVolume", out float backgroundVolume) ? backgroundVolume : 0f,
                effectVolume = audioMixer.GetFloat("EffectVolume", out float effectVolume) ? effectVolume : 0f
            };
            return audioGameData; // 返回音频设置数据
        }
        
        public bool SetAudioGameData(AudioGameData data = null)
        {
            try
            {
                if (data == null)
                {
                    data = GetAudioGameData(); // 如果没有传入数据，则获取当前音频设置
                }
                else
                {
                    SetMainVolume(data.mainVolume);
                    SetBackgroundAudioVolume(data.backgroundVolume);
                    SetEffectAudioVolume(data.effectVolume);
                    audioGameData = data; // 更新音频设置数据
                }
                return true; // 如果发生错误，返回true
            }
            catch (Exception e)
            {
                Debug.LogError($"设置音频数据时发生错误: {e.Message}");
                return false; // 如果发生错误，返回false
            }
        }
    }
}