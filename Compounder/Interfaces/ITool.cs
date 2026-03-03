namespace Compounder.Interfaces
{
    public interface ITool
    {
        void Deselect();
        void Select();
        void MouseDown(UiMouseClickEvent mev);
        void MouseUp(UiMouseClickEvent mev);
        void Draw(IDrawingContext dc);

    }
}
