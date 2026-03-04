namespace Compounder.Interfaces
{
    public interface IEditor
    {
        void ResetTool();
        void SetTool(ITool tool);
        ITool CurrentTool { get; }
        CompounderProject Project { get;  }
        ISceneObject[] GetSelected();
        List<ISceneObject> VirtualObjects { get; } 
        SceneLayer ActiveLayer { get; set; }
        void StackState();

    }
}
