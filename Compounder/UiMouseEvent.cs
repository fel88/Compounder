using Compounder.Interfaces;
using OpenTK.Mathematics;

namespace Compounder
{
    public class UiMouseClickEvent : AbstractUiEvent, IUIEvent
    {
        public CursorPosition Location;
        public MouseButtons Button;
        public UiMouseEventTypeEnum Type;
        public UiMouseClickEvent(DrawingContext dc, Control parent, IEditor editor) : base(dc, parent, editor)
        {
        }

        public enum UiMouseEventTypeEnum
        {
            ButtonDown, ButtonUp
        }
    }
}
