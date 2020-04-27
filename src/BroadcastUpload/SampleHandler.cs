using System;
using System.Diagnostics;
using AppRTC;
using AppRTC.Extensions;
using CoreMedia;
using Foundation;
using ReplayKit;
//using NativeLibrary;
using WebRTC.iOS.Binding;


namespace BroadcastUpload
{
    [Register("SampleHandler")]
    public class SampleHandler : RPBroadcastSampleHandler, IARDAppClientDelegate
    {
        private ARDAppClient _client;
        private ARDExternalSampleCapturer _capturer { get; set; }
        readonly object _capturerLock = new object();

        public override void BroadcastStarted(NSDictionary<NSString, NSObject> setupInfo)
        {
            base.BroadcastStarted(setupInfo);

            var settingsModel = new ARDSettingsModel();
            _client = ARDAppClient.Create(@delegate: this);
            _client.IsBroadcast = true;
            //string roomName;
            //if (setupInfo.ContainsKey("roomName".ToNative()))
            //{
            //    roomName = setupInfo["roomName"].ToString();
            //}
            //else
            //{
            //    var random = new Random(Environment.TickCount);
            //    roomName = "broadcast_" + random.Next(1000);
            //}

            _client.ConnectToRoomWithId("863200204", settingsModel, false);
            Console.WriteLine("Broadcast started.");
        }

        public override void BroadcastFinished()
        {
            base.BroadcastFinished();
            _client.Disconnect();
        }

        public override void ProcessSampleBuffer(CMSampleBuffer sampleBuffer, RPSampleBufferType sampleBufferType)
        {
            switch (sampleBufferType)
            {
                case RPSampleBufferType.Video:
                    // Handle video sample buffer
                    //++sampleNo;
                    Debug.WriteLine($"Process Sample received.");
                    try
                    {
                        lock (_capturerLock)
                        {
                            _capturer?.DidCaptureSampleBuffer(sampleBuffer);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    break;
                case RPSampleBufferType.AudioApp:
                    break;
                case RPSampleBufferType.AudioMic:
                    break;
            }
        }

        public void DidChangeConnectionState(RTCIceConnectionState state)
        {
            Console.WriteLine("ICE state changed: {0} ({1})", state, (int)state);
        }

        public void DidChangeState(ARDAppClientState state)
        {
            Console.WriteLine("Client {0}.", state);
        }

        public void DidCreateLocalCapturer(RTCCameraVideoCapturer localCapturer)
        {
        }

        public void DidCreateLocalExternalSampleCapturer(ARDExternalSampleCapturer externalSampleCapturer)
        {
            
            lock (_capturerLock)
            {
                _capturer = externalSampleCapturer;
            }
        }

        public void DidCreateLocalFileCapturer(RTCFileVideoCapturer fileCapturer)
        {
        }

        public void DidError(ARDAppException error)
        {
            Console.WriteLine("Error:{0}", error);
        }

        public void DidGetStats(RTCLegacyStatsReport[] stats)
        {
        }

        public void DidReceiveLocalVideoTrack(RTCVideoTrack localVideoTrack)
        {
        }

        public void DidReceiveRemoteVideoTrack(RTCVideoTrack remoteVideoTrack)
        {
        }

        public void DidCreatePeerConnection(RTCPeerConnection peerConnection)
        {
        }

        public void DidOpenDataChannel(RTCDataChannel dataChannel)
        {
        }
    }
}







