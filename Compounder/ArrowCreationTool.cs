using Compounder.Interfaces;
using OpenTK.Mathematics;

namespace Compounder
{
    public class ArrowCreationTool : AbstractTool, ITool
    {
        public ArrowCreationTool(IEditor editor, IDrawingContext dc) : base(editor, dc)
        {



        }


        public void Deselect()
        {

        }

        public void Draw(IDrawingContext dc)
        {
            if (curs.Count == 0)
                return;

            var curp = dc.GetCursor();
            
            var temp = new ArrowSceneObject()
            {
                Source = new ConnectorPoint() { RelativePositon = curs[0].World },
                Target = new ConnectorPoint() { RelativePositon = curp.World }
            };
            temp.TargetAnchor.Location = temp.Target.RelativePositon;
            temp.SourceAnchor.Location = temp.Source.RelativePositon;

            temp.Draw(dc as DrawingContext);
        }

        public void MouseDown(UiMouseClickEvent mev)
        {
            if (mev.Button != MouseButtons.Left)
                return;
            curs.Add(mev.Location);
        }
        List<CursorPosition> curs = new List<CursorPosition>();
        public void MouseUp(UiMouseClickEvent mev)
        {
            if (mev.Button != MouseButtons.Left)
                return;
            curs.Add(mev.Location);
            if (curs.Count == 2)
            {             
                var temp = new ArrowSceneObject()
                {
                    Source = new ConnectorPoint() { RelativePositon = curs[0].World },
                    Target = new ConnectorPoint() { RelativePositon = curs[1].World }
                };
                Editor.StackState();
                Editor.Project.Objects.Add(temp);
                temp.TargetAnchor.Location = temp.Target.RelativePositon;
                temp.SourceAnchor.Location = temp.Source.RelativePositon;
                Editor.ResetTool();
            }
        }

        public void Select()
        {

        }
    }
}
