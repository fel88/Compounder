using OpenTK.Mathematics;
using System.Xml.Linq;

namespace Compounder
{
    public interface ISceneObject
    {
        bool IsSelected { get; set; }
        void Draw(DrawingContext dc);
        double ZOrder { get; set; }
        Vector2d Location { get; }
        bool CheckHovered(DrawingContext dctx, Vector2d location);
        void Event(IUIEvent ev);
        XElement ToXml();
        BBox GetBBox();

    }
}
