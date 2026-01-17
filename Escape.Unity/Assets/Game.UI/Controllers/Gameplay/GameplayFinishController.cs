namespace Game.UI.Controllers.Gameplay {
    using System;
    using System.Linq;
    using Cysharp.Threading.Tasks;
    using Features.ExpProgressionRewards;
    using Features.GameResults;
    using Multicast;
    using Quantum;
    using Services.Photon;
    using Shared.DTO;
    using Shared.ServerEvents;
    using Shared.UserProfile.Commands;
    using Shared.UserProfile.Data;
    using UniMob;
    using UnityEngine;

    [Serializable, RequireFieldsInit]
    public struct GameplayFinishControllerArgs : IResultControllerArgs {
        public IScenesController ScenesController;
        public GameSnapshot      gameSnapshot;
    }

    public class GameplayFinishController : ResultController<GameplayFinishControllerArgs> {
        [Inject] private PhotonService photonService;
        [Inject] private SdUserProfile userProfile;

        protected override async UniTask Execute(Context context) {
            // собираем айди и снапшот из текущей сцены
            var gameId       = this.photonService.CurrentGameId;
            var gameSnapshot = this.Args.gameSnapshot;

            foreach (var user in gameSnapshot.Users) {
                user.UserId = photonService.GetPlayerByActorId(user.ActorNumber) is { } player && Guid.TryParse(player.UserId, out var userId)
                    ? userId
                    : Guid.Empty;
            }

            var isLocalPlayerDead = gameSnapshot.Users?.Any(it => it.UserId == App.ServerAccessTokenInfo.UserId && it.IsDead) ?? false;
            if (isLocalPlayerDead) {
                await UniTask.Delay(TimeSpan.FromSeconds(2.5f));
            }

            var backgroundBlackScreen = await context.RunBgScreenDisposable(showDuration: 1.5f);

            using (App.Lifetime.CreateNested(out var lifetime)) {
                var timerAtom         = Atom.Value(lifetime, 10);
                var currentCountAtom  = Atom.Value(lifetime, default(int?));
                var requiredCountAtom = Atom.Value(lifetime, 0);
                var noConnection      = Atom.Value(lifetime, false);

                var messageAtom = Atom.Computed(lifetime, () => {
                    var text = $"Waiting for other players results ({timerAtom.Value} sec)";
                    text += noConnection.Value ? " [No Internet]"
                        : currentCountAtom.Value.HasValue ? $" [{currentCountAtom.Value}/{requiredCountAtom.Value}]"
                        : "";
                    return text;
                });

                // ждем пока достаточное количество игроков отправят свои результаты
                await using (await context.RunProgressScreenDisposable(message: messageAtom, useSystemNavigator: true)) {
                    // слушаем оповещения от сервера о состоянии игры чтобы обновлять UI
                    App.Events.Listen(lifetime, (IGameServerEvent e) => {
                        if (e is SnapshotGameServerEvent snapshotGameServerEvent) {
                            currentCountAtom.Value  = snapshotGameServerEvent.SnapshotCount;
                            requiredCountAtom.Value = snapshotGameServerEvent.RequiredSnapshotCount;
                        }
                    });

                    context.Server.ConnectToGameEvents(lifetime, gameId, isConnectionLost => noConnection.Value = isConnectionLost);

                    // ждем результаты других игроков
                    do {
                        var reportResponse = await context.Server.GameReportSnapshot(new ReportGameSnapshotRequest {
                            GameId       = gameId,
                            GameSnapshot = gameSnapshot,
                        }, ServerCallRetryStrategy.RetryWithUserDialog);

                        if (reportResponse.ShouldWaitForUserPlayerResults == false) {
                            break;
                        }

                        await UniTask.Delay(TimeSpan.FromSeconds(1), DelayType.Realtime);

                        timerAtom.Value -= 1;
                    } while (true);
                }
            }

            // обновляем профиль игрока
            await using (await context.RunProgressScreenDisposable("refreshing_user_profile", useSystemNavigator: true)) {
                await context.Server.ExecuteUserProfile(new UserProfileFetchCommand(), ServerCallRetryStrategy.RetryWithUserDialog);
            }

            // показываем окно результатов игры
            await context.RunForResult(new ShowGameResultsForUnclaimedGamesControllerArgs {
                // Всё еще запущена симуляция игры, так что используем её для ускорения загрузки, а не запускаем локальную
                StartLocalSimulation = false,
            });

            // показываем анимацию повышения уровня
            await context.RunForResult(new ExpProgressionRewardsLevelUpControllerArgs());

            await using (await context.RunLoadingScreenDisposable(useSystemNavigator: true)) {
                await backgroundBlackScreen.DisposeAsync();

                // загружаем пустую сцену
                await this.Args.ScenesController.GoToEmpty(context);

                // отключаемся от photon
                await using (await context.RunProgressScreenDisposable("disconnecting_from_game_server", useSystemNavigator: true)) {
                    await this.photonService.DisconnectAsync(ConnectFailReason.UserRequest);
                }

                // возвращаемся в меню
                await this.Args.ScenesController.GoToMainMenu(context);
            }
        }
    }
}