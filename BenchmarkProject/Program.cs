using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Nethermind.Int256;
using System.Numerics;

namespace BenchmarkProject
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var config = ManualConfig.Create(DefaultConfig.Instance).WithOptions(ConfigOptions.JoinSummary | ConfigOptions.DisableLogFile);

            BenchmarkRunner.Run
            (
                [
                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt32Generator, uint>), config),

                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt64Generator, ulong>), config),

                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<DoubleGenerator, double>), config),

                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<DecimalGenerator, decimal>), config),

                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt128DivideByUInt64Generator, UInt128>), config),
                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt128Generator, UInt128>), config),

                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt256DivideByUInt64Generator, MyUInt256>), config),
                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt256DivideByUInt128Generator, MyUInt256>), config),
                    BenchmarkConverter.TypeToBenchmarks(typeof(DivisionBenchmark<UInt256Generator, MyUInt256>), config)
                ]
            );
        }
    }
    public readonly struct MyUInt256 : IDivisionOperators<MyUInt256, MyUInt256, MyUInt256>
    {
        private readonly UInt256 value;

        public MyUInt256(ulong uppest, ulong upper, ulong lower, ulong lowest)
            => value = new(u3: uppest, u2: upper, u1: lower, u0: lowest);

        private MyUInt256(UInt256 value)
            => this.value = value;

        public static MyUInt256 operator /(MyUInt256 left, MyUInt256 right)
            => new(value: left.value / right.value);
    }

    public static class ExtensionMethods
    {
        public static uint NextUInt32(this Random random)
            => (uint)random.Next();

        public static ulong NextUInt64(this Random random)
            => (ulong)random.NextInt64();
    }

    public readonly struct UInt32Generator : INumberGenerator<uint>
    {
        public static uint GenerateDividend(Random random)
            => random.NextUInt32();

        public static uint GenerateDivisor(Random random)
            => (uint)random.Next(maxValue: 1 << 16);
    }

    public readonly struct UInt64Generator : INumberGenerator<ulong>
    {
        public static ulong GenerateDividend(Random random)
            => random.NextUInt64();

        public static ulong GenerateDivisor(Random random)
            => random.NextUInt32();
    }

    public readonly struct DoubleGenerator : INumberGenerator<double>
    {
        public static double GenerateDividend(Random random)
            => random.NextUInt64();

        public static double GenerateDivisor(Random random)
            => random.NextUInt32();
    }

    public readonly struct DecimalGenerator : INumberGenerator<decimal>
    {
        public static decimal GenerateDividend(Random random)
            => (decimal)random.NextUInt64() * random.NextUInt32();

        public static decimal GenerateDivisor(Random random)
            => random.NextUInt64();
    }

    public readonly struct UInt128DivideByUInt64Generator : INumberGenerator<UInt128>
    {
        public static UInt128 GenerateDividend(Random random)
            => new(upper: random.NextUInt64(), lower: random.NextUInt64());

        public static UInt128 GenerateDivisor(Random random)
            => new(upper: 0, lower: random.NextUInt64());
    }

    public readonly struct UInt128Generator : INumberGenerator<UInt128>
    {
        public static UInt128 GenerateDividend(Random random)
            => new(upper: random.NextUInt64(), lower: random.NextUInt64());

        public static UInt128 GenerateDivisor(Random random)
            => new(upper: random.NextUInt32(), lower: random.NextUInt64());
    }

    public readonly struct UInt256DivideByUInt64Generator : INumberGenerator<MyUInt256>
    {
        public static MyUInt256 GenerateDividend(Random random)
            => new(uppest: random.NextUInt64(), upper: random.NextUInt64(), lower: random.NextUInt64(), lowest: random.NextUInt64());

        public static MyUInt256 GenerateDivisor(Random random)
            => new(uppest: 0, upper: 0, lower: 0, lowest: random.NextUInt64());
    }

    public readonly struct UInt256DivideByUInt128Generator : INumberGenerator<MyUInt256>
    {
        public static MyUInt256 GenerateDividend(Random random)
            => new(uppest: random.NextUInt64(), upper: random.NextUInt64(), lower: random.NextUInt64(), lowest: random.NextUInt64());

        public static MyUInt256 GenerateDivisor(Random random)
            => new(uppest: 0, upper: 0, lower: random.NextUInt64(), lowest: random.NextUInt64());
    }

    public readonly struct UInt256Generator : INumberGenerator<MyUInt256>
    {
        public static MyUInt256 GenerateDividend(Random random)
            => new(uppest: random.NextUInt64(), upper: random.NextUInt64(), lower: random.NextUInt64(), lowest: random.NextUInt64());

        public static MyUInt256 GenerateDivisor(Random random)
            => new(uppest: 0, upper: random.NextUInt64(), lower: random.NextUInt64(), lowest: random.NextUInt64());
    }

    public interface INumberGenerator<TNum>
    {
        static abstract TNum GenerateDividend(Random random);

        static abstract TNum GenerateDivisor(Random random);
    }

    // Some good advice on designing benchmarks https://stackoverflow.com/a/76107209
    [ShortRunJob]
    public class DivisionBenchmark<TNumberGenerator, TNum>
        where TNumberGenerator : struct, INumberGenerator<TNum>
        where TNum : struct, IDivisionOperators<TNum, TNum, TNum>
    {
        private static readonly Random random = new(Seed: 100);

        public TNum
            dividend = TNumberGenerator.GenerateDividend(random: random),
            divisor = TNumberGenerator.GenerateDivisor(random: random);

        [Benchmark]
        public TNum Division()
            => dividend / divisor;
    }
}