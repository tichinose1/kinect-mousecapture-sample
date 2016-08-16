using KinectMouseCursorConsole.Properties;
using Microsoft.Kinect;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace KinectMouseCursorConsole
{
    class Program
    {
        static void Main(string[] args)
        {
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
                .Do(h => Debug.WriteLine($"X: {h.Position.X}, Y: {h.Position.Y}, Z: {h.Position.Z}"))
                .Select(h => h.Position);

            // カーソル移動
            // 座標変換後、指定の数値の倍数に丸める
            raw
                .Select(p => new
                {
                    X = Settings.Default.ScreenX / 2 + Settings.Default.Scale * p.X,
                    Y = Settings.Default.ScreenY / 2 - Settings.Default.Scale * p.Y
                })
                .Select(a => new { X = Round(a.X), Y = Round(a.Y) })
                .DistinctUntilChanged()
                .Subscribe(a =>
                {
                    PInvoke.PerformMoveCursor(a.X, a.Y);
                });

            // 長押し
            // 一定期間内に指先が常に指定範囲内にいるかどうかを監視
            // クリック後は手前に引く必要あり
            raw
                .Select(p => p.Z < Settings.Default.RangeInZ)
                .Buffer(TimeSpan.FromSeconds(Settings.Default.LongClickTime))
                .Select(bs => bs.All(b => b))
                .DistinctUntilChanged()
                .Where(b => b)
                .Subscribe(_ =>
                {
                    PInvoke.PerformClick();
                });

            Console.ReadKey();
        }

        static int Round(double originalValue)
        {
            var n = Settings.Default.RoundCoefficient;
            return ((int)originalValue / n) * n;
        }
    }
}
