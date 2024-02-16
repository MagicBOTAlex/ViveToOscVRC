using Rug.Osc;
using System;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using Valve.VR;

namespace ViveToOscVRC
{
    internal class Program
    {
        // tracking/tracker/1/position

        static float lowestPoint = 0;

        static async Task Main()
        {
            IPAddress address = IPAddress.Parse("192.168.0.37");
            //IPAddress address = IPAddress.Parse("127.0.0.1");
            int port = 9000;

            OscSender sender = new OscSender(address, port);
            sender.Connect();

            InitializeOpenVR();
            int numOfLines = 0;
            StringBuilder sb = new StringBuilder();

            while (true)
            {
                sb.Append($"\nTracked devices. Floor offset: {lowestPoint}");
                numOfLines++;

                // If tracker flies away, then reset floor offset
                if (Math.Abs(lowestPoint) > 3)
                {
                    lowestPoint = 0;
                }

                for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
                {
                    var deviceClass = vrSystem.GetTrackedDeviceClass(i);
                    if (deviceClass != ETrackedDeviceClass.GenericTracker) continue;


                    TrackerInfo? trackerInfo = GetTrackerPose(i);
                    if (trackerInfo == null) continue;

                    Vector3 pos = trackerInfo.Value.Position;
                    Vector3 rot = trackerInfo.Value.EulerRotation;
                    //rot *= 180;

                    //rot = new Vector3(rot.X, rot.Y, rot.Z);

                    if (lowestPoint > pos.Y)
                    {
                        lowestPoint = pos.Y;
                    }
                    else
                    {
                        pos.Y -= lowestPoint;
                    }


                    sender.Send(new OscMessage($"/tracking/trackers/{i}/position", pos.X, pos.Y, pos.Z));
                    sender.Send(new OscMessage($"/tracking/trackers/{i}/rotation", rot.X, rot.Y, rot.Z));

                    sb.Append($"\nTracker {i}\nPosition: ( {pos.X.ToString("+0.00;-0.00")}, {pos.Y.ToString("+0.00;-0.00")}, {pos.Z.ToString("+0.00;-0.00")})\nRotation: ( {rot.X.ToString("+000.0;-000.0")}, {rot.Y.ToString("+000.0;-000.0")}, {rot.Z.ToString("+000.0;-000.0")})\n");
                    numOfLines += 4;
                }

                await Console.Out.WriteAsync(sb.ToString());
                sb.Clear();
                Console.SetCursorPosition(0, 0);
                numOfLines = 0;
                await Task.Delay(1);
            }
        }

        static CVRSystem vrSystem = default!;

        public static void InitializeOpenVR()
        {
            var error = EVRInitError.None;
            vrSystem = OpenVR.Init(ref error, EVRApplicationType.VRApplication_Background);

            if (error != EVRInitError.None)
            {
                throw new Exception("Unable to initialize OpenVR: " + error.ToString());
            }
        }

        public static void ShutdownOpenVR()
        {
            OpenVR.Shutdown();
        }

        public static TrackerInfo? GetTrackerPose(uint trackerIndex)
        {
            var poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            vrSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

            var pose = poses[trackerIndex];
            if (pose.bPoseIsValid)
            {
                HmdMatrix34_t mat34 = pose.mDeviceToAbsoluteTracking;

                // Position
                Vector3 position = new Vector3(mat34.m3,
                                           mat34.m7,
                                           mat34.m11);

                // Don't know why but this has to be done apparently
                position = new Vector3(position.X, position.Y, -position.Z);

                // Rotation
                Vector3 rotation = mat34.ExtractRotation().ToEulerAngles();

                //rotation = rotation.ToDegrees();

                //rotation = new Vector3(rotation.Z, rotation.X, rotation.Y); // rotation not correct
                //Vector3 rotation = new Vector3(mat34.m2, mat34.m6, mat34.m10);
                //rotation *= 360;

                TrackerInfo trackerInfo = new TrackerInfo();
                trackerInfo.Position = position;
                trackerInfo.EulerRotation = rotation;

                return trackerInfo;
            }

            return null;
        }

        public struct TrackerInfo
        {
            public Vector3 Position;
            public Vector3 EulerRotation;
            public Quaternion Rotation;
        }
    }
}
