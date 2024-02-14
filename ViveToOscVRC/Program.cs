using Rug.Osc;
using System.Diagnostics;
using System.Net;
using System.Numerics;
using System.Reflection;
using Valve.VR;

namespace ViveToOscVRC
{
    internal class Program
    {
        // tracking/tracker/1/position

        static async Task Main()
        {
            IPAddress address = IPAddress.Parse("192.168.0.37");
            //IPAddress address = IPAddress.Parse("127.0.0.1");
            int port = 9000;

            OscSender sender = new OscSender(address, port);
            sender.Connect();

            InitializeOpenVR();

            while (true)
            {
                await Console.Out.WriteLineAsync("\nTracked devices");

                for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
                {
                    var deviceClass = vrSystem.GetTrackedDeviceClass(i);
                    if (deviceClass != ETrackedDeviceClass.GenericTracker) continue;


                    TrackerInfo? trackerInfo = GetTrackerPose(i);
                    if (trackerInfo == null) continue;

                    Vector3 pos = trackerInfo.Value.Position;
                    Vector3 rot = trackerInfo.Value.EulerRotation;

                    sender.Send(new OscMessage($"/tracking/trackers/{i}/position", pos.X, pos.Y, pos.Z));
                    sender.Send(new OscMessage($"/tracking/trackers/{i}/rotation", rot.X, rot.Y, rot.Z));
                }

                await Task.Delay(100);
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
                var mat34 = pose.mDeviceToAbsoluteTracking;

                // Position
                Vector3 position = new Vector3(mat34.m3,
                                           mat34.m7,
                                           mat34.m11);

                // Rotation (quaternion)   
                Quaternion rotation = GetRotationFromMatrix(mat34);
                Vector3 EulerRotation = RotateVector(Vector3.UnitY, rotation);

                Console.WriteLine($"[{trackerIndex}] Pos: {position} Rot: {EulerRotation}");

                TrackerInfo trackerInfo = new TrackerInfo();
                trackerInfo.Position = position;
                trackerInfo.EulerRotation = EulerRotation;
                trackerInfo.Rotation = rotation;

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

        public static Quaternion GetRotationFromMatrix(HmdMatrix34_t matrix)
        {
            Quaternion q = new Quaternion();
            q.W = (float)Math.Sqrt(Math.Max(0, 1 + matrix.m0 + matrix.m5 + matrix.m10)) / 2;
            q.X = (float)Math.Sqrt(Math.Max(0, 1 + matrix.m0 - matrix.m5 - matrix.m10)) / 2;
            q.Y = (float)Math.Sqrt(Math.Max(0, 1 - matrix.m0 + matrix.m5 - matrix.m10)) / 2;
            q.Z = (float)Math.Sqrt(Math.Max(0, 1 - matrix.m0 - matrix.m5 + matrix.m10)) / 2;
            q.X *= Math.Sign(q.X * (matrix.m9 - matrix.m6));
            q.Y *= Math.Sign(q.Y * (matrix.m2 - matrix.m8));
            q.Z *= Math.Sign(q.Z * (matrix.m4 - matrix.m1));
            return q;
        }

        static Vector3 RotateVector(Vector3 vector, Quaternion quaternion)
        {
            // Rotate the vector using the quaternion
            Quaternion conjugate = Quaternion.Conjugate(quaternion);
            Quaternion result = quaternion * new Quaternion(vector, 0) * conjugate;

            // Extract the rotated vector from the resulting quaternion
            return new Vector3(result.X, result.Y, result.Z);
        }
    }
}
