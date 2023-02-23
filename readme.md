# FairlySadProductions CoreScripts
*Scripts to speed up your world development. More Udon, More Better!*

# Note: we don't recommend using these scripts at the moment. This library is being edited and extended outwards as a side project.

CoreScripts is a UdonSharp 1.0 library for use with VRChat worlds of all kinds - in particular, game worlds and other worlds that require complex networked interactions. This library is a dependency for other FairlySadProduction libraries and packages, and can be imported by other creators to extend.

### ⚠️Warning!⚠️
Currently this library requires a minor edit to UdonSharp 1.0 to work: 
1. In your UdonSharp package folder, open `Runtime/UdonSharpBehaviour.cs`
2. On line 148, find `public void RequestSerialization() {  }` and replace it with `public virtual void RequestSerialization() {  }`

See U# 1.0 PR: https://github.com/vrchat-community/UdonSharp/pull/37

## Components
### NetworkedUdonSharpBehaviour
*Easier manual sync, to stop you writing boilerplate*

`NetworkedUdonSharpBehaviour` is an abstract extension of the base `UdonSharpBehaviour` class. Its intended role is to automate common Manual-synced networking code and guard against VRChat networking issues, such as replaying old synced data.

An example of its use is as follows:
```cs
public class Foo : NetworkedUdonSharpBehaviour {
	[UdonSynced] private int counter;

	public void Interact() {
		ClaimOwnership();
		counter++;
		RequestSerialization();
	}

	protected override void OnNetworkUpdate() {
		Debug.Log($"Got new counter value: {counter}");
	}
}
```

Under the hood, `NetworkedUdonSharpBehaviour` handles a few things:

1. `ClaimOwnership()` replaces `Networking.SetOwner(Networking.LocalPlayer, gameObject)`
2. `RequestSerialization()` is enhanced - now it automatically triggers `OnDeserialization` a frame after requesting serialization.
3. All `NetworkedUdonSharpBehaviours` sync a version number whenever they serialize new data, and this number is validated to ensure that data cannot roll back: a frequent issue with VRChat's Udon network code.
4. `OnNetworkUpdate` replaces `OnDeserialization` in `NetworkedUdonSharpBehaviour` implementations - it must be implemented and it has strictly defined behaviour. It does not get triggered if the behaviour has incorrectly received old data, and is always called a frame after `RequestSerialization` is called.

### SimpleObjectPool
*Finally, a simple object pool that uses VRChat's own features!*

`SimpleObjectPool` is, as the name suggests, a simple pool of networked `SimplePooledObject` objects.  It differs from other 3rd-party object pools for VRChat in two ways

1. It has some measure of trust in the VRChat `VRCObjectPool` component, and doesn't attempt to totally replace that feature.
2. It leverages U# 1.0's ability to abstract base classes, so the `SimpleObjectPool` can pool any object that implements `SimplePooledObject`.

The pool allocates one networked object to every player who joins the world, and returns the object to the pool when that player leaves. It keeps track of what spawned `SimplePooledObject` is owned by the local player, allowing any UdonBehaviour to use that object to communicate owned data and instructions out to all other players.

For an example of the pool in action, a prefab is supplied with the library: you can find it in the `Prefabs` folder.
 
 To use the pool, `SimplePooledObject` must be extended by an implementing class. For example:
```cs
public class SPOExample : SimplePooledObject  
{
	[UdonSynced] private int counter;
  
  public void IncrementCounter()  
	{  if (ownerID != Networking.LocalPlayer.playerId || !Networking.IsOwner(gameObject))  
		 {
			 Debug.Log("IncrementCounter failed");  
			 return;  
		 }
	
  Debug.Log("IncrementCounter activated");  
  counter++;  
  RequestSerialization();  
 }  
 
 private void OnDisable()  
 {
	 Debug.Log($"{name} removed from {ownerID}; returned to pool owner");  
 }

 protected override void ClearSyncedData()  
 {
	 counter = 0;  
	 Debug.Log($"{name} cleared of data");  
 }

 protected override void HandleNewSyncedData()  
 {
	 Debug.Log($"{name} got new synced data: counter is {counter}");
 }
}
```

All of the pool logic is handled in the `SimplePooledObject` and `SimpleObjectPool` behaviours. By extending `SimplePooledObject`, you only have to worry about implementing two methods:
1. `ClearSyncedData`, which should reset all `[UdonSynced]` properties to their default values.
2. `HandleNewSyncedData` which does whatever you like when you retrieve new synced data.

You can customize the data that is synchronized in your implementation as you see fit: the pool handles ownership management and allocation in the background.

To interact with pooled objects, you can retrieve the pool's object array directly, via `SimpleObjectPool::GetPooledObjects()`, or retrieve the pooled object allocated to your own client via `SimpleObjectPool::GetPooledObject()`. In the latter case, you can then cast the received `SimplePooledObject` to your implementation (e.g. `var obj = (SimplePooledObjectExample)pool.GetPooledObject();`).

There's no magic in the background, just leverage of VRChat's `VRCObjectPool` script and `NetworkedUdonSharpBehaviour` to provide further guarantees of correctness.

With this pool, common networking problems, like "how do I tell the owner of an UdonBehaviour that I would like to do something" and "how do I sync data to all players without causing strange behaviour if lots of people are changing ownership" are solved!


### LobbyManager
*Game lobbies without weird bugs! A miracle of science!*

LobbyManager stops you having to write endless boilerplate lobby management code when you just want a collection of signed-up players who want to do something together.

The abstract `LobbyManager` behaviour, and the implementing `SimpleLobbyManager` behaviour, provide a set of simple tools that cover most basic lobby needs.

1. `AddPlayer(int playerID)` adds a player to the lobby.
2. `RemovePlayer(int playerID)` removes a player from the lobby.
3. `TryToStart()` attempts to start the game.
4. `_Reset()` resets the lobby to default, clearing out all signed-up players.
5. `UpdatePlayersView()` is called whenever the list of signed-up players changes, allowing the client to handle displaying the list of players (a "view" of the players signed up,  to use jargon) to the user.

If a player leaves the instance whilst signed up, they will automatically be removed from the lobby.

Note that editing the lobby and starting the game requires the client to own the object. It's intended you use `LobbyManager` with a networked object pool like `SimpleObjectPool` to handle players wanting to sign up, start the game, and so on.#

### Timer
*Wait, what*

Timers in Udon are generally annoying to write, as `SendCustomEventDelayedSeconds()` cannot be cancelled after firing, and operate invisibly in the background.
Thanks to recent defect fixes in Udon, `Instantiate()` works much more reliably to instantiate clones of objects with working `UdonBehaviour` components on them.
Leveraging this, we can spawn GameObjects that act like timers.

To use this behaviour:
1. Have an empty GameObject in your scene heirarchy, and add the Timer script to it. This will act as your template: `Instantiate()` needs to clone an object in the scene view for Udon on that object to be recreated.
2. Spawn new instances of the Timer GameObject when you want to start a timer, and then call `Timer::StartTimer(float, UdonBehaviour, string)` to start the timer. 
3. If you want to trigger the timer early, call `Timer::EndTimer()`
4. If you want to cancel the timer, `Destroy()` the GameObject.

### ActivateOnInteract
*Because nobody should have to write a button script for the 100th time*

With help from Vowgan, `ActivateOnInteract` is the brainlessly simple interact button script you've known you've needed! Thanks to the magic of editor scripting, the behaviour automatically reads the available Udon events exposed inside the behaviour added to the component's UdonBehaviour slot, making it easy to use with all major Udon varieties. In addition, activating the button can be networked between players (using SendCustomNetworkEvent) and optionally only networked to the owner of the object: perfect for simple scripting needs.

To use `ActivateOnInteract`:

1. Add the `ActivateOnInteract` component to an object with a collider on it.
2. Drag-and-drop the behaviour you want to activate an event on into the 'Behaviour' slot.
3. Select the event you want to fire from the drop down (the event might have underscores in front of it - that's normal)
4. Choose your networking options.
5. You're done!

