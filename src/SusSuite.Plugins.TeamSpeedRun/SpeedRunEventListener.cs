using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth.Customization;
using SusSuite.Core;
using SusSuite.Core.Services.PluginService;
using SusSuite.Plugins.TeamSpeedRun.Models;

namespace SusSuite.Plugins.TeamSpeedRun
{
    public class SpeedRunEventListener : IEventListener
    {
        private readonly SusSuiteCore _susSuiteCore;

        public SpeedRunEventListener(SusSuiteManager susSuiteManager)
        {
            var susSuiteCorePlugin = susSuiteManager.PluginManager.GetPlugin("SpeedRun");
            _susSuiteCore = susSuiteManager.GetSusSuiteCore(susSuiteCorePlugin);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            new Task(async () =>
            {
                System.Threading.Thread.Sleep(5000);
                _susSuiteCore.LoggerService.LogInformation("start");

                var impostors = e.Game.Players.Where(p => p.Character.PlayerInfo.IsImpostor).ToList();
                var crewMates = e.Game.Players.Where(p => !p.Character.PlayerInfo.IsImpostor).ToList();

                var speedRunData = new SpeedRunData();

                foreach (var clientPlayer in crewMates.Take(e.Game.PlayerCount / 2))
                {
                    if (clientPlayer.Character == null) continue;

                    await clientPlayer.Character.SetColorAsync(ColorType.Blue);
                    speedRunData.BlueTeam.TeamPlayers.Add(clientPlayer.Client.Id);
                    speedRunData.BlueTeam.TotalTasks += clientPlayer.Character.PlayerInfo.Tasks.Count();
                }

                foreach (var clientPlayer in crewMates.Skip(e.Game.PlayerCount / 2))
                {
                    if (clientPlayer.Character == null) continue;

                    await clientPlayer.Character.SetColorAsync(ColorType.Red);
                    speedRunData.RedTeam.TeamPlayers.Add(clientPlayer.Client.Id);
                    speedRunData.RedTeam.TotalTasks += clientPlayer.Character.PlayerInfo.Tasks.Count();
                }

                switch (impostors.Count())
                {
                    case 1:
                        await e.Game.Players.First(p => p.Character.PlayerInfo.IsImpostor).Character.SetColorAsync(ColorType.Pink);
                        break;
                    case 2:
                        await e.Game.Players.First(p => p.Character.PlayerInfo.IsImpostor).Character.SetColorAsync(ColorType.Pink);
                        await e.Game.Players.Last(p => p.Character.PlayerInfo.IsImpostor).Character.SetColorAsync(ColorType.Cyan);
                        break;
                }

                _susSuiteCore.PluginService.SetData(e.Game, speedRunData);

            }).Start();
        }

        [EventListener]
        public void OnTaskDone(IPlayerCompletedTaskEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            new Task(() =>
            {
                _susSuiteCore.PluginService.TryGetData<SpeedRunData>(e.Game, out var speedRunData);

                if (speedRunData.BlueTeam.TeamPlayers.Contains(e.ClientPlayer.Client.Id))
                {
                    speedRunData.BlueTeam.TasksDone++;
                }
                else
                {
                    speedRunData.RedTeam.TasksDone++;
                }

                if (speedRunData.RedTeam.TasksDone == speedRunData.RedTeam.TotalTasks)
                {
                    _susSuiteCore.PluginService.EndGame(e.Game, "Red Team Wins!", 1000, PluginService.WinType.CrewMate, new Vector2(16.5f, -2));
                }
                else if (speedRunData.BlueTeam.TasksDone == speedRunData.BlueTeam.TotalTasks)
                {
                    _susSuiteCore.PluginService.EndGame(e.Game, "Blue Team Wins!", 1000, PluginService.WinType.CrewMate, new Vector2(16.5f, -2));
                }

            }).Start();
        }
    }
}