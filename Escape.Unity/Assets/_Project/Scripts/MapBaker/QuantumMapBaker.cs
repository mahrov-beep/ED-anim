using Quantum;
using UnityEngine;
using UnityEngine.SceneManagement;

[assembly: QuantumMapBakeAssembly]
// ReSharper disable once UnusedType.Global
// ReSharper disable once CheckNamespace
public class QuantumMapBaker : MapDataBakerCallback {
    public override void OnBeforeBake(QuantumMapData data) { }

    public override void OnBake(QuantumMapData data) {
        WayBakerProcessor.Bake(data);
        SpawnsBakerProcessor.Bake(data);

        var scene = SceneManager.GetActiveScene();
        if (scene.name == "MainMenu") {
            return;
        }

        // MinimapBakerProcessor.Bake(data);
    }

    public override void OnBakeNavMesh(QuantumMapData data) { }
}