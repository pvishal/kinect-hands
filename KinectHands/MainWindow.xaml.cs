using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.Kinect;

namespace KinectHands
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        ImageProcessor imageProcessor = new ImageProcessor();
        JointTracker jointTracker = new JointTracker();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            kinectSensorChooser.KinectSensorChanged += new DependencyPropertyChangedEventHandler(kinectSensorChooser_KinectSensorChanged);
        }

        private void buttonTilt_Click(object sender, RoutedEventArgs e)
        {
            //Set angle to slider1 value
            if (kinectSensorChooser.Kinect != null && kinectSensorChooser.Kinect.IsRunning)
            {
                kinectSensorChooser.Kinect.ElevationAngle = (int)sliderTilt.Value;
            }
        }

        void kinectSensorChooser_KinectSensorChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

            var oldSensor = (KinectSensor)e.OldValue;

            //stop the old sensor
            if (oldSensor != null)
            {
                oldSensor.Stop();
                oldSensor.AudioSource.Stop();
            }

            //get the new sensor
            var newSensor = (KinectSensor)e.NewValue;
            if (newSensor == null)
            {
                return;
            }

            //turn on features that you need
            newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
            newSensor.SkeletonStream.Enable();

            //sign up for events if you want to get at API directly
            newSensor.AllFramesReady += new EventHandler<AllFramesReadyEventArgs>(sensor_AllFramesReady);


            try
            {
                newSensor.Start();
            }
            catch (System.IO.IOException)
            {
                //this happens if another app is using the Kinect
                kinectSensorChooser.AppConflictOccurred();
            }
        }


        void sensor_AllFramesReady(object sender, AllFramesReadyEventArgs e)
        {
            using (DepthImageFrame depthFrame = e.OpenDepthImageFrame())
            {
                if (depthFrame == null)
                {
                    return;
                }

                if (sliderMinDist.Value > sliderMaxDist.Value)
                {
                    sliderMinDist.Value = sliderMaxDist.Value;
                }

                textMinDistVal.Text = sliderMinDist.Value.ToString();
                textMaxDistVal.Text = sliderMaxDist.Value.ToString();

                BitmapSource depthBitmapSource = sliceDepthImage(depthFrame, (int)sliderMinDist.Value, (int)sliderMaxDist.Value);
                
                //Create a bitmap from the depth information 
                System.Drawing.Bitmap depthBmp = depthBitmapSource.ToBitmap();
                System.Drawing.Bitmap outBmp = new System.Drawing.Bitmap(depthBmp.Width, depthBmp.Height);

                //Get the position of interest on the depthmap from skeletal tracking
                DepthImagePoint rightHandPoint = jointTracker.GetJointPosition(kinectSensorChooser.Kinect, e, JointType.HandRight);

                //Aforge performs image processing here.
                outBmp = imageProcessor.ProcessFrame(depthBmp, rightHandPoint.X, rightHandPoint.Y);

                //textResult.Text = blobsDetector.TotalBlobCount + " blobs detected.";

                //Create a bitmapsource to show the processed image
                BitmapSource processedBitmapSource = outBmp.ToBitmapSource();

                //Display the images
                depthImageDisplay.Source = depthBitmapSource;
                procImageDisplay.Source = processedBitmapSource;
            }
        }

        private static BitmapSource sliceDepthImage(DepthImageFrame depthFrame, int min, int max)
        {
            byte[] pixels = GenerateColoredBytes(depthFrame, min, max);

            return BitmapSource.Create(depthFrame.Width, depthFrame.Height, 96, 96, PixelFormats.Bgr32, null, pixels, depthFrame.Width*4);
        }

        private static Byte[] GenerateColoredBytes(DepthImageFrame depthFrame, int minRange, int maxRange)
        {
            //get the raw data from kinect with the depth for every pixel
            short[] rawDepthData = new short[depthFrame.PixelDataLength];
            depthFrame.CopyPixelDataTo(rawDepthData);

            //use depthFrame to create the image to display on-screen
            //depthFrame contains color information for all pixels in image
            //Height x Width x 4 (Red, Green, Blue, empty byte)
            Byte[] pixels = new Byte[depthFrame.Height * depthFrame.Width * 4];

            //Bgr32  - Blue, Green, Red, empty byte
            //Bgra32 - Blue, Green, Red, transparency 
            //You must set transparency for Bgra as .NET defaults a byte to 0 = fully transparent

            //hardcoded locations to Blue, Green, Red (BGR) index positions       
            const int BlueIndex = 0;
            const int GreenIndex = 1;
            const int RedIndex = 2;

            //loop through all distances
            //pick a RGB color based on distance
            for (int depthIndex = 0, colorIndex = 0; depthIndex < rawDepthData.Length && colorIndex < pixels.Length; depthIndex++, colorIndex += 4)
            {

                int depth = rawDepthData[depthIndex] >> DepthImageFrame.PlayerIndexBitmaskWidth;

                if (depth <= minRange || depth > maxRange)
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 0;
                    pixels[colorIndex + RedIndex] = 0;
                }
                else
                {
                    pixels[colorIndex + BlueIndex] = 0;
                    pixels[colorIndex + GreenIndex] = 255;
                    pixels[colorIndex + RedIndex] = 0;
                } 
            }


            return pixels;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopKinect(kinectSensorChooser.Kinect);
        }

        void StopKinect(KinectSensor sensor)
        {
            if (sensor != null)
            {
                sensor.Stop();
                sensor.AudioSource.Stop();
            }
        }
    }
}
