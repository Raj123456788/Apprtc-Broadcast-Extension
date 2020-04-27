﻿//
// ARDVideoCallViewController.ARDAppClientDelegate.cs
//
// Author:
//       valentingrigorean <valentin.grigorean1@gmail.com>
//
// Copyright (c) 2019 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using AppRTC.iOS.Extenstions;
using CoreGraphics;
using UIKit;
using WebRTC.iOS.Binding;

namespace AppRTC.iOS
{

    public partial class ARDVideoCallViewController : IARDAppClientDelegate
    {
        private ARDCaptureController _captureController;
        private ARDFileCaptureController _fileCaptureController;
        private RTCVideoTrack _remoteVideoTrack;
        private RTCVideoTrack _localVideoTrack;


        public void DidCreatePeerConnection(RTCPeerConnection peerConnection)
        {

        }

        public void DidOpenDataChannel(RTCDataChannel dataChannel)
        {

        }

        public void DidChangeConnectionState(RTCIceConnectionState state)
        {
            Console.WriteLine("ICE state changed:{0}", state);
            _videoCallView.StatusLabel.Text = StatusTextForState(state);
        }

        public void DidChangeState(ARDAppClientState state)
        {
            switch (state)
            {
                case ARDAppClientState.Disconnected:
                    Console.WriteLine("Client disconnected.");
                    Hangup();
                    break;
                case ARDAppClientState.Connecting:
                    Console.WriteLine("Client connecting.");
                    break;
                case ARDAppClientState.Connected:
                    Console.WriteLine("Client connected.");
                    break;
            }
        }

        public void DidCreateLocalCapturer(RTCCameraVideoCapturer localCapturer)
        {
            _videoCallView.LocalVideoView.CaptureSession = localCapturer.CaptureSession;

            _captureController = new ARDCaptureController(localCapturer, new ARDSettingsModel());
            _captureController.StartCapture();

        }

        public void DidCreateLocalExternalSampleCapturer(ARDExternalSampleCapturer externalSampleCapturer)
        {
        }

        public void DidCreateLocalFileCapturer(RTCFileVideoCapturer fileCapturer)
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                _fileCaptureController = new ARDFileCaptureController(fileCapturer);
                _fileCaptureController.StartCapture();
            }
        }

        public void DidError(ARDAppException error)
        {
            Console.WriteLine(error);

            this.ShowAlertWithMessage(error.Message, Hangup);
        }

        public void DidGetStats(RTCLegacyStatsReport[] stats)
        {
            _videoCallView.StatsView.SetStats(stats);
            _videoCallView.SetNeedsLayout();
        }

        public void DidReceiveLocalVideoTrack(RTCVideoTrack localVideoTrack)
        {
            _localVideoTrack = localVideoTrack;
            if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.SIMULATOR)
            {

                var localView = new RTCEAGLVideoView();
                View.AddSubview(localView);

                var bounds = View.Bounds;
                var localVideoFrame = new CGRect(0, 0, 100f, 100f);
                // Place the view in the bottom right.
                localVideoFrame.Location = new CGPoint(
                    bounds.GetMaxX() - localVideoFrame.Size.Width - 8, bounds.GetMaxY() - localVideoFrame.Size.Height - 8 - AppDelegate.SafeAreaInsets.Top);

                localView.Frame = localVideoFrame;


                _localVideoTrack.AddRenderer(localView);
            }
        }

        public void DidReceiveRemoteVideoTrack(RTCVideoTrack remoteVideoTrack)
        {
            SetRemoteVideoTrack(remoteVideoTrack);
        }

        private string StatusTextForState(RTCIceConnectionState state)
        {
            switch (state)
            {
                case RTCIceConnectionState.New:
                case RTCIceConnectionState.Checking:
                    return "Connecting...";
                case RTCIceConnectionState.Connected:
                case RTCIceConnectionState.Completed:
                case RTCIceConnectionState.Failed:
                case RTCIceConnectionState.Disconnected:
                case RTCIceConnectionState.Closed:
                case RTCIceConnectionState.Count:
                    return "";
            }
            return "";
        }

        private void SetRemoteVideoTrack(RTCVideoTrack remoteVideoTrack)
        {
            if (_remoteVideoTrack == remoteVideoTrack)
            {
                return;
            }

            _remoteVideoTrack?.RemoveRenderer(_videoCallView.RemoteVideoRender);
            _remoteVideoTrack = null;
            _videoCallView.RemoteVideoRender.RenderFrame(null);
            _remoteVideoTrack = remoteVideoTrack;
            _remoteVideoTrack.AddRenderer(_videoCallView.RemoteVideoRender);
        }


    }
}
