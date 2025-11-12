using UnityEngine;
using Photon.Chat;
using Quantum;
using System.Collections.Generic;
using System;
using Quantum.Menu;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    private ChatClient _chatClient;
    private string _userName = "Player";
    private string _channelName = "Text Chat";

    public event Action<string> OnChatUserSubscribed;
    public event Action<string, string> OnMessageReceived;

    private void Start()
    {
        _userName += UnityEngine.Random.Range(1000, 9999);
        _chatClient = new ChatClient(this);
        _chatClient.Connect(
            PhotonServerSettings.Global.AppSettings.AppIdChat,
            PhotonServerSettings.Global.AppSettings.AppVersion,
            new AuthenticationValues(_userName)
        );
    }

    private void Update()
    {
        _chatClient?.Service();
    }

    public void SendMessageToChannel(string message)
    {
        _chatClient.PublishMessage(_channelName, message);
    }

    public void OnConnected()
    {
        _chatClient.Subscribe(
            _channelName,
            0,
            0,
            new ChannelCreationOptions { PublishSubscribers = true }
        );
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        OnChatUserSubscribed?.Invoke(_userName);
    }

    public void OnUserSubscribed(string channel, string user)
    {
        OnChatUserSubscribed?.Invoke(user);
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            OnMessageReceived?.Invoke(messages[i].ToString(), senders[i]);
        }
    }

    // Unused callbacks (required by interface)
    public void DebugReturn(Photon.Client.LogLevel level, string message) { }
    public void OnDisconnected() { }
    public void OnChatStateChange(ChatState state) { }
    public void OnPrivateMessage(string sender, object message, string channelName) { }
    public void OnUnsubscribed(string[] channels) { }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message) { }
    public void OnUserUnsubscribed(string channel, string user) { }
    public void OnCustomAuthenticationResponse(Dictionary<string, object> data) { }
    public void OnCustomAuthenticationFailed(string message) { }
}
