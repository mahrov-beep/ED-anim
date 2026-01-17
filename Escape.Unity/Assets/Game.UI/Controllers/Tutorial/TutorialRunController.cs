namespace Game.UI.Controllers.Tutorial {
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using Multicast;

    /// <summary>
    /// Часть ядра туторов. Вызывается из BaseTutorialSequence и никогда вручную.
    /// </summary>
    [RequireFieldsInit]
    public struct TutorialRunControllerArgs : IResultControllerArgs {
        public string                                                      CallerDebugName;
        public List<BaseTutorialSequence>                                  Targets;
        public Func<ControllerBase.Context, BaseTutorialSequence, UniTask> Call;
    }

    public class TutorialRunController : ResultController<TutorialRunControllerArgs> {
        private string debugName = "Tutorial";

        public override string DebugName => this.debugName;

        protected override async UniTask Execute(Context context) {
            foreach (var target in this.Args.Targets) {
#if UNITY_EDITOR
                this.debugName = $"Tutorial: {target.DebugName}.{this.Args.CallerDebugName}";
#endif

                await this.Args.Call(context, target);

                // Если тутор активировался то не вызываем коллбек на остатке туторов,
                // чтобы не допустить выполнение нескольких сценариев одновременно
                if (target.IsActive) {
                    break;
                }
            }
        }
    }
}