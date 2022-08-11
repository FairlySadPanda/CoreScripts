# FairlySadProductions CoreScripts
*Scripts to speed up your world development. More Udon, More Better!*

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
