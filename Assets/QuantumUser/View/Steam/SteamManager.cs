using Steamworks;
using UnityEngine;

public class SteamManager : PersistentSingletonMono<SteamManager>
{
    protected override void Awake()
    {
        base.Awake();

        try
        {
            SteamClient.Init(480, true);
        }
        catch (System.Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    private void OnDisable()
    {
        SteamClient.Shutdown();
    }
}
