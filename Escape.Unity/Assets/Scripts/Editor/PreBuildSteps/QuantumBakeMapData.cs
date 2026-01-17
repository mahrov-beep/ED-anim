using Multicast.Build;
using Quantum.Editor;

public class QuantumBakeMapData : PreBuildStep {
    public override void PreBuild(BuildContext context) {
        QuantumEditorAutoBaker.BakeAllScenes_MapData();
    }
}