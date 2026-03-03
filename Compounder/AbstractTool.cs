using Compounder.Interfaces;

namespace Compounder
{
    public class AbstractTool
    {
        public AbstractTool(IEditor editor, IDrawingContext dc)
        {
            DrawingContext = dc;
            Editor = editor;
        }
        public IDrawingContext DrawingContext { get; set; }
        public IEditor Editor { get; set; }
    }
}
