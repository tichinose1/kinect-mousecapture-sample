using KinectMouseCursorConsole.Properties;
using Microsoft.Kinect;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace KinectMouseCursorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var sensor = KinectSensor.GetDefault();
            var reader = sensor.BodyFrameSource.OpenReader();
            sensor.Open();

            var bodies = Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ =>
                {
                    using (var frame = reader.AcquireLatestFrame())
                    {
                        if (frame == null) return null;

                        var data = new Body[frame.BodyCount];
                        frame.GetAndRefreshBodyData(data);
                        return data;
                    }
                });
            var body = bodies
                .Where(bs => bs != null)
                .Select(bs => bs.Where(b => b.IsTracked).FirstOrDefault());
            var handRight = body
                .Where(b => b != null)
                .Select(b => b.Joints[JointType.HandRight]);
            var handRightPosition = handRight
                .Select(h => h.Position)
                .Do(p => Debug.WriteLine($"X: {p.X}, Y: {p.Y}"));
            handRightPosition
                .Subscribe(p =>
                {
                    var x = (1920 / 2) + p.X * Settings.Default.Scale;
                    var y = (1080 / 2) - p.Y * Settings.Default.Scale;

                    SetCursorPos((int)x, (int)y);

                    if (p.Z > 0.8) Debug.WriteLine("hoge");
                });

            Console.ReadKey();
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);
    }
}
