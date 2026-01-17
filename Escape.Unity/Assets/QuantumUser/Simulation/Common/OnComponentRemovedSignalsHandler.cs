namespace Quantum {
  public unsafe class OnComponentRemovedSignalsHandler<T> : SystemSignalsOnly,
          ISignalOnComponentRemoved<T> where T : unmanaged, IComponent {

    readonly ComponentHandler<T> _handler;

    public OnComponentRemovedSignalsHandler(ComponentHandler<T> handler) =>
            _handler = handler;

    public void OnRemoved(Frame f, EntityRef e, T* c) =>
            _handler(f, e, c);

    /*
     void Example() {
      var system = new OnComponentAddedSignalsHandler<UnitAim>((f, e, c) => {
        f.Destroy(c->AimEntity);
      });
    }
    */
  }
}