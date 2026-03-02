using OpenTK.Mathematics;

namespace Compounder
{
    public class UiMouseEvent : IUIEvent
    {
        public Vector2d Location;
        public MouseButtons Button;
        public bool Handled { get; set; }
    }
    public class UiMouseDoubleClickEvent : UiMouseEvent
    {
        
    }
}
