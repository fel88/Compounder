using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Compounder.Interfaces
{
    public interface ISceneObject
    {
        string Name { get; set; }
        bool IsSelected { get; set; }
        void Draw(DrawingContext dc);
        double ZOrder { get; set; }
        Vector2d Location { get; }
        bool CheckHovered(DrawingContext dctx, CursorPosition location);
        void Event(IUIEvent ev);
        XElement ToXml();
        BBox GetBBox();

    }
}
