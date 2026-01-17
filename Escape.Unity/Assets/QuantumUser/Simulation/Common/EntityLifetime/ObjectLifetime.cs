namespace Quantum {
  using Photon.Deterministic;
  public partial struct ObjectLifetime {
    public static ObjectLifetime Create(FP durationSec) {
      var lifetime = new ObjectLifetime();
      lifetime.TTL.Start(durationSec);
      return lifetime;
    }   
    
    public static SetResult Set(Frame f, EntityRef to, FP durationSec) {
      var lifetime = new ObjectLifetime();
      lifetime.TTL.Start(durationSec);

      return f.Set(to, lifetime);
    }
  }
}