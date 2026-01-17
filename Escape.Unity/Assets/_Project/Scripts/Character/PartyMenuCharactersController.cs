namespace Game.UI.Controllers.Features.Party {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using Domain;
    using Domain.Party;
    using ECS.Components.Unit;
    using Game.ECS.Systems.Unit;
    using Game.Shared.DTO;
    using Multicast;
    using Multicast.Pools;
    using Scellecs.Morpeh;
    using Storage;
    using UniMob;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class PartyMenuCharactersController : LifetimeMonoBehaviour {
        [SerializeField]
        private Transform container;

		[SerializeField]
		private Transform anchor;

		[SerializeField]
		private float sideDistance = 1.25f;

		[SerializeField]
		private float sideStep = 0.25f;

		[SerializeField]
		private float backBase = 1.75f;

		[SerializeField]
		private float backStep = 0.75f;

        [SerializeField]
        private GameObject characterPrefab;

		private readonly Dictionary<Guid, MainMenuRemoteCharacterVisual> visuals = new();

		[SerializeField]
		private float refreshIntervalSec = 5f;

		private readonly Dictionary<Guid, System.Threading.CancellationTokenSource> polling = new();

        private bool needToUpdate = false;

        private void Start() {
            Atom.Reaction(this.Lifetime, () => {
                var model   = App.Get<PartyModel>();
                var members = model.Members.Value;
                UpdateMembers(members);
            });
        }

        protected override void OnDestroy() {
            this.Clean();

            base.OnDestroy();
        }

        private void Clean() {
            var keys = this.visuals.Keys.ToList();
            
            foreach (var key in keys) {
                var view = this.visuals[key];
                
                this.visuals.Remove(key);

                if (this.polling.TryGetValue(key, out var cts)) {
                    cts.Cancel();
                    cts.Dispose();
                    this.polling.Remove(key);
                }
                
                if (view != null) {
                    GameObjectPool.Destroy(view.gameObject);
                }
            }
            
            using (Atom.NoWatch) {
                var world = World.Default;

                var filter = world.Filter.With<UnitPartyComponent>().Build();
                var stash  = world.GetStash<UnitPartyComponent>();
                
                foreach (var entity in filter) {
                    stash.Remove(entity);
                    world.RemoveEntity(entity);
                }
            }
        }

		private void UpdateMembers(Guid[] members) {
            if (this.characterPrefab == null) {
                return;
            }

            var localId = App.ServerAccessTokenInfo.UserId;

            var toKeep = new HashSet<Guid>();
            
            foreach (var id in members) {
                if (id == localId) {
                    continue;
                }
                
                toKeep.Add(id);
                
                if (!this.visuals.ContainsKey(id)) {
					var go = GameObjectPool.Instantiate(this.characterPrefab, Vector3.zero, Quaternion.identity,this.container);
                    this.visuals[id] = go.GetComponent<MainMenuRemoteCharacterVisual>();
					
                    StartPolling(id, this.visuals[id]);
                    
                    using (Atom.NoWatch) {
                        var world = World.Default;
                        var stash = world.GetStash<UnitPartyComponent>();

                        var entity = world.CreateEntity();

                        ref var party = ref stash.Add(entity);

                        party.guid      = id;
                        party.transform = this.visuals[id].transform;
                        party.level     = 0;
                        party.nickName  = "";

                        LoadUserInfo(id, entity).Forget();
                    }
                }
            }

            var toRemove = new List<Guid>();
            
            foreach (var kv in this.visuals) {
                if (!toKeep.Contains(kv.Key)) {
                    toRemove.Add(kv.Key);
                }
            }
			foreach (var rid in toRemove) {
                var view = this.visuals[rid];
                
                this.visuals.Remove(rid);
                
				if (this.polling.TryGetValue(rid, out var cts)) {
					cts.Cancel();
					cts.Dispose();
					this.polling.Remove(rid);

                    using (Atom.NoWatch) {
                        var world = World.Default;
                        
                        var filter = world.Filter.With<UnitPartyComponent>().Build();
                        var stash  = world.GetStash<UnitPartyComponent>();
                        
                        foreach (var entity in filter) {
                            ref var party = ref stash.Get(entity);

                            if (party.guid == rid) {
                                stash.Remove(entity);
                                world.RemoveEntity(entity);
                            }
                        }
                    }
                }
                if (view != null) {
                    GameObjectPool.Destroy(view.gameObject);
                }
            }

			Reposition();
        }

		private void Reposition() {
			var origin = this.anchor != null ? this.anchor : this.container;
			var forward = origin.forward;
			var right = origin.right;

			var keys = new List<Guid>(this.visuals.Keys);
			keys.Sort();

			for (int i = 0; i < keys.Count; i++) {
				var guid = keys[i];
				if (!this.visuals.TryGetValue(guid, out var view) || view == null) continue;

				int row = i / 2;
				bool rightSide = (i % 2) == 1;

				float back = this.backBase + row * this.backStep;
				float side = (this.sideDistance + row * this.sideStep) * (rightSide ? 1f : -1f);

				var worldPos = origin.position + right * side + forward * (-back);
				var rot = Quaternion.LookRotation(forward, Vector3.up);

				var t = view.transform;
				t.position = worldPos;
				t.rotation = rot;
			}
		}

		private void StartPolling(Guid userId, MainMenuRemoteCharacterVisual view) {
			if (this.polling.TryGetValue(userId, out var existing)) {
				existing.Cancel();
				existing.Dispose();
			}
            
			var cts = new CancellationTokenSource();
            
			this.polling[userId] = cts;
            
			PollLoop(userId, view, cts.Token).Forget();
		}

		private async UniTaskVoid PollLoop(Guid userId, MainMenuRemoteCharacterVisual view, System.Threading.CancellationToken token) {
			// initial apply immediately
			await LoadAndApplyOnce(userId, view, token);
			while (!token.IsCancellationRequested) {
				try {
					await UniTask.Delay(TimeSpan.FromSeconds(this.refreshIntervalSec), cancellationToken: token);
				}
				catch { break; }
				await LoadAndApplyOnce(userId, view, token);
			}
		}

		private async UniTask LoadAndApplyOnce(Guid userId, MainMenuRemoteCharacterVisual view, System.Threading.CancellationToken token) {
            try {
				var resp = await App.Server.LoadoutGetByUser(new LoadoutGetByUserRequest { UserId = userId }, ServerCallRetryStrategy.Throw);
                view.ApplyLoadout(resp.Loadout);
            }
            catch (Exception ex) {
                Debug.LogWarning($"Failed to load loadout for user {userId}: {ex.Message}");
            }
		}

		private async UniTaskVoid LoadUserInfo(Guid userId, Entity entity) {
            try {
				var resp = await App.Server.UserGetInfo(new UserGetInfoRequest { UserId = userId }, ServerCallRetryStrategy.Throw);

				var world = World.Default;
				var stash = world.GetStash<UnitPartyComponent>();

				if (stash.Has(entity)) {
					var party = stash.Get(entity);
                    
					party.level = resp.Level;
					party.nickName = resp.NickName;
                    
					stash.Set(entity, party);
				}
            }
            catch (Exception ex) {
                Debug.LogWarning($"Failed to load user info for user {userId}: {ex.Message}");
            }
		}
    }
}


