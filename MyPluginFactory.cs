using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Hive.Plugin;
namespace MyFirstPlugin
{
    class MyPluginFactory : IPluginFactory
    {
        
        public IGamePlugin Create(IPluginHost gameHost, string pluginName, Dictionary<string, string> config, out string errorMsg)
        {
            MyFirstPlugin plugin = new MyFirstPlugin();
            if (plugin.SetupInstance(gameHost, config, out errorMsg))
            {
                return plugin;
            }
            return null;
        }
    }
}
