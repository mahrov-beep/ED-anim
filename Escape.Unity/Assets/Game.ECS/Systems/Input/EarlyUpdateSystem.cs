namespace Game.ECS.Systems.Input {
    using Cysharp.Threading.Tasks;
    using UnityEngine;
    using SystemBase = Scellecs.Morpeh.SystemBase;

    /// <summary>
    /// https://gist.github.com/neuecc/bc3a1cfd4d74501ad057e49efcd7bdae#prelateupdate
    /// для систем которым нужно выполняться сразу полсе инпута
    ///
    /// ПЕРЕД:
    /// - апдейтом монобехов
    /// - апдейтом квантума
    /// </summary>
    public abstract class LastPreUpdateSystem : SystemBase {

        private float deltaTime;
        private bool  runEarlyUpdate;

        public sealed override void OnAwake() {
            runEarlyUpdate = true;

            Awake();

            RunLastPreUpdate().Forget();
        }

        public sealed override void Dispose() {
            runEarlyUpdate = false;

            OnDispose();
        }

        public sealed override void OnUpdate(float deltaTime) {
            this.deltaTime = deltaTime;
        }

        private async UniTaskVoid RunLastPreUpdate() {
            while (runEarlyUpdate) {
                await UniTask.Yield(PlayerLoopTiming.LastPreUpdate);

                // if (Application.isEditor) {
                //     Debug.Log($"EarlyUpdate: + {GetType().Name}");
                // }

                OnLastPreUpdate(deltaTime);
            }
        }

        // ReSharper disable once MemberCanBeProtected.Global
        public virtual void OnDispose() { }

        // ReSharper disable once MemberCanBeProtected.Global для удобства перехода
        public abstract void Awake();

        // ReSharper disable once MemberCanBeProtected.Global для удобства перехода
        public abstract void OnLastPreUpdate(float deltaTime);

    }

}