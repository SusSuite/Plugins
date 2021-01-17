using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Impostor.Api.Events;
using Impostor.Api.Events.Meeting;
using Impostor.Api.Events.Player;
using Impostor.Api.Innersloth;
using SusSuite.Core;

namespace SusSuite.Plugins.Sheriff
{
    public class SheriffEventListener : IEventListener
    {
        private readonly SusSuiteCore _susSuiteCore;

        public SheriffEventListener(SusSuiteManager susSuiteManager)
        {
            var susSuiteCorePlugin = susSuiteManager.PluginManager.GetPlugin("Sheriff");
            _susSuiteCore = susSuiteManager.GetSusSuiteCore(susSuiteCorePlugin);
        }

        [EventListener]
        public void OnGameStarted(IGameStartedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            var r = new Random();

            var data = new SheriffData();

            var crewMates = e.Game.Players.Where(p => p.Character?.PlayerInfo.IsImpostor == false).ToList();

            data.SheriffId = crewMates.ElementAt(r.Next(0, crewMates.Count)).Client.Id;

            _susSuiteCore.PluginService.SetData(e.Game, data);
        }

        [EventListener]
        public void OnMeetingStarted(IMeetingStartedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            _susSuiteCore.PluginService.TryGetData<SheriffData>(e.Game, out var data);

            data.InMeeting = true;
            _susSuiteCore.PluginService.SetData(e.Game, data);

            if (data.BeenNotified) return;

            data.BeenNotified = true;
            _susSuiteCore.PluginService.SetData(e.Game, data);

            var sheriff = e.Game.Players.First(p => p.Client.Id == data.SheriffId);

            new Task(async () =>
            {
                System.Threading.Thread.Sleep(5000);
                await _susSuiteCore.PluginService.SendPrivateMessageAsync(sheriff, "pssst...", "You are the Sheriff", "Do you trust your gut?");
            }).Start();
        }

        [EventListener]
        public void OnMeetingEnded(IMeetingEndedEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            new Task(async () =>
            {
                _susSuiteCore.PluginService.TryGetData<SheriffData>(e.Game, out var data);
                data.InMeeting = false;
                _susSuiteCore.PluginService.SetData(e.Game, data);

                if (!data.ExecutionScheduled) return;

                data.ExecutionScheduled = false;
                _susSuiteCore.PluginService.SetData(e.Game, data);

                System.Threading.Thread.Sleep(6000);

                if (e.Game.Players.All(p => p.Client.Id != data.MarkedForDeadId)) return;

                var target = e.Game.Players.First(p => p.Client.Id == data.MarkedForDeadId);

                if (target.Character == null) return;

                if (target.Character.PlayerInfo.IsDead) return;

                if (target.Character.PlayerInfo.IsImpostor)
                {
                    await target.Character.SetMurderedByAsync(target);
                }
                else
                {
                    var impostor = e.Game.Players.First(p =>
                        p.Character != null && !p.Character.PlayerInfo.IsDead && p.Character.PlayerInfo.IsImpostor);

                    await target.Character.SetMurderedByAsync(impostor);

                    if (!e.Game.Players.Any(p => p.Character != null && p.Client.Id == data.SheriffId && !p.Character.PlayerInfo.IsDead)) return;

                    var sheriff = e.Game.Players.First(p => p.Client.Id == data.SheriffId);
                    if (sheriff.Character == null) return;

                    await sheriff.Character.SetMurderedByAsync(impostor);

                    System.Threading.Thread.Sleep(5000);

                    e.Game.Players.ToList().ForEach(async p =>
                    {
                        if (p.Character == null) return;
                        switch (e.Game.Options.Map)
                        {
                            case MapTypes.Skeld:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Skeld);
                                break;
                            case MapTypes.MiraHQ:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Mira);
                                break;
                            case MapTypes.Polus:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Polus);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });

                    e.Game.Players.Reverse().ToList().ForEach(async p =>
                    {
                        if (p.Character == null) return;
                        switch (e.Game.Options.Map)
                        {
                            case MapTypes.Skeld:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Skeld);
                                break;
                            case MapTypes.MiraHQ:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Mira);
                                break;
                            case MapTypes.Polus:
                                await p.Character.NetworkTransform.SnapToAsync(SpawnPoints.Polus);
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    });
                }
            }).Start();
        }

        [EventListener]
        public void OnPlayerMurdered(IPlayerMurderEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            new Task(() =>
            {
                _susSuiteCore.PluginService.TryGetData<SheriffData>(e.Game, out var data);

                if (e.Victim.OwnerId != data.SheriffId) return;
                data.HasShot = true;
                _susSuiteCore.PluginService.SetData(e.Game, data);

            }).Start();
        }

        [EventListener]
        public void OnPlayerChat(IPlayerChatEvent e)
        {
            if (!_susSuiteCore.PluginService.IsGameModeEnabled(e.Game)) return;

            new Task(async () =>
            {
                _susSuiteCore.PluginService.TryGetData<SheriffData>(e.Game, out var data);

                if (e.Game.GameState != GameStates.Started) return;
                if (!data.InMeeting) return;
                if (e.ClientPlayer.Client.Id != data.SheriffId) return;
                if (!e.Message.Trim().StartsWith("/sheriff kill ")) return;

                var commands = e.Message.Trim().Split(' ');
                switch (commands.Length)
                {
                    case 3:
                        var target = commands[2].Trim();
                        if (data.HasShot)
                        {
                            await _susSuiteCore.PluginService.SendPrivateMessageAsync(e.ClientPlayer, "You already shot or your dead.");
                            return;
                        }
                        if (e.Game.Players.Any(p => p.Character?.PlayerInfo.PlayerName == target && !p.Character.PlayerInfo.IsDead))
                        {
                            if (e.ClientPlayer.Character != null && target != e.ClientPlayer.Character.PlayerInfo.PlayerName)
                            {

                                data.MarkedForDeadId = e.Game.Players
                                    .First(p => p.Character?.PlayerInfo.PlayerName == target)
                                    .Client.Id;
                                data.ExecutionScheduled = true;
                                data.HasShot = true;
                                _susSuiteCore.PluginService.SetData(e.Game, data);
                                await _susSuiteCore.PluginService.SendMessageAsync(e.Game, "Execution Scheduled!");
                            }
                            else
                            {
                                await _susSuiteCore.PluginService.SendPrivateMessageAsync(e.ClientPlayer, "You can't kill yourself.");
                            }
                        }
                        else
                        {
                            await _susSuiteCore.PluginService.SendPrivateMessageAsync(e.ClientPlayer, "Couldn't find an Alive Player with that Name.");
                        }
                        break;
                    default:
                        await _susSuiteCore.PluginService.SendPrivateMessageAsync(e.ClientPlayer, "Invalid Command.", "It should look like this:", $"/sheriff kill {e.ClientPlayer.Character?.PlayerInfo.PlayerName}");
                        break;
                }
            }).Start();
        }
    }

    public static class SpawnPoints
    {
        public static Vector2 Skeld = new(0, 5);
        public static Vector2 Mira = new(27, 5);
        public static Vector2 Polus = new(17, -17);
    }
}