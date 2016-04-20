using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Fougerite
{
    public class GlobalPluginCollector
    {
        private Dictionary<string, object> AllPlugins;
        private Dictionary<string, string> Types;
        private static GlobalPluginCollector pcollector;

        public GlobalPluginCollector()
        {
            AllPlugins = new Dictionary<string, object>();
            Types = new Dictionary<string, string>();
        }

        public static GlobalPluginCollector GetPluginCollector()
        {
            if (pcollector == null)
            {
                pcollector = new GlobalPluginCollector();
            }
            return pcollector;
        }

        public void AddPlugin(string name, object plugin, string typename)
        {
            AllPlugins[name] = plugin;
            Types[name] = typename;
        }

        public void RemovePlugin(string name)
        {
            if (AllPlugins.Keys.Contains(name))
            {
                AllPlugins.Remove(name);
            }
            if (Types.Keys.Contains(name))
            {
                Types.Remove(name);
            }
        }

        public object GetPlugin(string name)
        {
            if (AllPlugins.Keys.Contains(name))
            {
                return AllPlugins[name];
            }
            return null;
        }

        public string GetPluginType(string name)
        {
            if (Types.Keys.Contains(name))
            {
                return Types[name];
            }
            return null;
        }
    }
}
