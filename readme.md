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
NetworkedUdonSharpBehaviour is an abstract extension of the base UdonSharpBehaviour class. Its intended role is to automate common Manual-synced networking code and guard against VRChat networking issues, such as replaying old synced data.
An example of its use is as follows:
```
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

Under the hood, NetworkedUdonSharpBehaviour handles a few things:

1. `ClaimOwnership()` replaces `Networking.SetOwner(Networking.LocalPlayer, gameObject)`
2. `RequestSerialization()` is enhanced - now it automatically triggers `OnDeserialization` a frame after requesting serialization.
3. All `NetworkedUdonSharpBehaviours` sync a version number whenever they serialize new data, and this number is validated to ensure that data cannot roll back: a frequent issue with VRChat's Udon network code.
4. `OnNetworkUpdate` replaces `OnDeserialization` in `NetworkedUdonSharpBehaviour` implementations - it must be implemented and it has strictly defined behaviour. It does not get triggered if the behaviour has incorrectly received old data, and is always called a frame after RequestSerialization is called.

