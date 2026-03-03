using Compounder.Interfaces;

namespace Compounder
{
    public class UiMouseDoubleClickEvent : UiMouseClickEvent
    {
        public UiMouseDoubleClickEvent(DrawingContext dc, Control parent, IEditor editor) : base(dc, parent, editor)
        {
        }
    }
}
