using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    [Serializable]
    public class AudioGameData
    {
        [Range(0f, 1f)]
        public float mainVolume = 1f;
        [Range(0f, 1f)]
        public float backgroundVolume = 1f;
        [Range(0f, 1f)]
        public float effectVolume = 1f;
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        
        [Header("Audio Settings")]
        public AudioMixer audioMixer;
        public List<AudioClip> levelAudioClips;
        public List<AudioClip> effectAudioClips;
        public AudioSource backgroundAudioSource;
        public AudioSource effectAudioSource;
        
        private AudioGameData audioGameData;
        // 音频混合器参数名称常量
        private const string MAIN_VOLUME_PARAM = "MainVolume";
        private const string BACKGROUND_VOLUME_PARAM = "BackgroundVolume";
        private const string EFFECT_VOLUME_PARAM = "EffectVolume";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
                SetAudioGameData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void InitializeAudioManager()
        {
            LoadAudioClips();
            InitializeAudioSources();
        }

        private void InitializeAudioSources()
        {
            // 设置音频源的输出组
            if (audioMixer != null)
            {
                var backgroundGroup = audioMixer.FindMatchingGroups("Background");
                var effectGroup = audioMixer.FindMatchingGroups("Effect");
                
                if (backgroundGroup.Length > 0)
                    backgroundAudioSource.outputAudioMixerGroup = backgroundGroup[0];
                
                if (effectGroup.Length > 0)
                    effectAudioSource.outputAudioMixerGroup = effectGroup[0];
            }
        }

        private void LoadAudioClips()
        {
            try
            {
                levelAudioClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Audio/BackgroundAudio"));
                effectAudioClips = new List<AudioClip>(Resources.LoadAll<AudioClip>("Audio/EffectAudio"));
                
                if (levelAudioClips.Count == 0)
                {
                    Debug.LogWarning("No level audio clips found in Resources/Audio/BackgroundAudio");
                }
                if (effectAudioClips.Count == 0)
                {
                    Debug.LogWarning("No effect audio clips found in Resources/Audio/EffectAudio");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to load audio clips: {e.Message}");
            }
        }

        #region 音量控制
        public void SetMainVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                // 正确的dB转换：0对应-80dB，1对应0dB
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(MAIN_VOLUME_PARAM, dbValue);
                
                if (audioGameData != null)
                    audioGameData.mainVolume = volume;
            }
            else
            {
                Debug.LogWarning("Audio mixer is not assigned!");
            }
        }

        public void SetBackgroundAudioVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(BACKGROUND_VOLUME_PARAM, dbValue);
                
                if (audioGameData != null)
                    audioGameData.backgroundVolume = volume;
            }
            else
            {
                Debug.LogWarning("Audio mixer is not assigned!");
            }
        }
        
        public void SetEffectAudioVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioMixer != null)
            {
                float dbValue = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
                audioMixer.SetFloat(EFFECT_VOLUME_PARAM, dbValue);
                
                if (audioGameData != null)
                    audioGameData.effectVolume = volume;
            }
            else
            {
                Debug.LogWarning("Audio mixer is not assigned!");
            }
        }
        #endregion

        #region 音频播放控制
        public bool PlayBackgroundAudio(string levelName)
        {
            if (backgroundAudioSource == null)
            {
                Debug.LogWarning("Background audio source is not assigned!");
                return false;
            }

            string clipName = levelName + "_audio";
            AudioClip clip = levelAudioClips.Find(c => c.name == clipName);
            
            if (clip != null)
            {
                if (backgroundAudioSource.isPlaying && backgroundAudioSource.clip == clip)
                {
                    return true; // 已经在播放相同的音频
                }
                
                backgroundAudioSource.clip = clip;
                backgroundAudioSource.Play();
                return true;
            }
            else
            {
                Debug.LogWarning($"Audio clip '{clipName}' not found!");
                return false;
            }
        }
        
        public bool PlayEffectAudio(string effectName, bool loop = false, float delay = 0f)
        {
            if (effectAudioSource == null)
            {
                Debug.LogWarning("Effect audio source is not assigned!");
                return false;
            }

            AudioClip clip = effectAudioClips.Find(c => c.name == effectName);
            if (clip != null)
            {
                effectAudioSource.clip = clip;
                effectAudioSource.loop = loop;
                
                if (delay > 0f)
                    effectAudioSource.PlayDelayed(delay);
                else
                    effectAudioSource.Play();
                    
                return true;
            }
            else
            {
                Debug.LogWarning($"Effect audio clip '{effectName}' not found!");
                return false;
            }
        }
        
        public void StopBackgroundAudio()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Stop();
            }
        }
        
        public void StopEffectAudio()
        {
            if (effectAudioSource != null && effectAudioSource.isPlaying)
            {
                effectAudioSource.Stop();
            }
        }
        
        public void StopAllAudio()
        {
            StopBackgroundAudio();
            StopEffectAudio();
        }

        public void PauseBackgroundAudio()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.isPlaying)
            {
                backgroundAudioSource.Pause();
            }
        }

        public void ResumeBackgroundAudio()
        {
            if (backgroundAudioSource != null && !backgroundAudioSource.isPlaying && backgroundAudioSource.clip != null)
            {
                backgroundAudioSource.UnPause();
            }
        }
        #endregion

        #region 数据管理
        public AudioGameData GetAudioGameData()
        {
            if (audioGameData == null)
            {
                audioGameData = new AudioGameData();
            }

            if (audioMixer != null)
            {
                // 正确获取音频混合器中的音量值并转换为0-1范围
                if (audioMixer.GetFloat(MAIN_VOLUME_PARAM, out float mainDb))
                {
                    audioGameData.mainVolume = DbToVolume(mainDb);
                }

                if (audioMixer.GetFloat(BACKGROUND_VOLUME_PARAM, out float backgroundDb))
                {
                    audioGameData.backgroundVolume = DbToVolume(backgroundDb);
                }

                if (audioMixer.GetFloat(EFFECT_VOLUME_PARAM, out float effectDb))
                {
                    audioGameData.effectVolume = DbToVolume(effectDb);
                }
            }

            return audioGameData;
        }
        
        public bool SetAudioGameData(AudioGameData data = null)
        {
            try
            {
                if (data == null)
                {
                    Debug.LogWarning("Audio data is null, using default values");
                    data = new AudioGameData();
                }

                SetMainVolume(data.mainVolume);
                SetBackgroundAudioVolume(data.backgroundVolume);
                SetEffectAudioVolume(data.effectVolume);
                
                audioGameData = data;
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"设置音频数据时发生错误: {e.Message}");
                return false;
            }
        }

        #endregion

        #region 工具方法
        private float DbToVolume(float db)
        {
            return db <= -80f ? 0f : Mathf.Pow(10f, db / 20f);
        }

        public bool IsBackgroundAudioPlaying()
        {
            return backgroundAudioSource != null && backgroundAudioSource.isPlaying;
        }

        public bool IsEffectAudioPlaying()
        {
            return effectAudioSource != null && effectAudioSource.isPlaying;
        }

        public float GetBackgroundAudioProgress()
        {
            if (backgroundAudioSource != null && backgroundAudioSource.clip != null)
            {
                return backgroundAudioSource.time / backgroundAudioSource.clip.length;
            }
            return 0f;
        }
        #endregion

        #region Unity生命周期
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                PauseBackgroundAudio();
            }
            else
            {
                ResumeBackgroundAudio();
            }
        }
        #endregion
    }
}