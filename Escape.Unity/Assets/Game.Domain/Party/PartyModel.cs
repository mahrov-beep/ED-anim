namespace Game.Domain.Party {
    using System;
    using System.Linq;
    using Multicast;
    using UniMob;

    public class PartyModel : Model {
        public MutableAtom<Guid> Leader { get; } = Atom.Value(Guid.Empty);
        public MutableAtom<Guid[]> Members { get; } = Atom.Value(Array.Empty<Guid>());
        public MutableAtom<Guid[]> ReadyMembers { get; } = Atom.Value(Array.Empty<Guid>());
        
        public MutableAtom<bool> IsSearchingMatch { get; } = Atom.Value(false);
        public MutableAtom<int> MatchmakingTimeRemaining { get; } = Atom.Value(0);

        public PartyModel(Lifetime lifetime) : base(lifetime) { }

        [Atom] public bool IsMember => this.Members.Value.Contains(App.ServerAccessTokenInfo.UserId);
        [Atom] public bool IsLeader => this.Leader.Value == Guid.Empty || this.Leader.Value == App.ServerAccessTokenInfo.UserId;
        [Atom] public int  MemberCount => this.Members.Value.Length;
        [Atom] public bool IsSelfReady => this.ReadyMembers.Value.Contains(App.ServerAccessTokenInfo.UserId);
        [Atom] public bool AreAllReady {
            get {
                var m = this.Members.Value;
                var r = this.ReadyMembers.Value;
                if (m == null || r == null) return false;
                if (m.Length == 0) return false;
                
                foreach (var id in m) {
                    if (id == this.Leader.Value) {
                        continue;
                    }

                    if (!r.Contains(id)) {
                        return false;
                    }
                }
                return true;
            }
        }

        public void Set(Guid leader, Guid[] members, Guid[] readyMembers = null) {
            this.Leader.Value = leader;
            this.Members.Value = members ?? Array.Empty<Guid>();
            this.ReadyMembers.Value = readyMembers ?? Array.Empty<Guid>();
        }

        public void Clear() {
            this.Leader.Value = Guid.Empty;
            this.Members.Value = Array.Empty<Guid>();
            this.ReadyMembers.Value = Array.Empty<Guid>();
            this.IsSearchingMatch.Value = false;
            this.MatchmakingTimeRemaining.Value = 0;
        }

        public void StartMatchmaking() {
            this.IsSearchingMatch.Value = true;
            this.MatchmakingTimeRemaining.Value = 30;
        }

        public void StopMatchmaking() {
            this.IsSearchingMatch.Value = false;
            this.MatchmakingTimeRemaining.Value = 0;
        }

        public void UpdateMatchmakingTime(int seconds) {
            this.MatchmakingTimeRemaining.Value = seconds;
        }
    }
}


