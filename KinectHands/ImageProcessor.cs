using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math;
using AForge.Math.Geometry;

namespace KinectHands
{
    /// <summary>
    /// Performs the core image processing and creates the output image
    /// </summary>
    public class ImageProcessor
    {

        public ImageProcessor()
        {
        }

        public Bitmap ProcessFrame(Bitmap inputBitmap, int x, int y)
        {
            
            // Create an image for AForge to process
            Bitmap workingImage = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            workingImage = AForge.Imaging.Image.Clone(inputBitmap, PixelFormat.Format24bppRgb);

            // Create a mask for ROI selection
            Rectangle roi = new Rectangle(x - 30, y-30, 80, 80);

            Crop roicrop = new Crop(roi);
            Bitmap outimage = roicrop.Apply(workingImage);
            
            //Graphics g = Graphics.FromImage(workingImage);
            //Pen redPen = new Pen(Color.Red, 2);

            //g.Clear(Color.Black);
            //g.DrawImage(handImage, x, y);
            //g.DrawRectangle(redPen, roi);
            //g.DrawEllipse(redPen, x, y, 20, 20);

            return outimage;
        }


        // Conver list of AForge.NET's points to array of .NET points
        private System.Drawing.Point[] ToPointsArray(List<IntPoint> points)
        {
            System.Drawing.Point[] array = new System.Drawing.Point[points.Count];

            for (int i = 0, n = points.Count; i < n; i++)
            {
                array[i] = new System.Drawing.Point(points[i].X, points[i].Y);
            }

            return array;
        }
    }
}
