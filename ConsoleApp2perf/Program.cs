using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using GuidPhantom;

namespace ConsoleApp2perf
{
    [MemoryDiagnoser]
    [RankColumn]
    //[ShortRunJob]
    public class GuidBenchmarks
    {
        private static readonly Guid s_base = Guid.Parse("d2f2f0fe-cbf8-4dc8-9ecb-eedd066dc105");
        private static readonly Guid s_v7 = GuidKit.CreateVersion7();
        private static readonly Guid s_v8 = GuidKit.CreateVersion8MsSql();
        private static readonly Guid s_v1 = GuidKit.CreateVersion1();
        private static readonly Guid s_v6 = GuidKit.CreateVersion6();
        private static readonly Guid s_incBase = Guid.NewGuid();

		[Benchmark]
		public Guid NewGuid() => Guid.NewGuid();
		
		[Benchmark]
        public Guid CreateVersion7() => GuidKit.CreateVersion7();

        [Benchmark]
        public Guid CreateVersion8MsSql() => GuidKit.CreateVersion8MsSql();

        [Benchmark]
        public Guid CreateVersion1() => GuidKit.CreateVersion1();

        [Benchmark]
        public Guid CreateVersion6() => GuidKit.CreateVersion6();

        [Benchmark]
        public Guid CreateIncrementedGuid() => s_incBase.CreateIncrementedGuid(42);

        [Benchmark]
        public int ReverseIncrementedGuid()
        {
            var inc = s_incBase.CreateIncrementedGuid(42);
            return inc.ReverseIncrementedGuid(s_incBase);
        }

        [Benchmark]
        public GuidInfo GetGuidInfo() => s_v7.GetGuidInfo();

        [Benchmark]
        public GuidInfo GetGuidInfoMsSql() => s_v8.GetGuidInfo(GuidVersion8Type.MsSql);

        [Benchmark]
        public Guid ConvertVersion1To6() => s_v1.ConvertVersion1To6();

        [Benchmark]
        public Guid ConvertVersion6To1() => s_v6.ConvertVersion6To1();

        [Benchmark]
        public Guid ConvertVersion7To8MsSql() => s_v7.ConvertVersion7To8MsSql();

        [Benchmark]
        public Guid ConvertVersion8MsSqlTo7() => s_v8.ConvertVersion8MsSqlTo7();

        [Benchmark]
        public Guid CreateXorGuid() => s_base.CreateXorGuid(Guid.NewGuid());

        [Benchmark]
        public Guid CreateNumericGuid() => GuidKit.CreateNumericGuid(42);

        [Benchmark]
        public int ReverseNumericGuid() => GuidKit.CreateNumericGuid(42).ReverseNumericGuid();

        [Benchmark]
        public Guid FromHexString() => GuidKit.FromHexString("0x320C3D4DCC00875B8EC932D5F69181C0", bigEndian: true);

        [Benchmark]
        public string ToHexString() => s_v7.ToHexString(bigEndian: true);

        [Benchmark]
        public (GuidVariant Variant, byte? Version) GetVariantAndVersion() => s_v7.GetVariantAndVersion();
    }

    internal class Program
    {
        static void Main()
        {
            var summary = BenchmarkRunner.Run<GuidBenchmarks>();
        }
    }
}
