namespace Game.ECS.Systems.GameInventory {
    using System.Collections.Generic;
    using Game.ECS.Components.ItemBox;
    using Core;
    using Domain.GameInventory;
    using Game.ECS.Utilities;
    using Multicast;
    using Player;
    using Quantum;
    using UnityEngine;
    using Scellecs.Morpeh;
    using SystemBase = Scellecs.Morpeh.SystemBase;
   
    public class ItemBoxOutlineSystem : SystemBase {
        [Inject] private LocalPlayerSystem       localPlayerSystem;
        [Inject] private QuantumEntityViewSystem quantumEntityViewSystem;
        [Inject] private GameNearbyItemsModel    gameNearbyItemsModel;
        [Inject] private Stash<ItemBoxComponent> itemBoxStash;

        private readonly Dictionary<EntityRef, ItemBoxOutline> outlineCache = new();
        private readonly HashSet<EntityRef>                    visitedBoxes = new();

        private Filter   itemBoxFilter;
        private EntityRef cachedLocalRef = EntityRef.None;

        public override void OnAwake() {
            itemBoxFilter = World.Filter.With<ItemBoxComponent>().Build();
        }

        public override void OnUpdate(float deltaTime) {
            if (localPlayerSystem.HasNotLocalEntityRef(out var localRef)) {
                return;
            }

            if (!quantumEntityViewSystem.TryGetEntityView(localRef, out var localView)) {
                return;
            }

            if (cachedLocalRef != localRef) {
                cachedLocalRef = localRef;
                visitedBoxes.Clear();
            }

            UpdateVisitedBoxes();

            var playerPosition = localView.transform.position;

            foreach (var entity in itemBoxFilter) {
                ref var itemBoxComponent = ref itemBoxStash.Get(entity);
                var view = itemBoxComponent.quantumEntityView;
                if (view == null) {
                    continue;
                }

                var boxEntityRef = view.EntityRef;

                var outline = ResolveOutline(view);
                if (outline == null) {
                    continue;
                }
                
                var itemBoxPosition = itemBoxComponent.position != default
                    ? itemBoxComponent.position
                    : view.transform.position;
                var isNearest = gameNearbyItemsModel.NearbyItemEntity == boxEntityRef;
                var shouldShow   = ShouldShowOutline(outline, itemBoxPosition, playerPosition, boxEntityRef, isNearest);

                outline.SetOutline(shouldShow);
            }
        }

        private void UpdateVisitedBoxes() {
            var opened = gameNearbyItemsModel.OpenedNearbyItemEntity;
            if (opened != EntityRef.None) {
                visitedBoxes.Add(opened);
            }
        }

        private bool ShouldShowOutline(ItemBoxOutline outline, Vector3 itemBoxPosition, Vector3 playerPosition, EntityRef boxEntity, bool isNearest) {
            if (visitedBoxes.Contains(boxEntity)) {
                return false;
            }

            var maxRadius = Mathf.Max(outline.ActivationRadius, 0f);

            if (maxRadius <= 0f) {
                return false;
            }

            if (isNearest) {
                return true;
            }

            if (ScreenSpaceHelper.IsWithinDistance(playerPosition, itemBoxPosition, maxRadius)) {
                return true;
            }

            return false;
        }

        private ItemBoxOutline ResolveOutline(QuantumEntityView view) {
            var entityRef = view.EntityRef;

            if (outlineCache.TryGetValue(entityRef, out var cached) && cached != null) {
                return cached;
            }

            ItemBoxOutline outline = null;
            var components = view.ViewComponents;
            if (components != null) {
                for (int i = 0; i < components.Length; i++) {
                    if (components[i] is ItemBoxOutline found) {
                        outline = found;
                        break;
                    }
                }
            }

            if (outline == null) {
                outline = view.GetComponentInChildren<ItemBoxOutline>(true);
            }

            if (outline != null) {
                outlineCache[entityRef] = outline;
            }
            else {
                outlineCache.Remove(entityRef);
            }

            return outline;
        }
    }
}
