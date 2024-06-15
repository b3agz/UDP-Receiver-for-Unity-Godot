using System.Diagnostics;

namespace StreamerBotUDP.Unity;

public class UnityStreamerBotEventManagerExample : UnityStreamerBotEventManager
{
    protected override void RegisterEvents()
    {
        UdpReceiver.RegisterEvent("Test", StreamerBotTest);
    }

    private void StreamerBotTest(StreamerBotEventData eventData) {
        Debug.Log($"Event: {eventData.Event}");
        Debug.Log($"User: {eventData.User}");
        Debug.Log($"Message: {eventData.Message}");
        Debug.Log($"Amount: {eventData.Amount}");
    }
}