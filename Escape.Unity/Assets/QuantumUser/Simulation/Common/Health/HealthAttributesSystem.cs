namespace Quantum {
  using UnityEngine.Scripting;

  [Preserve]
  public unsafe class HealthAttributesSystem : SystemMainThreadFilter<HealthAttributesSystem.Filter>,
    ISignalOnComponentAdded<Health> {
    public struct Filter {
      public EntityRef Entity;
      public Health*   Health;
    }

    public void OnAdded(Frame f, EntityRef e, Health* component) {
      component->Init(f, e);
    }

    public override void Update(Frame f, ref Filter filter) {
      filter.Health->Update(f, filter.Entity);
    }
  }
}