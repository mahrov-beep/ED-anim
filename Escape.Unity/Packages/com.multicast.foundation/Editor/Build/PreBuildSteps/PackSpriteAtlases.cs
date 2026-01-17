namespace Multicast.Build.PreBuildSteps {
    using System;
    using UnityEditor.U2D;

    [Serializable]
    public class PackSpriteAtlases : PreBuildStep {
        public override void PreBuild(BuildContext context) {
            SpriteAtlasUtility.CleanupAtlasPacking();
            SpriteAtlasUtility.PackAllAtlases(context.BuildTarget, canCancel: false);
            SpriteAtlasUtility.CleanupAtlasPacking();
        }
    }
}