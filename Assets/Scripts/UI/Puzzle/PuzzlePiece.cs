using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Puzzle
{
    public class PuzzlePiece : MonoBehaviour
    {
        public string pieceIndex; // 拼图块的索引
        public List<Sprite> sprites;
        public int currentSpriteIndex = 0;
        public int trueSpriteIndex = 0;
        public bool isCorrect = false;
        
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
            if (sprites.Count > 0)
            {
                currentSpriteIndex = Random.Range(0, sprites.Count);
                GetComponent<Image>().sprite = sprites[currentSpriteIndex];
                CheckIfCorrect();
            }
        }

        private void OnPieceClicked()
        {
            currentSpriteIndex = (currentSpriteIndex + 1) % sprites.Count;
            GetComponent<Image>().sprite = sprites[currentSpriteIndex];
            CheckIfCorrect();
        }

        private void CheckIfCorrect()
        {
            isCorrect = false || currentSpriteIndex == trueSpriteIndex;
            PuzzleManager.Instance.UpdatePieceState(pieceIndex, isCorrect);
        }
    }
}