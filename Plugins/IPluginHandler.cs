using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AURankedPlugin.Plugins
{
    public interface IPluginHandler
    {
        ValueTask onEnableAsync();
        ValueTask onDisableAsync();
    }
}
