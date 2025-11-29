using System.Diagnostics;
using System.Numerics;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Disassemblers;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using MathLibrary;
using MathLibrary.Factorization;
using MathLibrary.Factorization.BlockSieves;
using MathLibrary.Factorization.DependencySolver;
using MathLibrary.Factorization.PolynomialSource;
using MathLibrary.Factorization.RelationManager;
using MathLibrary.Factorization.RelationSchedulers;
using MathLibrary.Factorization.SmoothCheckers;
using MathLibrary.Factorization.Types;



//var n = BigInteger.Parse("25195908475657893494027183240048398571429282126204032027777137836043662020707595556264018525880784406918290641249515082189298559149176184502808489120072844992687392807287776735971418347270261896375014971824691165077613379859095700097330459748808428401797429100642458691817195118746121515172654632282216869987549182422433637259085141865462043576798423387184774447920739934236584823824281198163815010674810451660377306056201619676256133844143603833904414952634432190114657544454178424020924616515723350778707749817125772467962926386356373289912154831438167899885040445364023527381951378636564391212010397122822120720357");
//BenchmarkRunner.Run<Benc>();
//Console.WriteLine(SemiPrimeGenerator.RandomSemiprimes(40, 100).Where(x => x.ToString()[0] == '5').First());
//var n = SemiPrimeGenerator.RandomSemiprimes(40, 1, seed: 123456 ^ 40).First();
//var r = Factorization.SPQSMethod(n);
//var r = Factorization.SPQSMethod(
//                    n,
//                    SpqsOptions.Build(n) with { EnableLargePrimes = QSLPOpt.OneLP, LargePrimeBoundMultiplier = 1.2 },
//                    new BatchOneLargePrimeSmoothnessChecker(),
//                    new SpqsBlockSieveFactory(),
//                    new SpqsRollingScheduler(),
//                    new SpqsOneLpRelationManager(),
//                    new GaussianZ2SolverAdapter());
//Console.WriteLine($"{r[0] * r[1]}" + " = " + n);
//var n = SemiPrimeGenerator.RandomSemiprimes(40, 1, seed: 123456 ^ 40).First();
//var bsf = new SpqsBlockSieveFactory();
//var sw = new Stopwatch();
//Factorization.SPQSMethod(
//                    n,
//                    SpqsOptions.Build(n),
//                    new EarlyAbortSmoothnessChecker(),
//                    new SpqsBlockSieveFactory(),
//                    new SpqsRollingScheduler(),
//                    new SpqsRelationManager(),
//                    new GaussianZ2SolverAdapter());
BenchmarkRunner.Run<Benc>();
//BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
Console.ReadKey();

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
[SimpleJob(RuntimeMoniker.Net90, launchCount: 2, warmupCount: 5, iterationCount: 15)]
public class Benc
{
    private const int SeedBase = 123456;
    private const int count = 1;
    private const int digits = 40;

    [ParamsSource(nameof(DigitsSet))]
    public int Digits { get; set; }
    public IEnumerable<int> DigitsSet => [digits];

    [ParamsSource(nameof(BSet))]
    public int B { get; set; }
    public IEnumerable<int> BSet => [96_000];

    [ParamsSource(nameof(BLSet))]
    public int BL { get; set; }
    public IEnumerable<int> BLSet => [131072];

    //[ParamsSource(nameof(CUT2Set))]
    //public int CUT2 { get; set; }
    //public IEnumerable<int> CUT2Set => [4096, 8192];

    //[ParamsSource(nameof(BPPSet))]
    //public int BPP { get; set; }
    //public IEnumerable<int> BPPSet => [8];

    //[ParamsSource(nameof(MAXSet))]
    //public int MAX { get; set; }
    //public IEnumerable<int> MAXSet => [28];

    private static BigInteger[] nums = SemiPrimeGenerator.RandomSemiprimes(digits, count);//BigInteger.Parse("5482469910660087512964170267228149643533");//BigInteger.Parse("583634904710641006823504050536673");

    [GlobalSetup]
    public void GlobalSetup()
    {
        int seed = SeedBase ^ Digits;
        nums = SemiPrimeGenerator.RandomSemiprimes(Digits, count, seed: seed);
        try
        {
            BigInteger warm = SemiPrimeGenerator.RandomSemiprime(Digits, smoothnessBound: SemiPrimeGenerator.RecommendSmoothnessBound(Digits));
            var res = Factorization.SPQSMethod(warm);
        }
        catch { /* прогрев best-effort */ }
    }

    [Benchmark(Description = "SPQS")]
    public BigInteger[] QS()
    {
        var results = new List<BigInteger>(nums.Length << 1);
        foreach (var n in nums)
            results.AddRange(
                Factorization.SPQSMethod(
                    n,
                    SpqsOptions.Build(n) with { B = B, BlockLen = BL },
                    new EarlyAbortSmoothnessChecker(),
                    new SpqsBlockSieveFactory(),
                    new SpqsRollingScheduler(),
                    new SpqsRelationManager(),
                    new GaussianZ2SolverAdapter()));
        return [.. results];
    }
}