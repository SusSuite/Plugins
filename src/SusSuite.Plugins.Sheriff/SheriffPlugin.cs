using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SusSuite.Core;
using SusSuite.Core.Models;

namespace SusSuite.Plugins.Sheriff
{
    public class SheriffPluginStartup : IPluginStartup
    {
        public void ConfigureHost(IHostBuilder host)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IEventListener, SheriffEventListener>();
        }
    }

    [ImpostorPlugin(
        name: "Sheriff",
        package: "SusSuite.Plugins",
        author: "SusSuite",
        version: "1.0.0"
    )]
    public class SheriffPlugin : PluginBase
    {
        private readonly SusSuiteManager _susSuiteManager;
        private readonly SusSuitePlugin _myPluginInfo;

        public SheriffPlugin(SusSuiteManager susSuiteManager)
        {
            _susSuiteManager = susSuiteManager;

            _myPluginInfo = new SusSuitePlugin()
            {
                Name = "Sheriff",
                Description =
                    "One CrewMate will become the Sheriff. You will be notified with a chat message during the first meeting. The Sheriff can type '/sheriff kill name' once per game to kill that player. " +
                    "If that player is a crew mate, the Sheriff will also die, else the Sheriff stays alive.",
                Author = "SusSuite",
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
