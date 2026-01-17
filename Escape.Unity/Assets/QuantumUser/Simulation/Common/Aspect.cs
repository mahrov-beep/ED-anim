namespace Quantum {
  public unsafe struct Aspect<TComponent1>
    where TComponent1 : unmanaged {

    public EntityRef    entity;
    public TComponent1* c1;
  }

  public unsafe struct Aspect<TComponent1, TComponent2>
    where TComponent1 : unmanaged
    where TComponent2 : unmanaged {

    public EntityRef    entity;
    public TComponent1* c1;
    public TComponent2* c2;
  }

  public unsafe struct Aspect<TComponent1, TComponent2, TComponent3>
    where TComponent1 : unmanaged
    where TComponent2 : unmanaged
    where TComponent3 : unmanaged {

    public EntityRef    entity;
    public TComponent1* c1;
    public TComponent2* c2;
    public TComponent3* c3;
  }

  public unsafe struct Aspect<TComponent1, TComponent2, TComponent3, TComponent4>
    where TComponent1 : unmanaged
    where TComponent2 : unmanaged
    where TComponent3 : unmanaged
    where TComponent4 : unmanaged {

    public EntityRef    entity;
    public TComponent1* c1;
    public TComponent2* c2;
    public TComponent3* c3;
    public TComponent4* c4;
  }
}