using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#pragma warning disable 649

namespace Studio.Framework
{
    [RequireComponent(typeof(CanvasRenderer))]
    public class Clickable : Graphic, IPointerClickHandler
    {
        public void OnPointerClick(PointerEventData eventData)
        {
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
        }
    }
}