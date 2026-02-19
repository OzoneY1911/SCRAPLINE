using Quantum;

public class PlayerUI : PersistentSingletonMono<PlayerUI>
{
    private void OnEnable()
    {
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
        Destroy(gameObject);
    }
}
