using Photon.Deterministic;

namespace Quantum
{
    public partial struct GameplayTimer
    {
        public void Start()
        {
            Remaining = Duration;
            IsRunning = true;
        }

        public void Stop()
        {
            Remaining = FP._0;
            IsRunning = false;
        }

        public void Tick(FP deltaTime)
        {
            if (!IsRunning) return;

            switch (Type)
            {
                case GameplayTimerType.Countdown:
                    Remaining -= deltaTime;
                    if (Remaining <= FP._0) Stop();
                    break;
            }
        }
    }
}
