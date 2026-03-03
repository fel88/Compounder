using OpenTK.Mathematics;

namespace Compounder.Interfaces
{
    public interface IDrawingContext
    {
        IEditor Editor { get; }
        CursorPosition GetCursor();        
    }
}
