using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.Framework
{
    [ExecuteInEditMode]
    public class GameObjectSelectable : Selectable
    {
        [System.Serializable]
        protected class GameObjectState {
            public GameObject normalTarget;
            public GameObject highlightedTarget;
            public GameObject pressedTarget;
            public GameObject selectedTarget;
            public GameObject disabledTarget;
            
            public GameObject Get(SelectionState state)
            {
                switch (state)
                {
                    case SelectionState.Highlighted: return highlightedTarget;
                    case SelectionState.Pressed: return pressedTarget;
                    case SelectionState.Selected: return selectedTarget;
                    case SelectionState.Disabled: return disabledTarget;
                    case SelectionState.Normal: 
                    default: return normalTarget;
                }
            }
        }

        [SerializeField]
        private GameObjectState _state;

        public override bool IsInteractable()
        {
            return base.IsInteractable();
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            for (var i = (int)SelectionState.Normal; i <= (int)SelectionState.Disabled; ++i)
            {
                var curIndex = (SelectionState)i;
                if (curIndex == state) continue;
                var target = _state.Get(curIndex);
                target.SetActive(false);
            }
            var selectedState = _state.Get(state);
            selectedState.SetActive(true);
        }
    }
}