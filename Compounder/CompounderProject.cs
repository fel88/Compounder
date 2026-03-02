using System.Xml.Linq;

namespace Compounder
{
    public class CompounderProject
    {
        public CompounderProject() { }
        public CompounderProject(XElement element) {

            foreach (var item in element.Elements())
            {
                if (item.Name == "item" )
                {
                    var kind = item.Attribute("kind").Value;
                    if (kind == "image")
                    {
                        Objects.Add(new ImageSceneObject(item));
                    }
                }
            }
        }
        public List<ISceneObject> Objects = new List<ISceneObject>();

        internal XElement ToXml()
        {
            XElement xElement = new XElement("project");
            foreach (var obj in Objects)
            {
                xElement.Add(obj.ToXml());
            }
            return xElement;
        }
    }
}
