using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StreamerBotEventManager : StreamerBotUDPReceiver {

    /// <summary>
    /// Registers event names and corresponding actions to listen for from StreamerBot.
    /// The name of the event must exactly match the "Event" variable in the UDP Payload.
    /// </summary>
    protected override void InitialiseStreamerBotEvents() {
        this.RegisterEvent("Test", StreamerBotTest);
    }

    #region Functions called by the registered events.

    private void StreamerBotTest(StreamerBotEventData eventData) {
        Debug.Log($"Event: {eventData.Event}");
        Debug.Log($"User: {eventData.User}");
        Debug.Log($"Message: {eventData.Message}");
        Debug.Log($"Amount: {eventData.Amount}");
    }

    #endregion
}
