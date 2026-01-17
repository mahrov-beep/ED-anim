namespace Quantum {
  public unsafe class AttributesSystem : SystemMainThreadFilter<AttributesSystem.Filter>,
    ISignalOnComponentAdded<Attributes> {
    public struct Filter {
        public EntityRef           Entity;
        public Attributes*         Attributes;
        public AttributesTickable* AttributesTickable;
    }

    // Initialises all the data structured contained in this Attributes component, when it is added to the entity
    public void OnAdded(Frame f, EntityRef e, Attributes* component) {
        var attributes           = f.ResolveDictionary(component->DataDictionary);
        var attributesEnumerator = attributes.GetEnumerator();
        while (attributesEnumerator.MoveNext() == true) {
            attributesEnumerator.ValuePtrUnsafe->Init(f, e);
        }
    }

    // Updates all the data structured contained in this Attributes component
    // This applies attributes specific logics such as "Cause one time damage", "Cause damage over time", etc
    public override void Update(Frame f, ref Filter filter) {
        if (!f.IsVerified) {
          return;
        }
      
        var attributes           = f.ResolveDictionary(filter.Attributes->DataDictionary);
        var attributesEnumerator = attributes.GetEnumerator();

        var anyTicked = false;
        while (attributesEnumerator.MoveNext() == true) {
            anyTicked |= attributesEnumerator.ValuePtrUnsafe->Update(f, filter.Entity);
        }
        
        if (!anyTicked) {
          f.Remove<AttributesTickable>(filter.Entity);
        }
    }
}
}