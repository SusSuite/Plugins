using System;
using System.Linq;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Events.Player;
using SusSuite.Core;
using SusSuite.Core.Services.PluginService;
using SusSuite.Plugins.Jester.Models;

namespace SusSuite.Plugins.Jester
{
    public class JesterEventListener : IEventListener
    {
        private readonly SusSuiteCore _susSuiteCore;

        public JesterEventListener(SusSuiteManager susSuiteManager)
        {
            var susSuiteCorePlugin = susSuiteManager.PluginManager.GetPlugin("Jester");
            _susSuiteCore = susSuiteManager.GetSusSuiteCore(susSuiteCorePlugin);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            var r = new Random();

            var jesterData = new JesterData();

            var crewMates = e.Game.Players.Where(p => p.Character?.PlayerInfo.IsImpostor == false).ToList();

            jesterData.JesterId = crewMates.ElementAt(r.Next(0, crewMates.Count)).Client.Id;

            _susSuiteCore.PluginService.SetData(e.Game, jesterData);
        }

        [EventListener]
        public void OnMeetingStarted(IMeetingStartedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            _susSuiteCore.PluginService.TryGetData<JesterData>(e.Game, out var jesterData); 

            if (jesterData.BeenNotified) return;

            jesterData.BeenNotified = true;

            _susSuiteCore.PluginService.SetData(e.Game, jesterData);

            var jester = e.Game.Players.First(p => p.Client.Id == jesterData.JesterId);

            new Task(async () =>
            {
                System.Threading.Thread.Sleep(5000);
                await _susSuiteCore.PluginService.SendPrivateMessageAsync(jester, "pssst...", "You are the Jester", "Get Voted Out");
            }).Start();
        }

        [EventListener]
        public void OnPlayerExiled(IPlayerExileEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            _susSuiteCore.PluginService.TryGetData<JesterData>(e.Game, out var jesterData);

            if (jesterData.JesterId == e.PlayerControl.OwnerId)
            {
                _susSuiteCore.PluginService.EndGame(e.Game, "Jester Wins!", 11000, PluginService.WinType.Impostor);
            }
        }
    }
}