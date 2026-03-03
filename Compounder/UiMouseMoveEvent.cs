using Compounder.Interfaces;

namespace Compounder
{
    public class UiMouseMoveEvent : AbstractUiEvent, IUIEvent
    {
        public CursorPosition Location;        
        
        public UiMouseMoveEvent(DrawingContext dc, Control parent, IEditor editor) : base(dc, parent, editor)
        {
        }        
        
    }
}
