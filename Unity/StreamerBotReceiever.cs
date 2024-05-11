using System;
using System.Reactive.Linq;
using Websocket.Client;
using UnityEngine;

public class StreamerBotReceiver : MonoBehaviour {
    private WebsocketClient _webSocketClient;

    // Replace this with your actual WebSocket server URL for StreamerBot
    private const string WebSocketUrl = "ws://localhost:5069";

    void Start() {
        ConnectToWebSocket();
    }

    void OnDestroy() {
        DisconnectFromWebSocket();
    }

    private void ConnectToWebSocket() {
        var url = new Uri(WebSocketUrl);

        _webSocketClient = new WebsocketClient(url) {
            IsReconnectionEnabled = true,
            ReconnectTimeout = TimeSpan.FromSeconds(30)
        };

        // Ensure to import System.Reactive.Linq for .Where
        _webSocketClient.MessageReceived
            .Where(msg => !string.IsNullOrEmpty(msg.Text))
            .Subscribe(OnWebSocketMessage);

        _webSocketClient.ReconnectionHappened.Subscribe(info =>
            Debug.Log($"Reconnection happened, type: {info.Type}"));

        _webSocketClient.DisconnectionHappened.Subscribe(info =>
            Debug.Log($"Disconnection happened, type: {info.Type}"));

        _webSocketClient.Start();
    }

    private void DisconnectFromWebSocket() {
        if (_webSocketClient != null) {
            _webSocketClient.Dispose();
            _webSocketClient = null;
        }
    }

    private void OnWebSocketMessage(ResponseMessage message) {
        Debug.Log($"Received message: {message.Text}");

        // Parse the incoming message to trigger Unity events
        try {
            var eventData = JsonUtility.FromJson<EventData>(message.Text);
            HandleEventData(eventData);
        } catch (Exception ex) {
            Debug.LogError($"Failed to parse JSON data: {ex.Message}");
        }
    }

    private void HandleEventData(EventData eventData) {
        // Trigger Unity events based on the received event data
        switch (eventData.eventType) {
            case "Jump":
                Debug.Log("Triggering Jump event!");
                break;
            case "Wave":
                Debug.Log("Triggering Wave event!");
                break;
            default:
                Debug.LogWarning($"Unknown event type: {eventData.eventType}");
                break;
        }
    }

    // Structure representing incoming JSON data
    [Serializable]
    public class EventData {
        public string eventType;
    }
}