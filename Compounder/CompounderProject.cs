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
            foreach (var item in element.Elements())
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

        public List<ISceneObject> Objects = new List<ISceneObject>();
        internal XElement ToXml()
        {
            ProjectXmlStoreContext ctx = new ProjectXmlStoreContext();
            XElement xElement = new XElement("project");
            foreach (var obj in Objects)
            {
                var element = obj.ToXml(ctx);
                if (element != null)
                    xElement.Add(element);
            }
            return xElement;
        }
    }
}
