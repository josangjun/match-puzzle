using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace Studio.Common
{
    [DefaultExecutionOrder(10)]
    [RequireComponent(typeof(UILocalize), typeof(Graphic))]
    public class UILineSpacing : MonoBehaviour
    {
        public SystemLanguage[] codes = new SystemLanguage[]
        {
            SystemLanguage.English,
            SystemLanguage.Japanese,
            SystemLanguage.Korean,
            SystemLanguage.ChineseTraditional,
            SystemLanguage.ChineseSimplified
        };

        public float spacing = 1f;

        private void OnEnable()
        {
            var lang = Localization.systemLanguage;

            if (codes.Length == 0 || codes.Contains(lang))
            {
                var graphic = GetComponent<Graphic>();
                if (graphic is Text text)
                {
                    text.lineSpacing = spacing;
                }
                else if (graphic is TMPro.TMP_Text tmpText)
                {
                    tmpText.lineSpacing = spacing;
                }
            }
        }
    }
}