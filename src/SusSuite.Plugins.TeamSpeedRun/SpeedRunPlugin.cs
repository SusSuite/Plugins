using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SusSuite.Core;
using SusSuite.Core.Models;

namespace SusSuite.Plugins.TeamSpeedRun
{
    public class JesterPluginStartup : IPluginStartup
    {
        public void ConfigureHost(IHostBuilder host)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventListener, SpeedRunEventListener>();
        }
    }

    [ImpostorPlugin(
        name: "SpeedRun",
        package: "SusSuite.Plugins",
        author: "Gavin Steinhoff",
        version: "1.0.0"
    )]
    public class SpeedRunPlugin : PluginBase
    {
        private readonly SusSuiteManager _susSuiteManager;
        private readonly SusSuitePlugin _myPluginInfo;

        public SpeedRunPlugin(SusSuiteManager susSuiteManager)
        {
            _susSuiteManager = susSuiteManager;

            _myPluginInfo = new SusSuitePlugin()
            {
                Name = "SpeedRun",
                Description = "Who can complete their task's first?",
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
