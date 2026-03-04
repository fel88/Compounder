using Compounder.Interfaces;
using System.Xml.Linq;

namespace Compounder
{
    public class CompounderProject
    {
        public CompounderProject() { }
        public CompounderProject(XElement element)
        {
            ProjectXmlReStoreContext ctx = new ProjectXmlReStoreContext();
            var objs = element.Element("objects");
            foreach (var item in objs.Elements())
            {
                if (item.Name == "item")
                {
                    var kind = item.Attribute("kind").Value;
                    if (kind == "image")
                    {
                        Objects.Add(new ImageSceneObject(item));
                    }
                    else
                    if (kind == "arrow")
                    {
                        Objects.Add(new ArrowSceneObject(item));
                    }
                    else
                    if (kind == "rect")
                    {
                        Objects.Add(new RectObject(item, ctx));
                    }
                }
            }
        }

        public List<SceneLayer> Layers = new List<SceneLayer>();
        public List<ISceneObject> Objects = new List<ISceneObject>();
        internal XElement ToXml()
        {
            ProjectXmlStoreContext ctx = new ProjectXmlStoreContext() { Project = this };
            XElement xElement = new XElement("project");
            XElement lel = new XElement("layers");
            XElement objs = new XElement("objects");

            xElement.Add(lel);
            xElement.Add(objs);
            foreach (var item in Layers)
            {
                var x = item.ToXml();
                x.Add(new XAttribute("id", Layers.IndexOf(item)));
                lel.Add(x);
            }

            foreach (var obj in Objects)
            {
                var element = obj.ToXml(ctx);
                if (element != null)
                    objs.Add(element);
            }
            return xElement;
        }
    }
}
