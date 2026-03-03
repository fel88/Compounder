namespace Compounder
{
    public abstract class AbstractSceneObject
    {
        public virtual double ZOrder { get; set; }
        public bool IsSelected { get; set; }

    }
}
