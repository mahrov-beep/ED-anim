namespace Scellecs.Morpeh {
    using System;
    using System.Runtime.CompilerServices;
    using JetBrains.Annotations;
    using UnityEngine;

    [Serializable]
    // ReSharper disable once UnusedTypeParameter
    public struct StateMachine<TOwner> where TOwner : struct, IComponent {
        private readonly Entity entity;

        private IFsmStateBase currentState;

        [PublicAPI]
        public Entity Owner => this.entity;

        [PublicAPI]
        public IFsmStateBase CurrentState => this.currentState;

        public int LastChangeFrame { get; private set; }

        public StateMachine(Entity entity) {
            this.entity          = entity;
            this.currentState    = null;
            this.LastChangeFrame = 0;
        }

        [PublicAPI]
        public bool StateIs(IFsmStateBase state) => this.currentState == state;

        [PublicAPI]
        public void GoTo<TComponent>(FsmState<TComponent> state, TComponent data)
            where TComponent : struct, IComponent {
            this.currentState?.Exit(this.entity);
            this.currentState = state;
            state?.Enter(this.entity, data);

            this.LastChangeFrame = Time.frameCount;
        }

        [PublicAPI]
        public void GoTo<TParentComponent, TComponent>(FsmState<TParentComponent, TComponent> state, TParentComponent parentData, TComponent data)
            where TParentComponent : struct, IComponent
            where TComponent : struct, IComponent {
            this.currentState?.Exit(this.entity);
            this.currentState = state;
            state?.Enter(this.entity, parentData, data);

            this.LastChangeFrame = Time.frameCount;
        }
    }

    public interface IFsmStateBase {
        string Name { get; }

        void Exit(Entity entity);
    }

    public class FsmState<TComponent> : IFsmStateBase
        where TComponent : struct, IComponent {
        private Stash<TComponent> componentStash;
        
        public string Name { get; } = typeof(TComponent).Name;

        public void Enter(Entity entity, TComponent data) {
            this.InitComponentStash(entity.world);
            this.componentStash.Set(entity, data);
        }

        public void Exit(Entity entity) {
            this.InitComponentStash(entity.world);
            this.componentStash.Remove(entity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponentStash(World world) {
            if (this.componentStash != null && this.componentStash.world == world) {
                return;
            }

            this.componentStash = world.GetStash<TComponent>();
        }
    }

    public class FsmState<TParentComponent, TComponent> : IFsmStateBase
        where TParentComponent : struct, IComponent
        where TComponent : struct, IComponent {
        private readonly FsmState<TParentComponent> parent;

        private Stash<TComponent> componentStash;

        public string Name => $"{this.parent.Name}.{typeof(TComponent).Name}";

        public FsmState(FsmState<TParentComponent> parent = null) => this.parent = parent;

        public void Enter(Entity entity, TParentComponent parentData, TComponent data) {
            this.parent?.Enter(entity, parentData);
            
            this.InitComponentStash(entity.world);
            this.componentStash.Set(entity, data);
        }

        public void Exit(Entity entity) {
            this.InitComponentStash(entity.world);
            this.componentStash.Remove(entity);

            this.parent?.Exit(entity);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitComponentStash(World world) {
            if (this.componentStash != null && this.componentStash.world == world) {
                return;
            }

            this.componentStash = world.GetStash<TComponent>();
        }
    }
}