using UnityEngine;
using UnityEngine.UI;

namespace UI.Puzzle
{
    public class PuzzlePiece: MonoBehaviour
    {
        public bool isTruePiece; // 是否为正确的拼图块
        
        private int pieceIndex; // 拼图块索引
        public int pictureIndex; // 图片索引
        private Image pieceImage; // 拼图块的图片组件
        private Button pieceButton; // 拼图块的按钮组件
        private PuzzleGame puzzleGame;
        
        private void Awake()
        {
            pieceImage = GetComponent<Image>(); // 获取拼图块的图片组件
            pieceButton = GetComponent<Button>(); // 获取拼图块的按钮组件
        }
        
        private void Start()
        {
            pieceButton.onClick.AddListener(OnPieceClicked); // 添加点击事件监听器
        }

        public void Initialize(int index, int pictureIndex, Sprite pieceSprite, PuzzleGame puzzleGame)
        {
            pieceIndex = index; // 设置拼图块索引
            this.pictureIndex = pictureIndex; // 设置图片索引
            pieceImage.sprite = pieceSprite; // 设置拼图块的图片
            this.puzzleGame = puzzleGame; // 设置拼图游戏实例
            
            CheckIfTruePiece(); // 检查是否为正确的拼图块
        }
        
        public void SetPiece(int pictureIndex, Sprite pieceSprite)
        {
            this.pictureIndex = pictureIndex; // 设置拼图块索引
            pieceImage.sprite = pieceSprite; // 设置拼图块的图片
            CheckIfTruePiece(); // 检查是否为正确的拼图块
        }

        private void CheckIfTruePiece()
        {
            if (pieceIndex == pictureIndex)
            {
                if (!isTruePiece)
                {
                    isTruePiece = true;
                    pieceButton.interactable = false;
                    puzzleGame.truePieceCount--;
                    if (puzzleGame.truePieceCount <= 0)
                    {
                        Debug.Log("All pieces are correctly placed!");
                        puzzleGame.FinishPuzzle();
                    }
                }
            }
            else
            {
                isTruePiece = false;
            }
        }

        private void OnPieceClicked()
        {
            puzzleGame.GetNextPicture(this);
        }
        
    }
}