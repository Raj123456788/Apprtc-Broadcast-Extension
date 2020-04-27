﻿//
// ARDMainViewController.cs
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
using Foundation;
using UIKit;
using System.Linq;
using CoreGraphics;
using AVFoundation;
using WebRTC.iOS.Binding;
using ReplayKit;

namespace AppRTC.iOS
{
    public class ARDMainViewController : UIViewController, IARDMainViewDelegate, IRTCAudioSessionDelegate, IARDVideoCallViewControllerDelegate, IRPBroadcastActivityViewControllerDelegate, IRPBroadcastControllerDelegate
    {
        const string barButtonImageString = @"ic_settings_black_24dp.png";
        const string loopbackLaunchProcessArgument = @"loopback";

        private ARDMainView _mainView;
        private AVAudioPlayer _audioPlayer;
        RPBroadcastController _broadcastController;
        UIBarButtonItem _shareButton;

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public override async void ViewDidLoad()
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            base.ViewDidLoad();

            var processInfo = new NSProcessInfo();
            if (processInfo.Arguments.Contains(loopbackLaunchProcessArgument))
            {
                OnStartCall(null, "", true);
            }

            await PermisionManager.GetRequiredPermissionAsync();
            SetupBroadcastUI();
        }

        public override void LoadView()
        {
            base.LoadView();
            Title = "AppRTC Mobile";
            NavigationItem.Title = "WebRTC Test";
            _shareButton = new UIBarButtonItem("Share", UIBarButtonItemStyle.Plain, OnShareButtonPressed);
            NavigationItem.RightBarButtonItem = _shareButton;
            _mainView = new ARDMainView(CGRect.Empty);
            _mainView.Delegate = this;
            _mainView.BackgroundColor = UIColor.White;

            View = _mainView;

            AddSettingsBarButton();

            var webRTCConfig = new RTCAudioSessionConfiguration();

            webRTCConfig.CategoryOptions |= AVAudioSessionCategoryOptions.DefaultToSpeaker;
            RTCAudioSessionConfiguration.SetWebRTCConfiguration(webRTCConfig);

            var session = RTCAudioSession.SharedInstance;
            session.AddDelegate(this);

            ConfigureAudioSession();
            SetupAudioPlayer();
            SetupBroadcastUI();
        }

        [Export("audioSessionDidStartPlayOrRecord:")]
        public void AudioSessionDidStartPlayOrRecord(RTCAudioSession session)
        {
            // Stop playback on main queue and then configure WebRTC.
            RTCDispatcher.DispatchAsyncOnType(RTCDispatcherQueueType.Main, () =>
            {
                if (_mainView.IsAudioLoopPlaying)
                {
                    Console.WriteLine("Stopping audio loop due to WebRTC start.");
                    _audioPlayer.Stop();
                }

                Console.WriteLine("Setting isAudioEnabled to YES.");
                session.IsAudioEnabled = true;
            });
        }

        [Export("audioSessionDidStopPlayOrRecord:")]
        public void AudioSessionDidStopPlayOrRecord(RTCAudioSession session)
        {
            // WebRTC is done with the audio session. Restart playback.
            RTCDispatcher.DispatchAsyncOnType(RTCDispatcherQueueType.Main, () =>
            {
                Console.WriteLine("audioSessionDidStopPlayOrRecord");
                RestartAudioPlayerIfNeeded();
            });
        }

        private void AddSettingsBarButton()
        {
            var settingButton = new UIBarButtonItem(new UIImage(barButtonImageString), UIBarButtonItemStyle.Plain, OnShareButtonPressed);
            NavigationItem.RightBarButtonItem = settingButton;
        }

        public void DidFinish(ARDVideoCallViewController viewController)
        {
            if (!viewController.IsBeingDismissed)
            {
                Console.WriteLine("Dismissing VC");
                viewController.DismissViewController(true, RestartAudioPlayerIfNeeded);
            }
            var session = RTCAudioSession.SharedInstance;
            session.IsAudioEnabled = false;
        }

        public void OnStartCall(ARDMainView mainView, string room, bool isLoopback)
        {
#if __H113__
            room = "12345";
#else
            if (string.IsNullOrWhiteSpace(room))
            {
                if (isLoopback)
                {
                    room = LoopbackRoomString();
                }
                else
                {
                    ShowAlertWithMessage("Missing room name.");
                    return;
                }
            }
#endif
            room = room.Trim();


            var settingsModel = new ARDSettingsModel();
            var session = RTCAudioSession.SharedInstance;

            session.UseManualAudio = settingsModel.CurrentUseManualAudioConfigSettingFromStore;
            session.IsAudioEnabled = false;


            var videoCallViewController = new ARDVideoCallViewController(room, isLoopback, this);
            videoCallViewController.ModalTransitionStyle = UIModalTransitionStyle.CrossDissolve;

            PresentViewController(videoCallViewController, true, null);
        }

        public void DidToggleAudioLoop(ARDMainView mainView)
        {
            if (mainView.IsAudioLoopPlaying)
            {
                _audioPlayer.Stop();
            }
            else
            {
                _audioPlayer.Play();
            }

            mainView.IsAudioLoopPlaying = _audioPlayer.Playing;
        }

        private string LoopbackRoomString()
        {
            return Guid.NewGuid().ToString().Replace("-", "");
        }

        private void ConfigureAudioSession()
        {
            var configuration = new RTCAudioSessionConfiguration();
            configuration.Category = AVAudioSession.CategoryAmbient;
            configuration.CategoryOptions = AVAudioSessionCategoryOptions.DuckOthers;
            configuration.Mode = AVAudioSession.ModeDefault;

            var session = RTCAudioSession.SharedInstance;
            session.LockForConfiguration();

            bool hasSucceeded;
            NSError error;

            if (session.IsActive)
            {
                hasSucceeded = session.SetConfiguration(configuration, out error);
            }
            else
            {
                hasSucceeded = session.SetConfiguration(configuration, true, out error);
            }

            if (!hasSucceeded)
            {
                Console.WriteLine("Error setting configuration:{0}", error.LocalizedDescription);
            }

            session.UnlockForConfiguration();
        }

        private void SetupAudioPlayer()
        {
            var audioFilePath = NSBundle.MainBundle.PathForResource("mozart", "mp3");
            var audioFileUrl = new NSUrl(audioFilePath);
            _audioPlayer = new AVAudioPlayer(audioFileUrl, "mozart", out _);
            _audioPlayer.NumberOfLoops = -1;
            _audioPlayer.Volume = 1;
            _audioPlayer.PrepareToPlay();
        }

        private void RestartAudioPlayerIfNeeded()
        {
            ConfigureAudioSession();
            if (_mainView.IsAudioLoopPlaying && PresentedViewController != null)
            {
                Console.WriteLine("Starting audio loop due to WebRTC end.");
                _audioPlayer.Play();
            }
        }

        private void ShowSettings(object sender, EventArgs e)
        {
            var settingsController = new ARDSettingsViewController(UITableViewStyle.Grouped, new ARDSettingsModel());

            var navigationController = new UINavigationController(settingsController);

            PresentViewController(navigationController, true, null);
        }

        private void ShowAlertWithMessage(string message)
        {
            var alert = UIAlertController.Create("", message, UIAlertControllerStyle.Alert);
            var defaultAction = UIAlertAction.Create("OK", UIAlertActionStyle.Default, (_) => { });

            alert.AddAction(defaultAction);

            PresentViewController(alert, true, null);
        }
        #region IRPBroadcastActivityViewControllerDelegate methods
        public void DidFinish(RPBroadcastActivityViewController broadcastActivityViewController, RPBroadcastController broadcastController, NSError error)
        {
            Console.WriteLine($"BroadcastActivityVC: {broadcastActivityViewController}: BroadcastController: {broadcastController} Error: {error}");

            InvokeOnMainThread(() => broadcastActivityViewController.DismissViewController(true, null));

            _broadcastController = broadcastController;

            if (error == null)
            {
                broadcastController.StartBroadcast(err =>
                {
                    if (err == null)
                    {
                        broadcastController.Delegate = this;

                        //InvokeOnMainThread(() =>
                        //{
                        //    _shareButton.Title = "Stop";
                        //    _shareButton.TintColor = UIColor.Red;
                        //});
                    }
                    else
                    {
                        // Some error has occurred starting the broadcast, surface it to the user.
                        InvokeOnMainThread(() =>
                        {
                            UIAlertController alertController = UIAlertController.Create("Error", err.LocalizedDescription, UIAlertControllerStyle.Alert);
                            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Cancel, null));
                            PresentViewController(alertController, true, null);
                        });
                    }
                });
            }
            else
            {
                Console.WriteLine($"Error returning from BroadcastActivity: {error}");
            }
        }
        #endregion
        #region IRPBroadcastControllerDelegate methods
        public void DidFinish(RPBroadcastController broadcastController, NSError error)
        {
            Console.WriteLine($"DidFinish: Error: {error}");
        }

        public void DidUpdateServiceInfo(RPBroadcastController broadcastController, NSDictionary<NSString, INSCoding> serviceInfo)
        {
            Console.WriteLine($"DidUpdateServiceInfo: {serviceInfo}");
        }
        #endregion
        void OnShareButtonPressed(object sender, EventArgs e)
        {
            if (!RPScreenRecorder.SharedRecorder.Recording)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
                {
                    RPBroadcastActivityViewController.LoadBroadcastActivityViewController(
                        (broadcastActivity, error) =>
                        {
                            broadcastActivity.ModalPresentationStyle = UIModalPresentationStyle.Popover;
                            broadcastActivity.Delegate = this;

                            if (UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad)
                            {
                                UIView srcView = (sender as UIBarButtonItem)?.ValueForKey(new NSString("view")) as UIView;
                                CGRect srcRect = srcView?.Frame ?? CGRect.Empty;

                                broadcastActivity.PopoverPresentationController.SourceRect = srcRect;
                                broadcastActivity.PopoverPresentationController.SourceView = srcView;
                            }

                            PresentViewController(broadcastActivity, true, null);
                        });
                }
            }
            else
            {
                Console.WriteLine("Disconnecting...");
                _broadcastController.FinishBroadcast(error =>
                {
                    InvokeOnMainThread(() =>
                    {
                        //_shareButton.TintColor = UIColor.FromRGB(0, 122, 255);
                        //_shareButton.Title = "Share";
                    });
                });
            }
        }
        void ResumeBroadcast()
        {
            if (_broadcastController?.Paused == true)
            {
                _broadcastController.ResumeBroadcast();
            }
        }

        void SetupBroadcastUI()
        {
            if (RPScreenRecorder.SharedRecorder != null)
            {
                UIApplication.Notifications.ObserveDidBecomeActive((sender, e) => ResumeBroadcast());
                UIApplication.Notifications.ObserveWillEnterForeground((sender, e) => ResumeBroadcast());
            }
        }

    }
}
