namespace Compounder
{
    public abstract class AbstractUiEvent
    {
        public AbstractUiEvent(DrawingContext dc, Control parent)
        {
            DrawingContext = dc;
            Parent = parent;
        }
        public bool Handled { get; set; }
        public DrawingContext DrawingContext { get; set; }
        public Control Parent { get; set; }
    }
}
