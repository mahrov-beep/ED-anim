namespace Quantum {

  public unsafe class OnComponentAddedSignalsHandler<T> : SystemSignalsOnly,
          ISignalOnComponentAdded<T> where T : unmanaged, IComponent {

    readonly ComponentHandler<T> _handler;

    public OnComponentAddedSignalsHandler(ComponentHandler<T> handler) => 
            _handler = handler;

    public void OnAdded(Frame f, EntityRef e, T* c) =>
            _handler(f, e, c);

    /*
     void Example() {
      var system = new OnComponentAddedSignalsHandler<UnitAim>((f, e, c) => {
        c->AimEntity = f.Create(c->AimPrototype);
      });
    }
    */
  }
}