using System.Runtime.Loader;

namespace Borks.Plugins
{
    /// <summary>
    /// A class to manage loading, storing, and executing plugins.
    /// </summary>
    public class PluginManager : IDisposable
    {
        public AssemblyLoadContext LoadContext { get; set; }

        public PluginManager()
        {
            LoadContext = new("PluginManager", true);
        }

        public void Dispose()
        {
            LoadContext.Unload();
            GC.SuppressFinalize(this);
        }
    }
}