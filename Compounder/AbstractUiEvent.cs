using Compounder.Interfaces;

namespace Compounder
{
    public abstract class AbstractUiEvent
    {
        public AbstractUiEvent(DrawingContext dc, Control parent, IEditor editor)
        {
            DrawingContext = dc;
            Parent = parent;
            Editor = editor;
        }
        public bool Handled { get; set; }
        public DrawingContext DrawingContext { get; set; }
        public IEditor Editor { get; set; }
        public Control Parent { get; set; }
    }
}
