using Godot;
using StreamerBotUDP;
using StreamerBotUDP.Godot;

public partial class GodotStreamerBotEventManagerExample : GodotStreamerBotEventManager
{
	protected override void RegisterEvents()
	{
		UdpReceiver.RegisterEvent("Test", StreamerBotTest);
	}

	private void StreamerBotTest(StreamerBotEventData eventData) {
		GD.Print($"Event: {eventData.Event}");
		GD.Print($"User: {eventData.User}");
		GD.Print($"Message: {eventData.Message}");
		GD.Print($"Amount: {eventData.Amount}");
	}
}