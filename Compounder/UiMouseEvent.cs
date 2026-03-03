using Compounder.Interfaces;
using OpenTK.Mathematics;

namespace Compounder
{
    public class UiMouseEvent : AbstractUiEvent, IUIEvent
    {
        public CursorPosition Location;
        public MouseButtons Button;
        public UiMouseEventTypeEnum Type;
        public UiMouseEvent(DrawingContext dc, Control parent, IEditor editor) : base(dc, parent, editor)
        {
        }

        public enum UiMouseEventTypeEnum
        {
            ButtonDown, ButtonUp
        }


    }
}
