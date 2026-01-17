namespace Game.UI.Controllers.Tutorial {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Cysharp.Threading.Tasks;
    using JetBrains.Annotations;
    using Multicast;
    using Multicast.FeatureToggles;
    using Multicast.GameProperties;
    using Shared;
    using UnityEngine.Pool;

    public abstract class BaseCoreTutorialSequence {
        [Inject] protected GamePropertiesModel GameProperties;
        [Inject] protected FeatureTogglesModel FeatureTogglesModel;
        
        private readonly List<BaseTutorialSequence> sequences = new();

        public string DebugName => this.GetType().Name;

        /// <summary>
        /// Нужно переопределить в каждом конкретном туторе.
        /// 
        /// Если запущен какой-то тутор, то система будет отправлять все коллбеки только ему,
        /// чтобы не допустить несколько параллельно работающих сценариев туторов.
        /// </summary>
        [PublicAPI]
        public abstract bool IsActive { get; }

        public abstract void ForceComplete();

        /// <summary>
        /// Добавляет новый сценарий тутора. Нужно вызывать только из конструктора TutorialService.
        /// </summary>
        [PublicAPI]
        protected void AddSubSequence(BaseTutorialSequence subSequence) {
            this.sequences.Add(subSequence);
        }

        /// <summary>
        /// Проверяет, активна ли хотя бы одна из подпоследовательностей туториалов.
        /// </summary>
        [PublicAPI]
        protected bool HasAnyActiveSubSequence() {
            return this.sequences.Any(static it => it.IsActive);
        }

        [PublicAPI]
        protected async UniTask ExecuteAsync(
            ControllerBase.Context context,
            Func<ControllerBase.Context, BaseTutorialSequence, UniTask> call,
            [CallerMemberName] string callerDebugName = "") {

            // Ищем первый активный контроллер
            var target = this.sequences.FirstOrDefault(static it => it.IsActive);
            
            var skipTutorials = this.GameProperties.Get(TutorialProperties.Booleans.DebugSkipTutorials) ||
                                this.FeatureTogglesModel.IsDisabled(SharedConstants.Game.FeatureToggles.ShowTutorials);
            if (skipTutorials) {
                target?.ForceComplete();
                return;
            }

            using (ListPool<BaseTutorialSequence>.Get(out var targets)) {
                if (target != null) {
                    // если есть активный тутор, то запускаем коллбек у него
                    targets.Add(target);
                }
                else {
                    // иначе вызываем коллбек у всех в ожидании что кто-то возможно активируется
                    targets.AddRange(this.sequences);
                }

                // Запускаем коллбек через промежуточный контроллер
                // что нужно только чтобы шаг тутора отображался в отладочном окне контроллеров..
                await context.RunForResult(new TutorialRunControllerArgs {
                    CallerDebugName = callerDebugName,
                    Targets         = targets,
                    Call            = call,
                });
            }
        }
    }
}