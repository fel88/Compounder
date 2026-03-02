using OpenTK.Mathematics;

namespace Compounder
{
    public interface IOffsetableSceneObject : ISceneObject
    {
        void Offset(Vector2d v);
    }
}
