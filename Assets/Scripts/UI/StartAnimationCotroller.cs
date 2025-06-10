using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace UI
{
    public class StartAnimationCotroller : MonoBehaviour
    {
        public static StartAnimationCotroller Instance { get; private set; } // 单例实例

        public GameObject skipUI; // 跳过UI
        public VideoPlayer videoPlayer; // 视频播放器
        public GameObject loadingScreen; // 加载/黑屏
        
        [SerializeField] private List<VideoClip> videoClips = new List<VideoClip>(); // 视频剪辑列表
        
        private int currentVideoIndex = 0; // 当前视频索引
        private bool isSkipUIShowing = false;
        private float skipConfirmTimeout = 3.0f; // 跳过确认UI显示时间(秒)
        private float skipUITimer = 0f;
        
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // 确保此对象跨场景不销毁
                // DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            
            // 打开加载屏幕/黑屏，隐藏游戏场景
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
        }
        
        private void Start()
        {
            if (GameStateManager.Instance.GetFlag("StartAnimationFinished"))
            {
                return;
            }
            // 设置视频播放器的播放速度
            videoPlayer.playbackSpeed = 1.0f;
            
            // 设置视频播放器的循环模式
            videoPlayer.isLooping = false;
            
            // 设置视频播放器的音量
            videoPlayer.SetDirectAudioVolume(0, 1.0f);
            
            // 预加载第一个视频但不播放
            if (videoClips.Count > 0)
            {
                videoPlayer.clip = videoClips[0];
                videoPlayer.Prepare();
                
                // 监听准备完成事件
                videoPlayer.prepareCompleted += OnVideoPrepared;
            }
            else
            {
                // 如果没有视频，直接进入游戏
                if (loadingScreen != null)
                {
                    loadingScreen.SetActive(false);
                }
                GameUIManager.Instance.PlaySceneAnimation();
            }
            //videoPlayer.gameObject.SetActive(false); // 确保视频播放器初始状态为不激活
        }
        
        private void OnVideoPrepared(VideoPlayer source)
        {
            // 视频准备好后，移除监听器并开始播放
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            
            // 关闭加载屏幕，开始播放视频
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            
            // 确保视频播放器处于激活状态
            videoPlayer.gameObject.SetActive(true);
            videoPlayer.Play();
            
            // 注册视频结束事件
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        
        private void Update()
        {
            if (GameStateManager.Instance.GetFlag("StartAnimationFinished"))
            {
                return;
            }
            // 如果正在显示跳过确认UI，处理确认逻辑和计时器
            if (isSkipUIShowing)
            {
                // 倒计时
                skipUITimer -= Time.deltaTime;
                
                // 确认跳过
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    // 确认跳过时，检查是否是最后一个视频
                    isSkipUIShowing = false;
                    skipUI.SetActive(false);
                    
                    // 如果是最后一个视频，则直接结束所有视频播放
                    if (currentVideoIndex >= videoClips.Count - 1)
                    {
                        FinishAllVideos();
                    }
                    else
                    {
                        // 不是最后一个视频，则播放下一个
                        PlayVideo(currentVideoIndex + 1);
                    }
                }
                
                // 超时自动隐藏跳过UI
                if (skipUITimer <= 0)
                {
                    isSkipUIShowing = false;
                    skipUI.SetActive(false);
                }
            }
            // 未显示跳过确认UI时，检测跳过输入
            else if (videoPlayer.isPlaying)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    // 显示跳过确认UI
                    skipUI.SetActive(true);
                    isSkipUIShowing = true;
                    skipUITimer = skipConfirmTimeout; // 重置计时器
                }
            }
        }
        
        public void PlayVideo(int i)
        {
            skipUI.SetActive(false);
            currentVideoIndex = i;
            
            // 如果索引越界，结束当前视频播放，关闭视频播放器
            if (i < 0 || i >= videoClips.Count)
            {
                Debug.Log("视频索引越界，结束播放");
                FinishAllVideos();
                return;
            }
            
            Debug.Log($"PlayVideo: {i}");
            
            // 确保视频播放器处于激活状态
            videoPlayer.gameObject.SetActive(true);
            
            // 设置视频播放器的视频剪辑
            videoPlayer.clip = videoClips[i];
            Debug.Log($"videoPlayer.clip.name: {videoPlayer.clip.name}");
            
            // 播放视频
            videoPlayer.Play();
            
            // 注册视频播放结束事件
            videoPlayer.loopPointReached -= OnVideoEnd; // 先移除之前可能存在的事件
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        
        private void OnVideoEnd(VideoPlayer source)
        {
            // 取消注册视频播放结束事件
            videoPlayer.loopPointReached -= OnVideoEnd;
            
            // 增加当前视频索引
            currentVideoIndex++;
            
            // 如果还有更多视频，播放下一个视频
            if (currentVideoIndex < videoClips.Count)
            {
                PlayVideo(currentVideoIndex);
            }
            else
            {
                // 所有视频播放完毕
                FinishAllVideos();
            }
        }
        
        // 添加一个新方法处理所有视频结束的情况
        private void FinishAllVideos()
        {
            // 取消注册事件避免重复调用
            videoPlayer.loopPointReached -= OnVideoEnd;
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            
            // 停止视频播放
            videoPlayer.Stop();
            
            // 清理RenderTexture
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                RenderTexture rt = videoPlayer.targetTexture;
                RenderTexture.active = rt;
                GL.Clear(true, true, Color.clear);
                RenderTexture.active = null;
            }
            
            // 确保视频播放器被禁用
            videoPlayer.gameObject.SetActive(false);
            
            Debug.Log("所有视频播放完毕");
            // 设置游戏状态标志，表示开始动画已完成
            GameStateManager.Instance.SetFlag("StartAnimationFinished", true);
            GameUIManager.Instance.PlaySceneAnimation();
        }
        
        // 公开方法用于外部调用，直接跳过所有视频
        public void SkipAllVideos()
        {
            FinishAllVideos();
        }
    }
}