using UnityEngine;

public class OpenURLButton : MonoBehaviour
{
    [SerializeField] private string _url;

    public void OpenURL()
    {
        Application.OpenURL(_url);
    }
}
