namespace Compounder.Interfaces
{
    public interface ITool
    {
        void Deselect();
        void Select();
        void MouseDown(UiMouseEvent mev);
        void MouseUp(UiMouseEvent mev);
        void Draw(IDrawingContext dc);

    }
}
