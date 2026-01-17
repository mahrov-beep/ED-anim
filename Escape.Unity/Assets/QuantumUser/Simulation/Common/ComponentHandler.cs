namespace Quantum {
  public unsafe delegate void ComponentHandler<T>(
          Frame f,
          EntityRef e,
          T* c)
          where T : unmanaged, IComponent;
}