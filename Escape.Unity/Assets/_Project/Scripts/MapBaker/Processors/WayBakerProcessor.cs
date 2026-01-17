using Quantum;
public class WayBakerProcessor {
    public static void Bake(QuantumMapData data) {
        var ways = QuantumMapDataBaker.FindLocalObjects<QPrototypeWay>(data.gameObject.scene);

        UnityEngine.Assertions.Assert.IsTrue(ways.Count < byte.MaxValue,
                        "Превышение лимита путей на сцене - может быть коллизия айдишников");

        for (var i = 0; i < ways.Count; i++) {
            var way = ways[i];
            way.Prototype.id = (byte)i;
        }
    }
}