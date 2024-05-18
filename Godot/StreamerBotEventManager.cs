using System.Collections;
using System.Collections.Generic;
using Godot;
using StreamerBotUDP;

public partial class StreamerBotEventManager : StreamerBotUDPReceiver {

    /// <summary>
    /// Registers event names and corresponding actions to listen for from StreamerBot.
    /// The name of the event must exactly match the "Event" variable in the UDP Payload.
    /// </summary>
    protected override void InitialiseStreamerBotEvents() {
        RegisterEvent("Test", StreamerBotTest);
    }

    #region Functions called by the registered events.

    private void StreamerBotTest(StreamerBotEventData eventData) {
        GD.Print($"Event: {eventData.Event}");
        GD.Print($"User: {eventData.User}");
        GD.Print($"Message: {eventData.Message}");
        GD.Print($"Amount: {eventData.Amount}");
    }

    #endregion
}
