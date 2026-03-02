namespace Compounder
{
    public interface IUIEvent
    {
        bool Handled { get; set; }
        DrawingContext DrawingContext { get; }
    }
}
