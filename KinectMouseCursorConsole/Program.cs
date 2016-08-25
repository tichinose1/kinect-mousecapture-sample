using KinectMouseCursorConsole.Properties;
using Microsoft.Kinect;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace KinectMouseCursorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Execute();

            Console.ReadLine();
        }

        private static double primaryScreenWidth = 0d;
        private static double primaryScreenHeight = 0d;

        private static void Execute()
        {
            CalculateScreenSize();

            var sensor = KinectSensor.GetDefault();
            var reader = sensor.BodyFrameSource.OpenReader();
            Task.Run(() => sensor.Open());

            var raw = Observable.Interval(TimeSpan.FromSeconds(1.0 / Settings.Default.FPS))
                .Select(_ =>
                {
                    using (var frame = reader.AcquireLatestFrame())
                    {
                        if (frame == null) return null;

                        var data = new Body[frame.BodyCount];
                        frame.GetAndRefreshBodyData(data);
                        return data;
                    }
                })
                .Where(bs => bs != null)
                .Select(bs => bs.Where(b => b.IsTracked).FirstOrDefault())
                .Where(b => b != null)
                .Select(b => b.Joints[JointType.HandRight])
                .Select(h => h.Position);

            raw
                .Subscribe(p => Debug.WriteLine($"X: {p.X}, Y: {p.Y}, Z: {p.Z}"));

            // カーソル移動
            // 座標変換後、指定の数値の倍数に丸める
            raw
                .Select(p => new
                {
                    X = primaryScreenWidth / 2 + Settings.Default.DirectionX * Settings.Default.Scale * p.X,
                    Y = primaryScreenHeight / 2 + Settings.Default.DirectionY * Settings.Default.Scale * p.Y,
                })
                //.Do(a => Debug.WriteLine($"a.X: {a.X}, a.Y: {a.Y}"))
                .Select(a => new
                {
                    X = Round(a.X),
                    Y = Round(a.Y),
                })
                //.DistinctUntilChanged()
                .Subscribe(a =>
                {
                    //Debug.WriteLine($"a.X: {a.X}, a.Y: {a.Y}");
                    PInvoke.PerformMoveCursor(a.X, a.Y);
                });

            // 長押し
            // 一定期間内に指先が常に指定範囲内にいるかどうかを監視
            // クリック後は手前に引く必要あり
            raw
                .Select(p => p.Z > Settings.Default.RangeInZ)
                .Buffer(TimeSpan.FromSeconds(Settings.Default.LongClickTime))
                .Select(bs => bs.All(b => b))
                .DistinctUntilChanged()
                .Where(b => b)
                .Subscribe(_ =>
                {
                    PInvoke.PerformClick();
                });
        }

        static void CalculateScreenSize()
        {
            var dpiX = PInvoke.GetDpiX();
            var dpiY = PInvoke.GetDpiY();
            Console.WriteLine($"dpiX: {dpiX}");
            Console.WriteLine($"dpiY: {dpiY}");

            const double LogicalInch = 96;
            var dipX = (double)dpiX / LogicalInch;
            var dipY = (double)dpiY / LogicalInch;
            Console.WriteLine($"dipX: {dipX}");
            Console.WriteLine($"dipY: {dipY}");

            primaryScreenWidth = SystemParameters.PrimaryScreenWidth * dipX;
            primaryScreenHeight = SystemParameters.PrimaryScreenHeight * dipY;
            Console.WriteLine($"primaryScreenWidth: {primaryScreenWidth}");
            Console.WriteLine($"primaryScreenHeight: {primaryScreenHeight}");
        }

        static int Round(double originalValue) => Round(originalValue, Settings.Default.RoundCoefficient);

        static int Round(double originalValue, int n) => ((int)originalValue / n) * n;
    }
}
