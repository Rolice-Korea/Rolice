using UnityEngine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace Rolice.Particle
{
    public class RcParticleEffect : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ps;
        [SerializeField] private float maxDuration = 5f;

        public event Action OnCompleted;
        public event Action<float> OnProgress;

        private CancellationTokenSource playingCts;

        public void ClearEvents()
        {
            OnCompleted = null;
            OnProgress = null;
        }

        private void OnValidate()
        {
            if (ps == null)
                ps = GetComponent<ParticleSystem>();
        }

        public async UniTask PlayAsync(bool loop = false, float delay = 0f, CancellationToken externalCt = default)
        {
            StopExisting();

            playingCts = CancellationTokenSource.CreateLinkedTokenSource(externalCt);
            var cts = playingCts;

            try
            {
                if (delay > 0)
                    await UniTask.Delay((int)(delay * 1000), cancellationToken: cts.Token);

                ps.Play();

                float elapsed = 0f;
                float duration = ps.main.duration;

                while (ps.isPlaying && elapsed < maxDuration)
                {
                    await UniTask.Yield(cts.Token);
                    elapsed += Time.deltaTime;

                    if (duration > 0)
                        OnProgress?.Invoke(Mathf.Clamp01(elapsed / duration));
                }

                if (loop && !cts.Token.IsCancellationRequested)
                    await PlayAsync(loop: true, delay: 0f, externalCt: cts.Token);
                else
                    OnCompleted?.Invoke();
            }
            catch (OperationCanceledException)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            finally
            {
                if (cts == playingCts)
                    playingCts = null;
                cts.Dispose();
            }
        }

        public void Stop()
        {
            StopExisting();
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        private void StopExisting()
        {
            if (playingCts == null) return;
            
            playingCts.Cancel();
            playingCts.Dispose();
            playingCts = null;
        }

        private void OnDestroy()
        {
            StopExisting();
        }
    }
}
