using OpenTK.Mathematics;

namespace Compounder.Interfaces
{
    public interface IOffsetableSceneObject : ISceneObject
    {
        void SetLocation(Vector2d location);
        void Offset(Vector2d v);
    }
}
