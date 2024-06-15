using Godot;

namespace StreamerBotUDP.Godot;

/// <summary>
/// Node for allowing a StreamerBotUDPReceiver instance to work within a Godot project.
/// Meant to be inherited by your custom event manager that defines methods and registers them to
/// run when events are received.
/// </summary>
public abstract partial class GodotStreamerBotEventManager : Node
{
    [Export] private int _port = 5069;
    protected readonly StreamerBotUDPReceiver UdpReceiver = new();

    public override void _Ready()
    {
        UdpReceiver.Port = _port;
        UdpReceiver.ConsolePrintDelegate = GD.Print;
        UdpReceiver.Init();

        RegisterEvents();
    }

    protected virtual void RegisterEvents()
    {
    }

    public override void _Process(double delta)
    {
        UdpReceiver.ProcessEventQueue();
    }

    public override void _ExitTree()
    {
        UdpReceiver.CloseConnection();
    }
}