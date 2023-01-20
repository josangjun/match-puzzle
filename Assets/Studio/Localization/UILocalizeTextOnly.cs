using UnityEngine;
using UnityEngine.UI;

namespace Studio.Common
{
    public class UILocalizeTextOnly : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private string key;

        private void Start()
        {
            text.text = Localization.Get(key);
        }

        private void OnValidate()
        {
            if (text == null)
            {
                text = GetComponent<Text>();
            }
        }
    }
}
