## What Is It?

These scripts allow for the triggering of events in [Unity](https://unity.com/) or [Godot .NET](https://godotengine.org/) from [Streamer.Bot](https://streamer.bot/). Information is passed over UDP using Streamer.bot's "UDP Broadcast" sub-action and listened for by these scripts in Unity or Godot.

### Requirements

The code was designed to work with Streamer.bot, however it is receiving Json-formatted information over UDP, so will theoretically work with any application that can send custom information over UDP.

No special tools or packages are required, it should work out of the box in both Unity and Godot 4. That being said, I have only tested it in Unity 2022.3.21f and Godot 4.2.2. Please be aware that this is C# code, and will not workin the non-.NET version of Godot.

### Installing

1. Grab the scripts out of the applicable folder and drop them in your project. Done. You will need `StreamerBotUDPReceiver.cs`. `StreamerBotEventManager.cs` is optional but I recommend using it if you don't want to go diving into the meat of the UDP Receiver code.

### Usage

The following assumes you are using the `StreamerBotEventManager` script or something like it.

Attach the `StreamerBotEventManager` script to an active GameObject (Unity) or visible Node (Godot) and set the `Port` value in the inspector to an unused port.

Open up the script and start writing functions for events you want to read from Streamer.bot. The functions can be called anything you like, but they must take in a `StreamerBotEventData` class as a parameter. This will contain the information received from Streamer.bot.

In the `InitialiseStreamerBotEvents()` function, register your events with the event name and the function it will call. There is "Test" event already in the code as an example.

Finally, head over to Streamer.bot and, in your Action of choice, create a "UDP Broadcast" Sub-Action. Make sure the port number matches the number you used in your engine. The payload needs to be formatted like this:

`{
  "Event": "Test",
  "User": "b3agz",
  "Message": "This is a test message.",
  "Amount": "42"
}`

### Other Information

The UDP listening code runs on a separate thread that is currently aborted using Abort(), which is a deprecated command. At the time of upload, the script works fine in the engines I have tested it in (see above), however this may need changing in the future.