namespace Compounder.Interfaces
{
    public interface IUIEvent
    {
        bool Handled { get; set; }
        DrawingContext DrawingContext { get; }
        IEditor Editor { get; }
    }
}
