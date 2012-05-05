using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using AForge;
using AForge.Imaging;
using AForge.Math;
using AForge.Math.Geometry;

namespace KinectHands
{
    /// <summary>
    /// Functions to detect, highlight and track quadrilaterals
    /// </summary>
    public class QuadDetector
    {

        public QuadDetector()
        {
        }

        public Bitmap ProcessFrame(Bitmap inputBitmap)
        {
            BlobCounter blobCounter = new BlobCounter();

            // Set the blob detection parameters
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 15;
            blobCounter.MinWidth = 15;
            blobCounter.MaxHeight = 100;
            blobCounter.MaxWidth = 100;

            // Create an image for AForge to process
            Bitmap workingImage = new Bitmap(inputBitmap.Width, inputBitmap.Height);
            workingImage = AForge.Imaging.Image.Clone(inputBitmap, PixelFormat.Format24bppRgb);

            blobCounter.ProcessImage(workingImage);
            Blob[] blobs = blobCounter.GetObjectsInformation();

            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();

            Graphics g = Graphics.FromImage(workingImage);
            Pen yellowPen = new Pen(Color.Yellow, 2); // circles
            Pen redPen = new Pen(Color.Red, 2);

            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                List<IntPoint> corners;

                if (shapeChecker.IsQuadrilateral(edgePoints, out corners))
                {
                    g.DrawPolygon(redPen, ToPointsArray(corners));
                }
            }

            return workingImage;
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
