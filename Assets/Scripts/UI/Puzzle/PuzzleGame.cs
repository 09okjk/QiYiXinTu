using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Puzzle
{
    public class PuzzleGame: MonoBehaviour
    {
        public Button closeButton; // 关闭按钮
        public GameObject puzzlePanel; // 拼图面板
        public GameObject puzzlePrefab; // 拼图预制件
        public Image TargetImage; // 目标图片
        [SerializeField] private List<PuzzlePiece> puzzlePieces; // 拼图块列表
        [SerializeField] private List<Sprite> pictures; // 图片列表

        public int truePieceCount;

        private void Awake()
        {
            if (closeButton)
            {
                closeButton.onClick.AddListener(HidePuzzlePanel); // 添加关闭按钮的点击事件监听器
            }
        }
        
        private void Start()
        {
            InitializePuzzlePieces(); // 初始化拼图块
            TargetImage.gameObject.SetActive(false);
        }

        private void InitializePuzzlePieces()
        {
            puzzlePieces = new List<PuzzlePiece>(); // 确保列表已初始化
            for (int i = 0; i < pictures.Count; i++)
            {
                // 创建拼图块实例
                GameObject puzzlePieceObject = Instantiate(puzzlePrefab, puzzlePanel.transform); // 在拼图面板下实例化拼图块预制件
                PuzzlePiece puzzlePiece = puzzlePieceObject.GetComponent<PuzzlePiece>();
                if (puzzlePiece != null)
                {
                    // 初始化拼图块
                    int pictureIndex = Random.Range(0, pictures.Count); // 随机选择图片索引
                    Sprite pieceSprite = pictures[pictureIndex]; // 获取对应的图片
                    puzzlePiece.Initialize(i, pictureIndex, pieceSprite, this); // 假设所有拼图块都是正确的
                    puzzlePieces.Add(puzzlePiece); // 添加到拼图块列表中
                }
                else
                {
                    Debug.LogError("PuzzlePiece component not found on the prefab.");
                }
            }
            truePieceCount = puzzlePieces.Count; // 设置正确拼图块的数量
        }

        public void GetNextPicture(PuzzlePiece piece)
        {
            int nextPictureIndex = piece.pictureIndex + 1 >= pictures.Count ? 0 : piece.pictureIndex + 1; // 获取下一个图片索引
            piece.SetPiece(nextPictureIndex, pictures[nextPictureIndex]); // 设置拼图块的图片
        }
        
        private void HidePuzzlePanel()
        {
            gameObject.SetActive(false); // 隐藏拼图面板
        }
        
        public void FinishPuzzle()
        {
            HidePuzzlePanel(); // 隐藏拼图面板
            TargetImage.gameObject.SetActive(true); // 显示目标图片
        }

    }
}