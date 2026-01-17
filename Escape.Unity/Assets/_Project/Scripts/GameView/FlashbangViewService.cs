namespace _Project.Scripts.GameView
{
    using System;
    using Knife.Effects;
    using Quantum;
    using UnityEngine;

    public sealed class FlashbangViewService : IDisposable
    {
        private readonly QuantumGame game;
        private readonly FlashbangPostprocess postprocess;
        private IDisposable subscription;
        private bool disposed;

        public FlashbangViewService(QuantumGame game, FlashbangPostprocess postprocess)
        {
            this.game = game ?? throw new ArgumentNullException(nameof(game));
            this.postprocess = postprocess != null ? postprocess : throw new ArgumentNullException(nameof(postprocess));
        }

        public void Initialize()
        {
            GuardNotDisposed();

            subscription?.Dispose();
            subscription = QuantumEvent.SubscribeManual<EventFlashbangBlindUpdate>(OnFlashbangBlindUpdate, game);
        }

        private void OnFlashbangBlindUpdate(EventFlashbangBlindUpdate evt)
        {
            if (disposed || game?.Frames == null)
            {
                return;
            }

            var frame = game.Frames.Predicted;
            if (!frame.TryGet(evt.Target, out Unit unit))
            {
                return;
            }

            var isLocal = frame.Context.IsLocalPlayer(unit.PlayerRef);
            if (!isLocal)
            {
                return;
            }

            var amount = evt.Strength.AsFloat;
            if (amount <= 0.0f)
            {
                return;
            }

            postprocess.BlindWithAmount(Mathf.Clamp01(amount));
        }

        private void GuardNotDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException(nameof(FlashbangViewService));
            }
        }

        public void Dispose()
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            subscription?.Dispose();
            subscription = null;
        }
    }
}
