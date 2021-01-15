using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SusSuite.Core;
using SusSuite.Core.Models;

namespace SusSuite.Plugins.Jester
{
    public class JesterPluginStartup : IPluginStartup
    {
        public void ConfigureHost(IHostBuilder host)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventListener, JesterEventListener>();
        }
    }

    [ImpostorPlugin(
        name: "Jester",
        package: "SusSuite.Plugins",
        author: "Gavin Steinhoff",
        version: "1.0.0"
    )]

    public class JesterPlugin : PluginBase
    {
        private readonly SusSuiteManager _susSuiteManager;
        private readonly SusSuitePlugin _myPluginInfo;

        public JesterPlugin(SusSuiteManager susSuiteManager)
        {
            _susSuiteManager = susSuiteManager;

            _myPluginInfo = new SusSuitePlugin()
            {
                Name = "Jester",
                Description = "One CrewMate will become the jester. You will be notified with a chat message during the first meeting. The Jester wins if they are voted out.",
                Author = "Gavin Steinhoff",
                Version = "1.0.0",
                PluginType = PluginType.GameMode,
                PluginColor = "[00aaffff]"
            };
        }

        public override ValueTask EnableAsync()
        {
            _susSuiteManager.PluginManager.RegisterPlugin(_myPluginInfo);
            return default;
        }

        public override ValueTask DisableAsync()
        {
            _susSuiteManager.PluginManager.UnRegisterPlugin(_myPluginInfo);
            return default;
        }
    }
}
