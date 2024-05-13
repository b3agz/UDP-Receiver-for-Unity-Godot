using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Text.Json;
using UnityEngine;
#nullable enable

public class StreamerBotUDPReceiver : MonoBehaviour {

    [Header("Connection")]
    [Tooltip("The port that StreamerBot is sending the event over. This is set in the Action dialogue box for each action.")]
    [SerializeField] private int _port = 5069;

    #region Threading Stuff
    private Thread? _receiveThread;
    private UdpClient? _client;
    #endregion

    #region Delegate Stuff
    public delegate void StreamerBotEvent(StreamerBotEventData eventData);
    private Dictionary<string, StreamerBotEvent> _eventHandlers;

    protected virtual void InitialiseStreamerBotEvents() {

        // Register StreamerBotEvents here. Recommend overriding this function from an
        // inherited script for the sake of neatness and sanity.

    }

    /// <summary>
    /// Registers a new StreamerBotEvent.
    /// </summary>
    /// <param name="eventType">The name of the event. Must exactly match the Event value passed in from StreamerBot.</param>
    /// <param name="action">The function to be called when this event is received.</param>
    protected void RegisterEvent(string eventType, StreamerBotEvent action) {

        // If we haven't already registered this event type, set it to this action.
        if (!_eventHandlers.ContainsKey(eventType)) {
            _eventHandlers[eventType] = action;
        // If we have registered it, add the action to the event type.
        } else {
            _eventHandlers[eventType] += action;
        }

    }

    /// <summary>
    /// Checks to see if we have a registered action for the given StreamerBotEventData and runs that action
    /// if we do.
    /// </summary>
    /// <param name="eventData">The StreamerBotEventData received from StreamerBot.</param>
    private void ProcessEvents(StreamerBotEventData eventData) {

        if (eventData == null || eventData.Event == null) return;

        // If we have a registed action for this event, run that function. Else log a warning.
        if (_eventHandlers.TryGetValue(eventData.Event, out StreamerBotEvent? handler)) {
            handler?.Invoke(eventData);
        } else {
            Debug.LogWarning($"StreamerBot sent event type \"{eventData.Event}\" but no matching action is registered for this event");
        }
    }
    #endregion

    /// <summary>
    /// Initialises the UDP receiver thread and delegate lists.
    /// </summary>
    private void Init() {

        Debug.Log($"Attempting to initialise StreamerBot UDP Receiver: 127.0.0.1:{_port}");

        // Belts and braces error check to make sure we haven't already tried to start the thread.
        if (_receiveThread == null) {
            // Setup the thread and start it running.
            _receiveThread = new Thread(new ThreadStart(ReceiveData));
            _receiveThread.IsBackground = true;
            _receiveThread.Start();
        } else {
            Debug.LogWarning("Attempted to start StreamerBot UDP Receiver thread but thread was already running.");
        }

        _eventHandlers = new Dictionary<string, StreamerBotEvent>();
        InitialiseStreamerBotEvents();

    }

    /// <summary>
    /// Checks to see if we have a thread or client running and aborts/closes them.
    /// </summary>
    private void CloseConnection() {
        if (_receiveThread != null) {
            _receiveThread.Abort();
            _receiveThread = null;
        }
        _client?.Close();
    }

    /// <summary>
    /// Closes the current connection (if there is one) and initialises a new one.
    /// </summary>
    public void Reset() {
        CloseConnection();
        Init();
    }

    /// <summary>
    /// Runs continuously checking for information from UDP port. DO NOT CALL FROM MAIN THREAD!
    /// </summary>
    private void ReceiveData() {

        Debug.Log($"StreamerBot UDP Receiver thread started for 127.0.0.1:{_port}");

        _client = new UdpClient(_port);

        // Begin UDP Receiver loop.
        while (true) {

            // Try to receive JSON data and packaged into a StreamerBotEventData class. If successful,
            // send the resulting data to TryEvent to be used.
            try {

                // Get the JSON information from the UDP message.
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _client.Receive(ref anyIP);
                string receivedData = Encoding.UTF8.GetString(data);

                // Serialize the JSON data into a StreamerBotEventData class.
                JsonSerializerOptions options = new JsonSerializerOptions {
                    PropertyNameCaseInsensitive = true, // Make the parser case-insensitive
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                StreamerBotEventData newEvent = JsonSerializer.Deserialize<StreamerBotEventData>(receivedData, options);

                // Send StreamerBotEventData to be processed.
                ProcessEvents(newEvent);

            } catch (Exception err) {
                Debug.Log(err.ToString());
            }
        }
    }

    #region Automatic Initialisation/Connection Closing

    private void OnEnable() {
        Init();
    }

    private void OnDisable() {
        CloseConnection();
    }

    private void OnApplicationQuit() {
        CloseConnection();
    }

    #endregion

    public static StreamerBotEventData ParseJson(string jsonString) {

        JsonSerializerOptions options = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true, // Make the parser case-insensitive
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        StreamerBotEventData eventData = JsonSerializer.Deserialize<StreamerBotEventData>(jsonString, options);
        return eventData;
    }

}

/// <summary>
/// Contains the data passed in from StreamerBot. The data can include any or all of the fields
/// in this class. For example, sending a Bit Cheer event would include the Event, User, and
/// Amount (and possibly Message), whereas sending an ad-break event would only need an
/// Event.
/// </summary>
public class StreamerBotEventData {

    /// <summary>
    /// The type of event. Can be anything you wish but the string passed from StreamerBot
    /// must match exactly with whatever you are doing in Unity.
    /// </summary>
    public string? Event { get; set; }

    /// <summary>
    /// The username associated with the event. For example, if the event was a subscription,
    /// this would be the username of the subscriber.
    /// </summary>
    public string? User { get; set; }

    /// <summary>
    /// A message associated with the event. For example, if you wanted to play TTS from this event,
    /// this string would contain the message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// A numerical amount associated with this event. For example, the number of bits cheered or subs
    /// gifted.
    /// </summary>
    public int? Amount { get; set; }
    
}