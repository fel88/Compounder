using Compounder.Interfaces;
using OpenTK.Mathematics;
using System.Security.Cryptography;

namespace Compounder
{
    public class MoveByAnchorTool : AbstractTool, ITool
    {
        public MoveByAnchorTool(MoveAnchor parent, IEditor editor, IDrawingContext dc) : base(editor, dc)
        {
            MoveAnchor = parent;
            Selected = editor.GetSelected();

        }
        ISceneObject[] Selected;
        MoveAnchor MoveAnchor;
        public void Deselect()
        {

        }

        public void Draw(IDrawingContext dc)
        {
            if (isDrag)
            {
                var diff = dc.GetCursor().World - startCursor;
                foreach (var item in Selected.OfType<IOffsetableSceneObject>())
                {
                    item.SetLocation(startsLocations[item] + diff);
                }
                MoveAnchor.Location = startsLocations[MoveAnchor] + diff;
            }
        }
        bool isDrag = false;
        Dictionary<ISceneObject, Vector2d> startsLocations = new Dictionary<ISceneObject, Vector2d>();
        Vector2d startCursor;
        public void MouseDown(UiMouseClickEvent mev)
        {

        }

        public void MouseUp(UiMouseClickEvent mev)
        {
            isDrag = false;
            Editor.ResetTool();
        }

        public void Select()
        {
            Editor.StackState();
            startsLocations.Clear();
            foreach (var item in Selected)
            {
                startsLocations.Add(item, item.Location);
            }
            startsLocations.Add(MoveAnchor, MoveAnchor.Location);
            startCursor = DrawingContext.GetCursor().World;
            isDrag = true;
        }
    }
}
