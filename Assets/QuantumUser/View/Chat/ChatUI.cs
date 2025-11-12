using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using WebSocketSharp;

public class ChatUI : MonoBehaviour
{
    [Header("Global References")]
    [SerializeField] private InputManager _inputManager;
    [SerializeField] private ChatManager _chatManager;

    [Header("UI References")]
    [SerializeField] private Canvas _chatCanvas;
    [SerializeField] private TextMeshProUGUI _chatLog;
    [SerializeField] private TMP_InputField _chatInput;

    [Header("Settings")]
    [SerializeField] private float _chatHideDelay = 4;

    private bool _isChatFocused;

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
        if (_inputManager.PlayerControls.PersistentMap.ToggleChat.WasPressedThisFrame())
        {
            if (!_isChatFocused)
            {
                ShowChatUI();
                StartTyping();
            }
            else
            {
                TrySendMessage();
            }
        }
    }

    private void ShowChatUI()
    {
        CancelInvoke();
        _chatCanvas.enabled = true;
    }

    private void HideChatUI()
    {
        _chatCanvas.enabled = false;
    }

    private void StartTyping()
    {
        _isChatFocused = true;
        _chatInput.interactable = true;
        _chatInput.ActivateInputField();
    }

    private void FinishTyping()
    {
        _isChatFocused = false;
        _chatInput.interactable = false;
        _chatInput.DeactivateInputField();
    }

    private void TrySendMessage()
    {
        var message = _chatInput.text.Trim();

        if (!string.IsNullOrEmpty(message))
        {
            _chatManager.SendMessageToChannel(message);
            _chatInput.text = "";
        }
        else
        {
            HideChatUI();
        }

        FinishTyping();
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

        ShowChatUI();
        Invoke("HideChatUI", _chatHideDelay);
    }
}
