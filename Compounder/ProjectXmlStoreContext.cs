using Compounder.Interfaces;

namespace Compounder
{
    public class ProjectXmlStoreContext
    {
        public CompounderProject Project;
        public List<SceneGroup> Groups = new List<SceneGroup>();
    }
    public class ProjectXmlReStoreContext
    {
        public Dictionary<int, SceneGroup> Groups = new Dictionary<int, SceneGroup>();
    }
}
