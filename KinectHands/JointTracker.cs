﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Kinect;

namespace KinectHands
{
    class JointTracker
    {
        /// <summary>
        /// Provides skeleton and joint data
        /// </summary>   

        public JointTracker()
        {
        }

        private Skeleton GetTrackedSkeleton(AllFramesReadyEventArgs e)
        {
            using (SkeletonFrame skeletonFrameData = e.OpenSkeletonFrame())
            {
                if (skeletonFrameData == null)
                {
                    return null;
                }

                // Kinect SDK always returns 6 skeleton
                const int skeletonCount = 6;
                Skeleton[] allSkeletons = new Skeleton[skeletonCount];

                skeletonFrameData.CopySkeletonDataTo(allSkeletons);

                //Get the first tracked skeleton out of the 6
                Skeleton trackedSkeleton = null;

                foreach (Skeleton skeleton in allSkeletons)
                {
                    // if no skeleton is tracked, null will be returned.
                    if (skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        trackedSkeleton = skeleton;
                        break;
                    }
                }


                return trackedSkeleton;
            }
        }

        public DepthImagePoint GetJointPosition(KinectSensor sensor, AllFramesReadyEventArgs e, JointType jointType)
        {

            Skeleton skeleton = GetTrackedSkeleton(e);
            DepthImagePoint depthPoint = new DepthImagePoint();

            if (skeleton != null)
            {
                Joint joint = skeleton.Joints[jointType];

                // Return the joint only if the joint is tracked, not inferred
                if (joint.TrackingState == JointTrackingState.Tracked)
                {
                    SkeletonPoint jointPoint = joint.Position;
                    depthPoint = sensor.MapSkeletonPointToDepth(jointPoint, DepthImageFormat.Resolution320x240Fps30);
                }
            }

            return depthPoint;
        }
    }
}
