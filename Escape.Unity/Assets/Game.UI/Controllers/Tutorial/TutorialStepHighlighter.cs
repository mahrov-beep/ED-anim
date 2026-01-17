namespace Game.UI.Controllers.Tutorial {
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Multicast.Misc.Tutorial;
    using UniMob.UI.Widgets;
    using UnityEngine;

    public class TutorialStepHighlighter<TStep> where TStep : struct, Enum {
        private readonly Dictionary<TStep, TutorialStepHighlight> highlightedObjects;

        private TStep? highlightedStep;

        [PublicAPI]
        public TStep? CurrentStep { get; private set; }

        public TutorialStepHighlighter(Dictionary<TStep, TutorialStepHighlight> highlight) {
            this.highlightedObjects = highlight;
        }

        [PublicAPI]
        public void ChangeStep(TStep? newStep) {
            if (newStep is { } step && !this.highlightedObjects.ContainsKey(step)) {
                Debug.LogError($"[TutorialStepHighlighter] Step {step} not exist in {nameof(highlightedObjects)} dictionary");
            }

            this.Unhighlight();
            this.CurrentStep = newStep;
            this.HighlightCurrent();
        }

        private void HighlightCurrent() {
            if (this.CurrentStep is not { } currentStep) {
                return;
            }

            if (this.highlightedStep is { } highlighted && EqualityComparer<TStep>.Default.Equals(currentStep, highlighted)) {
                return;
            }

            if (!this.highlightedObjects.TryGetValue(currentStep, out var toEnable)) {
                return;
            }

            this.highlightedStep = currentStep;

            TutorialStatics.AddActiveRoute(toEnable.Route);

            foreach (var toEnableId in toEnable.Objects) {
                TutorialObjectGlobal.Enable(toEnableId);
            }
        }

        private void Unhighlight() {
            if (this.highlightedStep is not { } highlighted) {
                return;
            }

            if (!this.highlightedObjects.TryGetValue(highlighted, out var toDisable)) {
                return;
            }

            this.highlightedStep = null;

            TutorialStatics.RemoveActiveRoute(toDisable.Route);

            foreach (var toDisableId in toDisable.Objects) {
                TutorialObjectGlobal.Disable(toDisableId);
            }
        }
    }

    public struct TutorialStepHighlight {
        public readonly RouteSettings      Route;
        public readonly TutorialObjectID[] Objects;

        public TutorialStepHighlight(RouteSettings route, TutorialObjectID[] objects) {
            this.Route   = route;
            this.Objects = objects;
        }
    }
}