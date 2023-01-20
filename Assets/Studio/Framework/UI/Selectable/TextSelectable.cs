using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Studio.Framework
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Text))]
    public class TextSelectable : Selectable
    {
        [System.Serializable]
        protected class ColorState {
            public Color normalColor;
            public Color highlightedColor;
            public Color pressedColor;
            public Color selectedColor;
            public Color disabledColor;
            public void Set(Color color)
            {
                normalColor = highlightedColor = pressedColor = selectedColor = disabledColor = color;
            }
            public Color Get(SelectionState state)
            {
                switch (state)
                {
                    case SelectionState.Highlighted: return highlightedColor;
                    case SelectionState.Pressed: return pressedColor;
                    case SelectionState.Selected: return selectedColor;
                    case SelectionState.Disabled: return disabledColor;
                    case SelectionState.Normal: 
                    default: return normalColor;
                }
            }
        }

        public override bool IsInteractable()
        {
            return base.IsInteractable();
        }

        [System.Serializable]
        protected class EffectState
        {
            public Shadow shadow;
            public ColorState color = new ColorState();

            public void Set(SelectionState state)
            {
                if (shadow)
                    shadow.effectColor = color.Get(state);
            }
        }

        protected override void Start()
        {
            base.Start();
            Initialize();
        }

        [SerializeField]
        private EffectState[] effects = new EffectState[0];

        [ContextMenu("Initialize")]
        public void Initialize()
        {
            var text = base.targetGraphic;
            var colors = base.colors;
            colors.normalColor = text.color;
            base.colors = colors;

            var shadows = GetComponents<Shadow>();
            if (effects == null)
                effects = new EffectState[shadows.Length];

            for (var i = 0; i < shadows.Length; ++i)
            {
                var shadow = shadows[i];
                if (effects[i] == null || effects[i].shadow == null)
                {
                    effects[i] = new EffectState();
                    effects[i].shadow = shadow;
                    effects[i].color.Set(shadow.effectColor);
                }
            }
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);

            foreach (var effect in effects)
                effect.Set(state);
        }
    }
}