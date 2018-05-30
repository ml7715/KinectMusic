using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.Controls;
using System.Net.Http;
using System.Net.Http.Headers;

namespace skeletal_tracking
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private KinectSensorChooser sensorChooser;
        private Skeleton[] skeletonData;
        float cameraHeight;

        // Used for Spotify network
        HttpClient client = new HttpClient();

        string waveButtonName = "SineButton";

        string token = "BQBsLh1qGPrWSL4CgfkqLeHTvzCm6AzqnjlSbfNvRACjNVTUlJXtDFl5H0x6Ps9YS5V6LUFf0yUtuEbOGp4MIAXq3uUfE5JJBCVs9DNFKYILLEWXfLe4rDT_11h7eZbnW7RHiAJ8Bhpg9BKroWP6gHdyELpDAWI7rDF6bj30B43OUArtxuj2J96tc2E_0BaglXKNzj1JixbGzLVuRnQDf8TqXk853f7FW293WEMEv_ClMFdPDMyGQGJzNbJVykKswF8x6l558C8";
        
        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            this.sensorChooser = new KinectSensorChooser();
            this.sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
            this.sensorChooserUi.KinectSensorChooser = this.sensorChooser;
            this.sensorChooser.Start();
        }

        private void SensorChooserOnKinectChanged(object sender, KinectChangedEventArgs args)
        {
            bool error = false;
            if (args.OldSensor != null)
            {
                try
                {
                    args.OldSensor.DepthStream.Range = DepthRange.Default;
                    args.OldSensor.SkeletonStream.EnableTrackingInNearRange = false;
                    args.OldSensor.DepthStream.Disable();
                    args.OldSensor.SkeletonStream.Disable();
                }
                catch (InvalidOperationException)
                {
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                    error = true;
                }
            }

            if (args.NewSensor != null)
            {
                try
                {
                    args.NewSensor.DepthStream.Enable(DepthImageFormat.Resolution640x480Fps30);
                    args.NewSensor.SkeletonStream.Enable();

                    skeletonData = new Skeleton[args.NewSensor.SkeletonStream.FrameSkeletonArrayLength];

                    //args.NewSensor.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(KinectSkeletonFrameReady);

                }
                catch (InvalidOperationException)
                {
                    error = true;
                    // KinectSensor might enter an invalid state while enabling/disabling streams or stream features.
                    // E.g.: sensor might be abruptly unplugged.
                }
            }

            if (!error)
                kinectRegion.KinectSensor = args.NewSensor;
        }

        private void KinectSkeletonFrameReady(object sender, SkeletonFrameReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null && this.skeletonData != null)
                {
                    cameraHeight = skeletonFrame.FloorClipPlane.Item4;
                    skeletonFrame.CopySkeletonDataTo(this.skeletonData);
                }
            }
            HandleSkeletons();
        }

        private void HandleSkeletons()
        {
            foreach (Skeleton skeleton in this.skeletonData)
            {
                if(skeleton == null)
                {
                    continue;
                }
                else if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                {
                    HandleJoints(skeleton.Joints);
                }
            }
        }

        private void HandleJoints(JointCollection jointCollection)
        {
            Joint handLeft = jointCollection[JointType.HandLeft];
            Joint handRight = jointCollection[JointType.HandRight];

            double leftX_Real = (handLeft.Position.X + 0.7) / 1.4;
            double leftY_Real = (handLeft.Position.Y + 0.7 - cameraHeight) / 1.4;
            double rightX_Real = (handRight.Position.X + 0.7) / 1.4;
            double rightY_Real = (handRight.Position.Y + 0.7 - cameraHeight) / 1.4;

            double leftX = leftX_Real;
            double leftY = 1.0 - leftY_Real;
            double rightX = rightX_Real;
            double rightY = 1.0 - rightY_Real;

            double normX_Left = leftX * canvas.ActualWidth;
            double normY_Left = leftY * canvas.ActualHeight;
            double normX_Right = rightX * canvas.ActualWidth;
            double normY_Right = rightY * canvas.ActualHeight;

            double leftRed = normX_Left - (RedCircle.ActualWidth / 2.0);
            double topRed = normY_Left - (RedCircle.ActualHeight / 2.0);
            Canvas.SetLeft(RedCircle, leftRed);
            Canvas.SetTop(RedCircle, topRed);

            double leftBlue = normX_Right - (BlueCircle.ActualWidth / 2.0);
            double topBlue = normY_Right - (BlueCircle.ActualHeight / 2.0);
            Canvas.SetLeft(BlueCircle, leftBlue);
            Canvas.SetTop(BlueCircle, topBlue);

            float newFreq = (float) (Math.Pow(2, -2 + 4*rightY_Real));

            if(newFreq >= 0.0 && newFreq <= 4.0)
                SoundCode.ChangePitch(newFreq);

            float newVol = (float)leftY_Real;
            
            if(newVol >= 0.0 && newVol <= 1.0)
                SoundCode.ChangeVolume(newVol);          


            if(leftY_Real > 0.5)
            {
                ArrowLeftUp.Fill = Brushes.LightYellow;
                ArrowLeftDown.Fill = Brushes.Gold;
            }
            else if(leftY_Real < 0.5)
            {
                ArrowLeftDown.Fill = Brushes.LightYellow;
                ArrowLeftUp.Fill = Brushes.Gold;
            }

            if(rightY_Real > 0.5)
            {
                ArrowRightUp.Fill = Brushes.LightYellow;
                ArrowRightDown.Fill = Brushes.Gold;
            }
            else if(rightY_Real < 0.5)
            {
                ArrowRightDown.Fill = Brushes.LightYellow;
                ArrowRightUp.Fill = Brushes.Gold;
            }
        }

        private String PrettyCoords(double leftX_Real, double leftY_Real, double rightX_Real, double rightY_Real)
        {
            return leftX_Real + " " + leftY_Real + "\n" + rightX_Real + " " + rightY_Real;
        }

        private void KinectCircleButton_Click(object sender, RoutedEventArgs e)
        {
            FileButton.Background = Brushes.White;
            MicButton.Background = Brushes.White;
            SineButton.Background = Brushes.White;
            TrigButton.Background = Brushes.White;
            SquaButton.Background = Brushes.White;
            SawButton.Background = Brushes.White;
            ((KinectCircleButton)sender).Background = Brushes.AliceBlue;
            FrequencyController.Visibility = Visibility.Visible;
            SourceSetter.Visibility = Visibility.Hidden;

            waveButtonName = ((KinectCircleButton)sender).Name;
        }

        private void KinectCircleButton_Click_1(object sender, RoutedEventArgs e)
        {
            SourceSetter.Visibility = Visibility.Hidden;
            FileNameTextBox.Visibility = Visibility.Visible;
            button1.Visibility = Visibility.Visible;
        }

        private void browse_Click(object sender, RoutedEventArgs e)
        {
            string filename = @"C:\Users\marcl\Desktop\sample.mp3";

            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".mp3";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
      
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                filename = dlg.FileName;
                FileNameTextBox.Text = filename;
            }

            SoundCode.InitialiseFileStream(filename);
        }

        private void KinectCircleButton_Mic(object sender, RoutedEventArgs e)
        {
            FileButton.Background = Brushes.White;
            MicButton.Background = Brushes.White;
            SineButton.Background = Brushes.White;
            TrigButton.Background = Brushes.White;
            SquaButton.Background = Brushes.White;
            SawButton.Background = Brushes.White;
            MicButton.Background = Brushes.AliceBlue;

            SoundCode.InitialiseMic();
        }

        private void Interact_Click(object sender, RoutedEventArgs e)
        {
            kinectRegion.Visibility = Visibility.Hidden;
            canvas.Visibility = Visibility.Visible;

            Canvas.SetLeft(BlueCircle, (canvas.ActualWidth - BlueCircle.ActualWidth) / 2);
            Canvas.SetTop(BlueCircle, (canvas.ActualHeight - BlueCircle.ActualHeight) / 2);
            Canvas.SetLeft(RedCircle, (canvas.ActualWidth - RedCircle.ActualWidth) / 2);
            Canvas.SetTop(RedCircle, (canvas.ActualHeight - RedCircle.ActualHeight) / 2);
            if (SpotifyButton.Background != Brushes.AliceBlue)
            {
                Task state = SoundCode.ReproduceSound();
            }
            sensorChooser.Kinect.SkeletonFrameReady += new EventHandler<SkeletonFrameReadyEventArgs>(KinectSkeletonFrameReady);
            this.GetTrackData();
        }

        private void ButtonSetFrequency(object sender, RoutedEventArgs e)
        {
            FrequencyController.Visibility = Visibility.Hidden;
            SourceSetter.Visibility = Visibility.Visible;

            var noteName = ((KinectCircleButton)sender).Name;
            var corrFreq = NameToFreq(noteName);

            SoundCode.setFreq = corrFreq;

            switch (waveButtonName)
            {
                case "SineButton":
                    SoundCode.GenerateSineWave();
                    break;
                case "TrigButton":
                    SoundCode.GenerateTrigWave();
                    break;
                case "SquaButton":
                    SoundCode.GenerateSquareWave();
                    break;
                case "SawButton":
                    SoundCode.GenerateSawToothWave();
                    break;
            }
        }

        private double NameToFreq(String noteName)
        {
            var freq = 261.63;

            switch (noteName)
            {
                case "C4":
                    freq = 261.63;
                    break;
                case "C_4":
                    freq = 277.18;
                    break;
                case "D4":
                    freq = 293.66;
                    break;
                case "D_4":
                    freq = 311.13;
                    break;
                case "E4":
                    freq = 329.63;
                    break;
                case "F4":
                    freq = 349.23;
                    break;
                case "F_4":
                    freq = 369.99;
                    break;
                case "G4":
                    freq = 392.00;
                    break;
                case "G_4":
                    freq = 415.30;
                    break;
                case "A4":
                    freq = 440.00;
                    break;
                case "A_4":
                    freq = 466.16;
                    break;
                case "B4":
                    freq = 493.88;
                    break;
            }

            return freq;
        }

        private async void ButtonPlay(object sender, RoutedEventArgs e)
        {
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync("https://api.spotify.com/v1/me/player/play?device_id=33117cdda7e7ff420265d5e02a51bd4d6584daf8", requestContent);
        }

        private async void ButtonPause(object sender, RoutedEventArgs e)
        {
            // Create the HttpContent for the form to be posted.
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PutAsync("https://api.spotify.com/v1/me/player/pause?device_id=33117cdda7e7ff420265d5e02a51bd4d6584daf8", requestContent);
        }

        private async void ButtonNext(object sender, RoutedEventArgs e)
        {
            // Create the HttpContent for the form to be posted.
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json")
            });
            
            // Get the response.
            HttpResponseMessage response = await client.PostAsync("https://api.spotify.com/v1/me/player/next?device_id=33117cdda7e7ff420265d5e02a51bd4d6584daf8", requestContent);
        }

        private async void ButtonPrevious(object sender, RoutedEventArgs e)
        {
            // Create the HttpContent for the form to be posted.
            var requestContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string, string>("Accept", "application/json"),
                new KeyValuePair<string, string>("Content-Type", "application/json"),
            });

            // Get the response.
            HttpResponseMessage response = await client.PostAsync("https://api.spotify.com/v1/me/player/previous?device_id=33117cdda7e7ff420265d5e02a51bd4d6584daf8", requestContent);

        }

        private void GetTrackData()
        {

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(@"https://api.spotify.com/v1/me/player/currently-playing?market=ES&Accept=application/json&Content-Type=application/json&Bearer=" + this.token);
            //request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            //using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            //using (Stream stream = response.GetResponseStream())
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    SongName.Text = await reader.ReadToEndAsync();
            //}

            //var client = new RestClient("https://api.spotify.com/v1/me/player/currently-playing");
            //var request = new RestRequest(Method.GET);
            //request.AddHeader("authorization", "Bearer " + this.token);
            //request.AddHeader("content-type", "application/x-www-form-urlencoded");
            //request.RequestFormat = RestSharp.DataFormat.Json;

            //IRestResponse response = client.Execute(request);

            //RestSharp.Deserializers.JsonDeserializer deserial = new RestSharp.Deserializers.JsonDeserializer();

            //var JSONObj = deserial.Deserialize<Dictionary<string, dynamic>>(response);
            //string title = (JSONObj["item"]["name"]);

            //SongName.Text = title;

        }

        private void KinectCircleButton_Spotify(object sender, RoutedEventArgs e)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", this.token);

            SpotifyController.Visibility = Visibility.Visible;

            FileButton.Background = Brushes.White;
            MicButton.Background = Brushes.White;
            SineButton.Background = Brushes.White;
            TrigButton.Background = Brushes.White;
            SquaButton.Background = Brushes.White;
            SawButton.Background = Brushes.White;
            MicButton.Background = Brushes.White;
            SpotifyButton.Background = Brushes.AliceBlue;

            this.SongFrame.Visibility = Visibility.Visible;
            this.SongTitle.Visibility = Visibility.Visible;
            this.Logo.Visibility = Visibility.Visible;
            this.TitleIntestation.Visibility = Visibility.Visible;

            SoundCode.InitialiseMic();

            Task state = SoundCode.ReproduceSound();
        }
    }

}