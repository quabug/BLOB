using System.IO;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Benchmark
{
    public class BinaryWriteMethods
    {
        [Test, Performance]
        public unsafe void Write1MB([Values(8, 128, 512, 1024, 1024*256)] int chunkSize)
        {
            var totalSize = 1024 * 1024;
            var iterCount = totalSize / chunkSize;
            var chunk = new byte[chunkSize];
            var stream = new MemoryStream();
            const int measurementCount = 10;
            const int warmupCount = 2;

            Measure.Method(() =>
                {
                    for (var index = 0; index < chunkSize; index++) stream.WriteByte(chunk[index]);
                })
                .SampleGroup("WriteByte")
                .SetUp(() => stream = new MemoryStream(totalSize))
                .MeasurementCount(measurementCount)
                .WarmupCount(warmupCount)
                .IterationsPerMeasurement(iterCount)
                .Run()
            ;

            Measure.Method(() =>
                {
                    fixed (byte* ptr = &chunk[0])
                    {
                        using var unmanagedStream = new UnmanagedMemoryStream(ptr, chunkSize);
                        unmanagedStream.CopyTo(stream);
                    }
                })
                .SampleGroup("UnmanagedStream.CopyTo")
                .SetUp(() => stream = new MemoryStream(totalSize))
                .MeasurementCount(measurementCount)
                .WarmupCount(warmupCount)
                .IterationsPerMeasurement(iterCount)
                .Run()
            ;

            Measure.Method(() =>
                {
                    using var writer = new BinaryWriter(stream);
                    writer.Write(chunk);
                })
                .SampleGroup("BinaryWrite")
                .SetUp(() => stream = new MemoryStream(totalSize))
                .MeasurementCount(measurementCount)
                .WarmupCount(warmupCount)
                .IterationsPerMeasurement(iterCount)
                .Run()
            ;

#if UNITY_2021_2_OR_NEWER
            Measure.Method(() =>
                {
                    stream.Write(chunk);
                })
                .SampleGroup("Stream.Write")
                .SetUp(() => stream = new MemoryStream(totalSize))
                .MeasurementCount(measurementCount)
                .WarmupCount(warmupCount)
                .IterationsPerMeasurement(iterCount)
                .Run()
            ;
#endif
        }
    }
}