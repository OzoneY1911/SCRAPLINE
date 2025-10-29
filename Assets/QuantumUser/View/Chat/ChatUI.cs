using Photon.Chat;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private ChatManager _chatManager;
    [SerializeField] private TextMeshProUGUI _chatLog;
    [SerializeField] private TMP_InputField _chatInput;

    private void OnEnable()
    {
        _chatManager.OnMessageReceived += AddMessage;
        _chatManager.OnChatUserSubscribed += HandleNewSubscription;
    }

    private void OnDisable()
    {
        _chatManager.OnMessageReceived -= AddMessage;
        _chatManager.OnChatUserSubscribed -= HandleNewSubscription;
    }

    private void Update()
    {
        if (_inputManager.PlayerControls.PersistentMap.Chat.WasPressedThisFrame())
        {
            _chatManager.SendMessageToChannel(_chatInput.text);
            _chatInput.text = "";
        }
    }

    private void HandleNewSubscription(string username)
    {
        AddMessage($"{username} has joined the chat.");
    }

    private void AddMessage(string message, string sender = "")
    {
        if (sender.IsNullOrEmpty())
        {
            _chatLog.text += $"{message}\n";
        }
        else
        {
            _chatLog.text += $"{sender}: {message}\n";
        }
    }
}
