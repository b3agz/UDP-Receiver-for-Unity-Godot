using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;
#nullable enable

namespace StreamerBotUDP {

    public class StreamerBotUDPReceiver : MonoBehaviour {

        [Header("Connection")]
        [Tooltip("The port that StreamerBot is sending the event over. This is set in the Action dialogue box for each action.")]
        [SerializeField] private int _port = 5069;

        #region Threading Stuff

        private Thread? _receiveThread;
        private UdpClient? _client;
        private CancellationTokenSource? _cancellationTokenSource;
        private static readonly ConcurrentQueue<StreamerBotEventData>? _events = new();

        #endregion

        #region Delegate Stuff
        public delegate void StreamerBotEvent(StreamerBotEventData eventData);
        private Dictionary<string, StreamerBotEvent> _eventHandlers = new();

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

            if (eventData == null || eventData.Event == null) {
                Debug.LogWarning("Attempted to process a null event.");
                return;
            }

            // If we have a registered action for this event, run that function. Else log a warning.
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

            // Belts and braces error check to make sure we haven't already started the thread.
            if (_receiveThread == null) {
                // Setup the thread and start it running.
                _cancellationTokenSource = new();
                CancellationToken token = _cancellationTokenSource.Token;
                _receiveThread = new Thread(() => ReceiveData(token));
                _receiveThread.IsBackground = true;
                _receiveThread.Start();
            } else {
                Debug.LogWarning("Attempted to start StreamerBot UDP Receiver thread but thread was already running.");
            }

            _eventHandlers = new Dictionary<string, StreamerBotEvent>();
            InitialiseStreamerBotEvents();

        }

        /// <summary>
        /// Called at the end of Init(), this function is intended to house the registration
        /// of StreamerBot events and their associated action.
        /// </summary>
        protected virtual void InitialiseStreamerBotEvents() {

            // Example:
            // RegisterEvent("Test", StreamerBotTest);

        }

        /// <summary>
        /// Checks to see if we have a thread or client running and aborts/closes them.
        /// </summary>
        private void CloseConnection() {

            // Make sure the receiver thread is not null.
            if (_receiveThread != null) {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }

            // Set receiver thread to null so it can be reinitialised if needed.
            _receiveThread = null;
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
        /// Runs continuously checking for information from UDP port. Designed to run on a separate thread.
        /// DO NOT CALL FROM MAIN THREAD!
        /// </summary>
        private void ReceiveData(CancellationToken token) {

            Debug.Log($"StreamerBot UDP Receiver thread started for 127.0.0.1:{_port}");

            using (_client = new UdpClient(_port)) {
                // Begin UDP Receiver loop.
                while (!token.IsCancellationRequested) {

                    // Try to receive JSON data and packaged into a StreamerBotEventData class. If successful,
                    // send the resulting data to TryEvent to be used.
                    try {

                        // Get the JSON information from the UDP message.
                        IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = _client.Receive(ref anyIP);
                        string receivedData = Encoding.UTF8.GetString(data);

                        // Serialize the JSON data into a StreamerBotEventData class.
                        StreamerBotEventData newEvent = JsonUtility.FromJson<StreamerBotEventData>(receivedData);

                        // Add the new event to our events queue to be processed on the main thread.
                        if (newEvent != null) {
                            _events?.Enqueue(newEvent);
                        }

                    } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted) {
                        Debug.LogWarning("StreamerBot UDP Receiver thread was interrupted.");
                    } catch (Exception err) {
                        Debug.LogWarning(err.ToString());
                    }
                }
            }
            Debug.Log("StreamerBot UDP Receiver thread has stopped.");
        }

        private void Update() {

            // Make sure the events queue is not null.
            if (_events == null) return;

            // Send any events that have been queued up to be processed.
            while (_events.TryDequeue(out StreamerBotEventData? newEvent)) {
                if (newEvent != null) {
                    ProcessEvents(newEvent);
                }
            }

        }

        // These functions run automatically when the parent GameObject is enabled/disabled or the
        // application quits. Calling Init() and CloseConnection() from here ensures that if your
        // StreamerBotManager object is disabled/activated, it has the same behaviour as resetting
        // the UDP connection/receiver thread.
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

    }

    /// <summary>
    /// Contains the data passed in from StreamerBot. The data can include any or all of the fields
    /// in this class. For example, sending a Bit Cheer event would include the Event, User, and
    /// Amount (and possibly Message), whereas sending an ad-break event would only need an
    /// Event.
    /// </summary>
    [System.Serializable]
    public class StreamerBotEventData {

        /// <summary>
        /// The type of event. Can be anything you wish but the string passed from StreamerBot
        /// must match exactly with whatever you are doing in Unity.
        /// </summary>
        public string Event;

        /// <summary>
        /// The username associated with the event. For example, if the event was a subscription,
        /// this would be the username of the subscriber.
        /// </summary>
        public string User;

        /// <summary>
        /// A message associated with the event. For example, if you wanted to play TTS from this event,
        /// this string would contain the message.
        /// </summary>
        public string Message;

        /// <summary>
        /// A numerical amount associated with this event. For example, the number of bits cheered or subs
        /// gifted.
        /// </summary>
        public int Amount;

        /// <summary>
        /// The constructor establishes the default values for each field, which is what the value will be if
        /// the UDP payload from StreamerBot does not contain a particular field.
        /// </summary>
        public StreamerBotEventData() {
            Event = string.Empty;
            User = string.Empty;
            Message = string.Empty;
            Amount = 0;
        }
    }
}