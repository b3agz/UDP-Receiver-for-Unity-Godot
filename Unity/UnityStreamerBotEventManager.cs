using System.Diagnostics;
using StreamerBotUDP;
using UnityEngine;

public abstract class UnityStreamerBotEventManager : MonoBehaviour
{
    [Header("Connection")]
    [Tooltip("The port that StreamerBot is sending the event over. This is set in the Action dialogue box for each action.")]
    [SerializeField] private int _port = 5069;
    
    protected readonly StreamerBotUDPReceiver UdpReceiver = new StreamerBotUDPReceiver();
    
    public void OnEnable()
    {
        UdpReceiver.Port = _port;
        UdpReceiver.ConsolePrintDelegate = Debug.Log;
        UdpReceiver.Init();
     
        RegisterEvents();
    }
    
    protected virtual void RegisterEvents()
    {
    }

    public void Update()
    {
        UdpReceiver.ProcessEventQueue();
    }

    private void OnDisable()
    {
        UdpReceiver.CloseConnection();
    }

    private void OnApplicationQuit() {
        UdpReceiver.CloseConnection();
    }
}