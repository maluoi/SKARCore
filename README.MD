# StereoKit ARCore

This is a very minimal example of using ARCore with StereoKit [based on Nakamir's work](https://github.com/Nakamir-Code/SKARCore/tree/develop/Examples/StereoKitTest/StereoKitTest_ARCore) using vanilla StereoKit and recently added features instead of a fork. This is just for pose and AR background, and is not currently robust to rotations or other lifecycle events.

This is implemented as an `IStepper`, [ARCore](/ARCore.cs) which depends on a [custom skybox shader](/Assets/skyUnlitExternal.hlsl). Right now this works best with an `SKSettings.appMode` of `AppMode.Window`, and relies on StereoKit v0.4.0-preview.713 or later.
