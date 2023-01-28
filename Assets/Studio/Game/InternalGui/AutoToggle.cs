using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Studio.Game
{
    public class AutoToggle : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            var toggle = GetComponent<Toggle>();
            Stage.Instance.simulator.onAuto.Subscribe(tf =>
                toggle.SetIsOnWithoutNotify(tf)).AddTo(this);
        }
    }
}