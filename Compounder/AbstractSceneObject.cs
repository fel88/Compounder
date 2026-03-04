using Compounder.Interfaces;

namespace Compounder
{
    public abstract class AbstractSceneObject
    {
        public string Name { get; set; }
        public virtual double ZOrder { get; set; }
        public bool IsSelected { get; set; }
        public bool IsHovered { get; set; }
        public ISceneObject[] Childs { get; set; }
        public SceneGroup Group { get; set; }
        public SceneLayer Layer { get; set; }
    }
}
