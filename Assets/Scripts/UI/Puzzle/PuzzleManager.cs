using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Puzzle
{
    public class PuzzleManager: MonoBehaviour
    {
        [Header("Puzzle Settings")]
        public static PuzzleManager Instance; // 单例实例
        public GameObject panel;
        public GameObject puzzlePanel; // 拼图面板
        public Button closeButton; // 关闭按钮
        public int puzzlePieceCount = 12; // 拼图块数量
        public int truePieceCount = 0; // 正确拼图块数量
        public string puzzleID = "puzzle"; // 拼图面板ID
        public GameObject TrueObject;
        
        [Header("Puzzle Item")]
        public Button puzzleItemButton; // 拼图物品按钮
        public Image puzzleItemImage;
        public GameObject puzzleItemDescription; // 拼图物品描述
        public string puzzleItemID = "PuzzleItem"; // 拼图物品ID
        
        private Dictionary<string,bool> puzzleStateDictionary = new Dictionary<string, bool>(); // 存储拼图块状态
        
        public event Action OnReSetAllPuzzlePieces; // 重置所有拼图块事件
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this; // 设置单例实例
            }
            else
            {
                Destroy(gameObject); // 如果实例已存在，销毁当前对象
            }
            
        }
        
        private void Start()
        {
            closeButton.onClick.AddListener(ClosePanel); // 添加关闭按钮监听器
            puzzleItemButton.onClick.AddListener(CheckPuzzleItem); // 检查拼图物品按钮点击事件
        }

        private void CheckPuzzleItem()
        {
            if (InventoryManager.Instance != null)
            {
                if (InventoryManager.Instance.HasItem(puzzleItemID))
                {
                    puzzleItemImage.gameObject.SetActive(true);
                    puzzleItemDescription.gameObject.SetActive(true);
                    puzzleItemButton.enabled = false; // 禁用拼图物品按钮
                    Debug.Log("使用拼图物品，直接完成拼图");
                    FinishPuzzle();
                }
            }
        }

        public void OpenPanel()
        {
            if (panel != null)
            {
                panel.SetActive(true); // 显示拼图面板
                // 暂停游戏
                Time.timeScale = 0f; // 暂停时间
            }
            else
            {
                Debug.LogError("Puzzle panel is not assigned.");
            }
        }
        public void ClosePanel()
        {
            Time.timeScale = 1f;
            panel.SetActive(false);
        }
        
        public void UpdatePieceState(string pieceIndex, bool state)
        {
            // 更新拼图块状态
            puzzleStateDictionary[pieceIndex] = state;
            truePieceCount = 0; // 重置正确拼图块数量
            foreach (var value in puzzleStateDictionary.Values.Where(value => value))
            {
                truePieceCount++; // 增加正确拼图块数量
            }

            Debug.Log("拼图块 " + pieceIndex + " 状态更新为: " + state);
            Debug.Log("当前正确拼图块数量: " + truePieceCount);
            if (truePieceCount >= puzzlePieceCount) // 检查是否完成拼图
            {
                FinishPuzzle(); // 完成拼图
            }
        }
        private void HidePuzzlePanel()
        {
            // 恢复游戏时间
            Time.timeScale = 1f; // 恢复时间
            puzzlePanel.SetActive(false); // 隐藏拼图面板
        }
        public void FinishPuzzle()
        {
            if (puzzleID == "puzzle")
            {
                // GameStateManager.Instance.SetFlag("show_TimeGate",false);
            }else if (puzzleID == "puzzle2")
            {
                
            }
            TrueObject.SetActive(true);
            GameStateManager.Instance.SetFlag(puzzleID+"Completed", true); // 设置拼图完成标志
            Debug.Log("拼图完成，隐藏拼图面板");
            HidePuzzlePanel(); // 隐藏拼图面板
        }

        public void ReSetAllPuzzlePieces()
        {
            OnReSetAllPuzzlePieces?.Invoke();
        }
    }
}