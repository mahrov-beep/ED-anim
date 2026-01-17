namespace Game.ECS.Systems.Unit {
    using System;
    using Quantum;
    using EventBase = Quantum.EventBase;
    using SystemBase = Scellecs.Morpeh.SystemBase;
    public abstract class QuantumEventHandlerSystem<T> : SystemBase where T : EventBase {
        private IDisposable disposable;

        public override void OnAwake() {
            disposable = QuantumEvent.SubscribeManual<T>(OnReceive);
        }

        public override void Dispose() {
            disposable.Dispose();
        }

        public override void OnUpdate(float deltaTime) { }

        protected abstract void OnReceive(T data);

    }
}