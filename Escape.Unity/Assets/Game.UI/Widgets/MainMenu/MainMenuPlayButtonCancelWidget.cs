namespace Game.UI.Widgets.MainMenu {
    using System;
    using Controllers.Features.GameplayStart;
    using Multicast;
    using Shared.DTO;
    using UniMob;
    using UniMob.UI;
    using UnityEngine;
    using Views.MainMenu;

    public class MainMenuPlayButtonCancelWidget : StatefulWidget {
    }

    public class MainMenuPlayButtonCancelState : ViewState<MainMenuPlayButtonCancelWidget>, IMainMenuPlayButtonState {
        [Inject] private Domain.Party.PartyModel partyModel;

        public override WidgetViewReference View => UiConstants.Views.MainMenu.PlayButtonCancel;

        [Atom] public bool IsSearchingMatch => this.partyModel.IsSearchingMatch.Value;
        [Atom] public bool IsLeader         => this.partyModel.IsLeader;

        [Atom]
        public bool IsReady {
            get {
                var members = this.partyModel.Members.Value;

                if (members != null && members.Length > 1) {
                    if (!this.partyModel.AreAllReady) {
                        return false;
                    }
                }

                return true;
            }
        }

        [Atom] public int MatchmakingTimeRemaining => this.partyModel.MatchmakingTimeRemaining.Value;

        public void Play() {   }            
     

        public void Stop() {
            if (this.IsSearchingMatch) {
                this.StopMatchmaking();
            }
        }

        private async void StopMatchmaking() {
            try {
                await App.Server.MatchmakingCancel(new MatchmakingCancelRequest { }, ServerCallRetryStrategy.RetryWithUserDialog);
                this.partyModel.StopMatchmaking();
            }
            catch (Exception ex) {
                Debug.LogWarning($"Failed to cancel matchmaking: {ex.Message}");
            }
        }
    }
}
