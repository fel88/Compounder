using Compounder.Interfaces;
using OpenTK.Mathematics;

namespace Compounder
{
    public class ArrowMoveByAnchorTool : AbstractTool, ITool
    {
        public ArrowMoveByAnchorTool(ArrowSceneObject parent, ArrowMoveAnchor anchor, IEditor editor, IDrawingContext dc) : base(editor, dc)
        {
            Arrow = parent;
            Anchor = anchor;            
        }

        

        ArrowSceneObject Arrow;
        ArrowMoveAnchor Anchor;

        public void Deselect()
        {

        }

        public void Draw(IDrawingContext dc)
        {
            if (isDrag)
            {
                var diff = dc.GetCursor().World - startCursor;
                
                Anchor.Location = startsLocations[Anchor] + diff;
            }
        }
        bool isDrag = false;
        Dictionary<ISceneObject, Vector2d> startsLocations = new Dictionary<ISceneObject, Vector2d>();
        Vector2d startCursor;
        public void MouseDown(UiMouseClickEvent mev)
        {
            startsLocations.Clear();

            startsLocations.Add(Anchor, Anchor.Location);
            startCursor = DrawingContext.GetCursor().World;
            isDrag = true;

        }

        public void MouseUp(UiMouseClickEvent mev)
        {
            isDrag = false;
            Editor.ResetTool();
        }

        public void Select()
        {
            startsLocations.Clear();
            
            startsLocations.Add(Anchor, Anchor.Location);
            startCursor = DrawingContext.GetCursor().World;
            isDrag = true;
        }
    }
}
