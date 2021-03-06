using System;
using System.IO;
using NUnit.Framework;
using Unity.PerformanceTesting;

namespace Benchmark
{
    [TestFixture]
    [Category("benchmark")]
    public class BinaryWriteMethods
    {
        [Test, Performance]
        public unsafe void Benchmark([Values(8, 128, 512, 1024, 1024*8, 1024*64, 1024*256)] int chunkSize)
        {
            var totalSize = 1024 * 1024;
            var iterCount = Math.Min(totalSize / chunkSize, 1024);
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
                .SampleGroup("Stream.Write(Span)")
                .SetUp(() => stream = new MemoryStream(totalSize))
                .MeasurementCount(measurementCount)
                .WarmupCount(warmupCount)
                .IterationsPerMeasurement(iterCount)
                .Run()
            ;

            Measure.Method(() =>
                {
                    fixed (void* ptr = &chunk[0])
                    {
                        var span = new ReadOnlySpan<byte>(ptr, chunk.Length);
                        stream.Write(span);
                    }
                })
                .SampleGroup("Stream.Write(pointer->Span)")
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