using System;
using Foundation;
using ReplayKit;
using UIKit;

namespace BroadcastUploadUI
{
    [Register("BroadcastViewController")]
    public class BroadcastViewController : UIViewController
    {
        private UITextField _roomNameField;

        UIButton _cancelButton;
        UIButton _connectButton;
        UILabel _title;
        UILabel[] _separators;
        UITextField[] _octets;

        public override void ViewDidLoad()
        {
            _title = new UILabel
            {
                Text = "IP address:",
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _octets = new UITextField[4];
            _separators = new UILabel[3];

            for (int i = 0; i < _octets.Length; ++i)
            {
                _octets[i] = new UITextField
                {
                    BorderStyle = UITextBorderStyle.RoundedRect,
                    KeyboardType = UIKeyboardType.NumberPad,
                    TextAlignment = UITextAlignment.Center,
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
            }

            for (int i = 0; i < _separators.Length; ++i)
            {
                _separators[i] = new UILabel
                {
                    Text = ".",
                    TranslatesAutoresizingMaskIntoConstraints = false
                };
            }

            View.BackgroundColor = UIColor.LightGray;

            nfloat textFieldWidth = NMath.Ceiling(GetStringWidth(new NSString("99999"), _octets[0].Font));

            View.AddSubview(_separators[1]);

            _separators[1].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _separators[1].CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;

            View.AddSubview(_octets[1]);

            _octets[1].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _octets[1].RightAnchor.ConstraintEqualTo(_separators[1].LeftAnchor, -4).Active = true;
            _octets[1].WidthAnchor.ConstraintEqualTo(textFieldWidth).Active = true;
            _octets[1].HeightAnchor.ConstraintEqualTo(_octets[1].WidthAnchor).Active = true;

            View.AddSubview(_octets[2]);

            _octets[2].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _octets[2].LeftAnchor.ConstraintEqualTo(_separators[1].RightAnchor, 4).Active = true;
            _octets[2].WidthAnchor.ConstraintEqualTo(textFieldWidth).Active = true;
            _octets[2].HeightAnchor.ConstraintEqualTo(_octets[2].WidthAnchor).Active = true;

            View.AddSubview(_separators[0]);

            _separators[0].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _separators[0].RightAnchor.ConstraintEqualTo(_octets[1].LeftAnchor, -4).Active = true;

            View.AddSubview(_separators[2]);

            _separators[2].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _separators[2].LeftAnchor.ConstraintEqualTo(_octets[2].RightAnchor, 4).Active = true;

            View.AddSubview(_octets[0]);

            _octets[0].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _octets[0].RightAnchor.ConstraintEqualTo(_separators[0].LeftAnchor, -4).Active = true;
            _octets[0].WidthAnchor.ConstraintEqualTo(textFieldWidth).Active = true;
            _octets[0].HeightAnchor.ConstraintEqualTo(_octets[1].WidthAnchor).Active = true;

            View.AddSubview(_octets[3]);

            _octets[3].BottomAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _octets[3].LeftAnchor.ConstraintEqualTo(_separators[2].RightAnchor, 4).Active = true;
            _octets[3].WidthAnchor.ConstraintEqualTo(textFieldWidth).Active = true;
            _octets[3].HeightAnchor.ConstraintEqualTo(_octets[2].WidthAnchor).Active = true;

            _cancelButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _cancelButton.SetTitleColor(UIColor.Red, UIControlState.Normal);
            _cancelButton.SetTitle("Cancel", UIControlState.Normal);
            _cancelButton.TouchUpInside += (sender, args) => UserDidCancelSetup();

            View.AddSubview(_cancelButton);

            _cancelButton.LeftAnchor.ConstraintEqualTo(_octets[0].LeftAnchor).Active = true;
            _cancelButton.TopAnchor.ConstraintEqualTo(_octets[0].BottomAnchor, 32).Active = true;

            _connectButton = new UIButton(UIButtonType.System)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            _connectButton.SetTitle("Connect", UIControlState.Normal);
            _connectButton.TouchUpInside += (sender, args) => UserDidFinishSetup();

            View.AddSubview(_connectButton);

            _connectButton.RightAnchor.ConstraintEqualTo(_octets[3].RightAnchor).Active = true;
            _connectButton.TopAnchor.ConstraintEqualTo(_octets[3].BottomAnchor, 32).Active = true;

            View.AddSubview(_title);

            _title.Font = UIFont.BoldSystemFontOfSize(_title.Font.PointSize);
            _title.CenterXAnchor.ConstraintEqualTo(View.CenterXAnchor).Active = true;
            _title.BottomAnchor.ConstraintEqualTo(_octets[1].TopAnchor, -32).Active = true;
        }

        //private void UserDidCancelSetup()
        //{
        //    throw new NotImplementedException();
        //}

        ////private void UserDidFinishSetup()
        ////{
        ////    throw new NotImplementedException();
        ////}

        private void UserDidFinishSetup()
        {
           // _roomNameField.Text = "859502387";
            // URL of the resource where broadcast can be viewed that will be returned to the application
            string broadcastUrlString = "https://192.168.1.29/api/wipc";
            var broadcastUrl = NSUrl.FromString(broadcastUrlString);

            // Service specific broadcast data example which will be supplied to the process extension during broadcast
            var keys = new NSString[] { new NSString("IPAddress") };
            var objects = new INSCoding[] { new NSString(broadcastUrlString) };
            var setupInfo = NSDictionary<NSString, INSCoding>.FromObjectsAndKeys(objects, keys);

            // Tell ReplayKit that the extension is finished setting up and can begin broadcasting
            ExtensionContext.CompleteRequest(broadcastUrl, setupInfo);
            // Tell ReplayKit that the extension is finished setting up and can begin broadcasting
            //ExtensionContext.CompleteRequest(broadcastURL, setupInfo);
        }

        private void UserDidCancelSetup()
        {
            // Tell ReplayKit that the extension was cancelled by the user
            ExtensionContext.CancelRequest(new NSError(new NSString("com.google.AppRTCMobile"), -1));
        }
       

        nfloat GetStringWidth(NSString str, UIFont font)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }
            if (font == null)
            {
                throw new ArgumentNullException(nameof(font));
            }

            var fontAttributes = new NSMutableDictionary
            {
                [UIStringAttributeKey.Font] = font
            };

            var attributes = new UIStringAttributes(fontAttributes);

            return str.GetSizeUsingAttributes(attributes).Width;
        }
        private void DidTap()
        {
            View.EndEditing(true);
        }
    }
}
