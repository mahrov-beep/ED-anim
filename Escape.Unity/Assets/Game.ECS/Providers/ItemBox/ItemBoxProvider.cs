namespace Game.ECS.Providers.ItemBox {
    using Components.ItemBox;
    using global::Quantum;
    using global::Quantum.Core;
    using Scellecs.Morpeh.Providers;

    [RequireComponent(typeof(QuantumEntityView))]
    public class ItemBoxProvider : MonoProvider<ItemBoxComponent> {
        private void Reset() {
            ref var data = ref GetData();
            data.quantumEntityView = this.GetComponent<QuantumEntityView>();
        }
    }
}