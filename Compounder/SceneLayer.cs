using System.Xml.Linq;

namespace Compounder
{
    public class SceneLayer
    {
        internal XElement ToXml()
        {
            XElement ret = new XElement("layer");
            return ret;
        }
        public bool Visible { get; set; }
        public string Name { get; internal set; }
    }
}
