using UnityEngine;

namespace News
{
    [CreateAssetMenu(fileName = "New News", menuName = "News/News Data")]
    public class NewsData : ScriptableObject
    {
        public string newsID; // 新闻ID
        public string newsTitle; // 新闻标题
        [TextArea] public string newsContent; // 新闻内容
        public Sprite newsImage; // 新闻图片ID
        public bool isRead; // 是否已读
    }
}