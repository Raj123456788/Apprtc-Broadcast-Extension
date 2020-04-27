# AppRTC - iOS implementation of the Google WebRTC Demo

## About
 The precompiled libWebRTC static library bundled with the pod works with 64-bit apps, unlike prior versions of WebRTC projects where only the 32-bit version was available. Currently, the project is designed to run on iOS Devices (iOS Simulator is not supported).

##Running the Project

1) In BroadcastUpload Project: sampleHandler.cs file. Please Change the first parameter to a random room id from https://appr.tc/
2) To enable broadcasting do the following:
3)Make sure the room id is working by joining into apprtc session 
4)Install the app on the mobile device Settings > Control Center, and tap Customize Controls Under Customize Controls, include Screen Recording Open the Control Center by swiping up from the bottom edge of any screen, and find 3D touch/long press the Screen Recording icon. Select Apprtc.iOS from the list

Currently It does not broadcast to apprtc and I am unable to understand why. I am getting Attempting to start a Invalid broadcast session
