using System;
using System.Collections.Generic;
using Audio;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Puzzle
{
    public enum PuzzleType
    {
        Puzzle,// 拼图
        Kyodai,// 连连看
    }
    public class PuzzlePiece : MonoBehaviour
    {
        public string pieceIndex; // 拼图块的索引
        public List<Sprite> sprites;
        public int currentSpriteIndex = 0;
        public int trueSpriteIndex = 0;
        public bool isCorrect = false;
        public PuzzleType puzzleType = PuzzleType.Puzzle; // 块类型
        
        private Button button;
        
        private void Awake()
        {
            button = GetComponent<Button>();
        }
        private void Start()
        {
            if (button != null)
            {
                button.onClick.AddListener(OnPieceClicked);
            }
            PuzzleManager.Instance.OnReSetAllPuzzlePieces+= ResetPuzzlePiece; // 订阅重置拼图块事件
            ResetPuzzlePiece();
            CheckIfCorrect();
        }

        private void OnPieceClicked()
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Count;
            GetComponent<Image>().sprite = sprites[currentSpriteIndex];
            AudioManager.Instance.PlayEffectAudio("button_menu_audio");
            CheckIfCorrect();
        }

        private void CheckIfCorrect()
        {
            isCorrect = false || currentSpriteIndex == trueSpriteIndex;
            PuzzleManager.Instance.UpdatePieceState(pieceIndex, isCorrect);
            if (puzzleType == PuzzleType.Kyodai && !isCorrect )
            {
                // 恢复暂停的时间
                Time.timeScale = 1f;
                // 延迟1秒后重置拼图块
                Invoke(nameof(TriggerResetPuzzles), 1f);
                
            }
        }
        
        private void TriggerResetPuzzles()
        {
            Debug.Log("执行重置拼图"); // 添加调试日志
            PuzzleManager.Instance.ReSetAllPuzzlePieces();
            Invoke(nameof(PualseGame), 0.1f);
        }
        private void PualseGame()
        {
            // 恢复暂停的时间
            Time.timeScale = 0f;
        }

        public void ResetPuzzlePiece()
        {
            switch (puzzleType)
            {
                case PuzzleType.Puzzle:
                    if (sprites.Count > 0)
                    {
                        currentSpriteIndex = Random.Range(0, sprites.Count);
                    }
                    break;
                case PuzzleType.Kyodai:
                    currentSpriteIndex = 0;
                    break;
            }
            GetComponent<Image>().sprite = sprites[currentSpriteIndex];
        }
    }
}