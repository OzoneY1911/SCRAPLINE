using Quantum;
using TMPro;
using UnityEngine;
using WebSocketSharp;

public class ChatUI : PersistentSingletonMono<ChatUI>
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

    public bool IsChatFocused => _isChatFocused;

    public bool IsShowingChat;

    protected override void Awake()
    {
        base.Awake();

        _chatCanvas.enabled = false;
    }

    private void OnEnable()
    {
        _chatManager = FindAnyObjectByType<ChatManager>();

        _chatManager.OnMessageReceived += AddMessage;
        _chatManager.OnChatUserSubscribed += HandleNewSubscription;
        QuantumCallback.Subscribe<CallbackGameDestroyed>(this, OnGameDestroyed);
    }

    private void OnGameDestroyed(CallbackGameDestroyed callback)
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        _chatManager.OnMessageReceived -= AddMessage;
        _chatManager.OnChatUserSubscribed -= HandleNewSubscription;
    }

    public void ShowChatUI()
    {
        CancelInvoke();
        _chatCanvas.enabled = true;
        IsShowingChat = true;
    }

    private void HideChatUI()
    {
        _chatCanvas.enabled = false;
        IsShowingChat = false;
    }

    public void StartTyping()
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

    public void TrySendMessage()
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
