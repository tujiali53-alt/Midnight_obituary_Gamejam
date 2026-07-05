using UnityEngine;
using UnityEngine.EventSystems;

namespace ObituaryTomorrow.UI
{
    public sealed class PanelTipsHoverForwarder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private PanelTipsHoverController controller;

        public void Bind(PanelTipsHoverController owner)
        {
            controller = owner;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            controller?.NotifyPointerEnter();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            controller?.NotifyPointerExit();
        }
    }
}
