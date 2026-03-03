namespace Compounder.Interfaces
{
    public interface IEditor
    {
        void ResetTool();
        void SetTool(ITool tool);
        ITool CurrentTool { get; }
        ISceneObject[] GetSelected();
    }
}
