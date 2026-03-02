using OpenTK.Mathematics;
using System.Windows.Input;

namespace Compounder
{
    public class UiMouseEvent : AbstractUiEvent, IUIEvent
    {
        public Vector2d Location;
        public MouseButtons Button;

        public UiMouseEvent(DrawingContext dc, Control parent) : base(dc, parent)
        {
        }

        

    }
}
