using OpenTK.Mathematics;

namespace Compounder
{
    public interface ISceneObject
    {
        bool IsSelected { get; set; }
        void Draw(DrawingContext dc);
        double ZOrder { get; set; }
        Vector2d Location { get; set; }
        bool CheckHovered(DrawingContext dctx, Vector2d location);
        void Event(DrawingContext dctx, IUIEvent ev);
    }
}
