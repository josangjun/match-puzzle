using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Studio.Game
{
    public class AutoToggle : MonoBehaviour
    {
        [SerializeField]
        private Toggle toggle;

        // Start is called before the first frame update
        void Start()
        {
            Stage.Instance.simulator.onAuto.Subscribe(tf =>
                toggle.SetIsOnWithoutNotify(tf)).AddTo(this);
        }

        public void OnValueChanged()
        {
            Stage.Instance.simulator.enabled = toggle.isOn;
        }
    }
}