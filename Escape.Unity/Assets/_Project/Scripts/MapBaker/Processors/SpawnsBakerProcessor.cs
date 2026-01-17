using Quantum;
public class SpawnsBakerProcessor {
    public static void Bake(QuantumMapData data) {
        var prots = QuantumMapDataBaker.FindLocalObjects<QPrototypeSpawnPoint>(data.gameObject.scene);

        UnityEngine.Assertions.Assert.IsTrue(prots.Count < byte.MaxValue,
                        $"Превышение лимита компонентов {prots.GetType()} на сцене - может быть коллизия айдишников");

        for (var i = 0; i < prots.Count; i++) {
            var prot = prots[i];
            prot.Prototype.ID = (byte)i;
        }
    }
}