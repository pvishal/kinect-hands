using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Imaging.Filters;
using AForge;
using AForge.Imaging;
using AForge.Math.Geometry;
using System.Drawing;
using System.Drawing.Imaging;

namespace KinectHands
{
    /// <summary>
    /// Has functions to detect and highlight blobs.
    /// </summary>
    public class BlobTracker
    {
        private Bitmap image = null;
        private int imageWidth, imageHeight;

        private BlobCounter blobCounter = new BlobCounter();

        // The current blob being processed and displayed
        private Blob currentBlob;
        public int TotalBlobCount;
        public int TrackedBlobCount;
        public Bitmap TrackedBlobImage;

        // The blob being currently tracked
        private AForge.Point Position;

        public BlobTracker()
        {
            TrackedBlobImage = new Bitmap(320, 240);
        }

        private int FindLargestBlob()
        {
            Blob[] blobs;
            int blobCount;

            blobCounter.ObjectsOrder = ObjectsOrder.Area;

            // Find the blobs
            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            blobCount = blobs.Count();

            if (blobCount > 0)
            {
                currentBlob = blobs[0];
            }

            return blobCount;
        }

        private int FindNearestBlob()
        {
            Blob[] blobs;
            int blobCount;

            blobCounter.ObjectsOrder = ObjectsOrder.None;

            // Find the blobs
            blobCounter.ProcessImage(this.image);
            blobs = blobCounter.GetObjectsInformation();

            blobCount = blobs.Count();
            TotalBlobCount = blobCount;

            if (blobCount > 0)
            {
                Blob nearestBlob = new Blob(blobs[0]);
                float leastDistance = nearestBlob.CenterOfGravity.DistanceTo(this.Position);
                float currentDistance;

                foreach (Blob blob in blobs)
                {
                    currentDistance = blob.CenterOfGravity.DistanceTo(this.Position);
                    if (currentDistance < 50)
                    {
                        this.TrackedBlobCount = 1;
                        if (currentDistance < leastDistance)
                        {
                            nearestBlob = blob;
                            leastDistance = currentDistance;
                        }
                    }
                    else
                    {
                        this.TrackedBlobCount = 0;
                    }
                }

                if (this.TrackedBlobCount > 0)
                {
                    this.currentBlob = nearestBlob;
                }
                else
                {
                    // Do not update the current blob
                }
            }
            else
            {
                this.TrackedBlobCount = 0;
            }

            return blobCount;
        }

        public Bitmap ProcessFrame(Bitmap depthImage, int trackMode)
        {   
            // Create an image for AForge to process
            this.image = AForge.Imaging.Image.Clone(depthImage, PixelFormat.Format24bppRgb);
            imageWidth = this.image.Width;
            imageHeight = this.image.Height;

            // Set the blob detection parameters
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 15;
            blobCounter.MinWidth = 15;
            blobCounter.MaxHeight = 100;
            blobCounter.MaxWidth = 100;

            // Find the blob of interest
            if (trackMode == 0)
            {
                TotalBlobCount = FindLargestBlob();
                if (TotalBlobCount > 0)
                {
                    blobCounter.ExtractBlobsImage(this.image, currentBlob, true);
                    TrackedBlobImage = currentBlob.Image.ToManagedImage();

                    //Remember the blob
                    this.Position = currentBlob.CenterOfGravity;
                }
                else
                {
                    //Clear the output bitmap
                    using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(TrackedBlobImage))
                    {
                        g.Clear(System.Drawing.Color.Black);
                    }
                }

                TrackedBlobCount = 0;
            }
            else if (trackMode == 1)
            {
                // Find the blob closest to trackedBlob
                TotalBlobCount = FindNearestBlob();
                
                // At this point currentBlob would either be the older trackedBlob or the current blob 
                // closest to it. In any case, get its image.

                if (TrackedBlobCount > 0)
                {
                    blobCounter.ExtractBlobsImage(this.image, currentBlob, true);
                    this.TrackedBlobImage = currentBlob.Image.ToManagedImage();
                }

                //Remember this blob
                this.Position = currentBlob.CenterOfGravity;
            }

            return TrackedBlobImage;
        }
    }
}
