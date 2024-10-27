using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Shapes;

namespace Compounder
{
    public class AItem
    {
        public static int NewId = 0;
        public int Id { get; set; }
        public AItem()
        {
            Id = NewId++;
        }
        public virtual void Event(UiEvent ev)
        {

        }
        public List<AItem> Parents = new List<AItem>();
        public string Name { get; set; }
        public float Width { get; set; } = 40;
        public float Height { get; set; } = 40;
        public float _radius = 40;
        public RectangleF Rect
        {
            get
            {
              
                return new RectangleF(Position.X - Width / 2, Position.Y - Height / 2, Width, Height);
            }
        }
        public float Radius
        {
            get
            {
                return _radius;
            }
            set
            {
                _radius = value;
                Width = value * 2;
                Height = value * 2;
            }
        }

        public void Detach()
        {
            foreach (var item in Parents)
            {
                item.Childs.Remove(this);
            }
            Parents.Clear();
        }
        public static GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            float diameter = radius * 2;
            SizeF size = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }


       
        PointF _pos;
        public float X { get => _pos.X; set => _pos.X = value; }
        public float Y { get => _pos.Y; set => _pos.Y = value; }
        public PointF Position { get => _pos; set => _pos = value; }
        public bool Done { get; set; }
        public bool DrawProgress { get; set; } = true;
        int _progress;
        public int Progress
        {
            get
            {
                if (AutoProgress && Childs.Count > 0)
                {
                    _progress = (int)Math.Round((Childs.Sum(z => z.Progress) / (float)Childs.Count));
                }
                return _progress;
            }
            set => _progress = Math.Max(Math.Min(100, value), 0);
        }
        public bool AutoProgress { get; set; }
        public List<AItem> Childs = new List<AItem>();
     
       

     

        internal void AddChild(AItem aItem)
        {
            Childs.Add(aItem);
            aItem.Parents.Add(this);
        }

        public virtual void ToXml(StringBuilder sb)
        {
            sb.AppendLine($"<item id=\"{Id}\" name=\"{Name}\" drawProgress=\"{DrawProgress}\" autoProgress=\"{AutoProgress}\" progress=\"{Progress}\" pos=\"{Position.X};{Position.Y}\" radius=\"{Radius}\" width=\"{Width}\" height=\"{Height}\" >");
            sb.AppendLine("<childs>");
            foreach (var citem in Childs)
            {
                sb.Append(citem.Id + ";");
            }
            sb.AppendLine("</childs>");
            sb.AppendLine("</item>");
        }
    }
}