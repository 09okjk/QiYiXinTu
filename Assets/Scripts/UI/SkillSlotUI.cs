using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class SkillSlotUI: MonoBehaviour
    {
        public Image SkillIcon;
        public TextMeshProUGUI KeyText;
        public TextMeshProUGUI CountText;

        public SkillSlotUI(Image slotImage, TextMeshProUGUI keyText, TextMeshProUGUI countText)
        {
            SkillIcon = slotImage;
            KeyText = keyText;
            CountText = countText;
        }
    }
}