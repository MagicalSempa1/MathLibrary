using System.Numerics;
using System.Diagnostics;
using BenchmarkDotNet.Attributes;
using MathLibrary.EllipticCurves;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using MathLibrary.Matrices;
using Microsoft.VisualBasic;
using BenchmarkDotNet.Mathematics;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using System.Collections;
using System.Collections.Generic;
using MathLibrary.Factorization;
using MathLibrary;

[MemoryDiagnoser]
public class Benc
{
    //MatrixNxN<double> matrix = new MatrixNxN<double>(500);
    readonly BigInteger n;
    //readonly BigInteger f1;
    //readonly BigInteger f2;

    public Benc()
    {
        n = (BigInteger)1125899839733759 * 4398042316799;
        Console.WriteLine(n.GetBitLength());
        //f1 = Sequences.Fib(n);
        //f2 = Sequences.Fib(n + 1);
    }
    //public static BigInteger n = (BigInteger)1125899839733759 * 18014398241046527; //(BigInteger)1125899839733759 * 4398042316799;//BigInteger.Parse("20282408092494394779761211604993");
    //BigInteger n = (BigInteger)1125899839733759 * 4398042316799;//(BigInteger)16769023 * 4398042316799;
    //BigInteger n = (BigInteger)1125899839733759 * 2305843009213693951;
    //public static BigInteger n = BigInteger.Parse("5848646497896863985654683840657257485865965846839687893645988398657842867537637496597847534976753974269537749673772737195743767684824875673742375967374274328538775724723847537742854737547273756757542835776752375757657427673524567876654265754364788907978645467809876543466457689809654433455656677687898890987766565544543232311212123113133324345454667766768889988776554839585689823847593687342991949295066058342896543254675869708765432536475869776534356789087654367890876543456789086756454356789087653456789087654342536475869707876543567869876545678908765435678765435678765456778908765431342536475869675645345678905745785439287455747546675738672783765773475657385356874672719143479134473567947674966200235860884825668528555485936860485473280180012308485486853584865305485856384687384028688345856853482085860384687553248596842380457348675838458680240201834845567868578550860345885684385602804208274557568438458468240854876780420583067024431041044572854945739245647623308101846702545868483680380205080838745806752486857241454670802785655024575650249965545569784659522464545895454567672375686832576979742767427746775939557697989949567760300422647595674548954868593684539465738583693683589638466839589284663698588569468497498759648563752719775497149576427567452964715485439658695302205848674439946627711975946749608369637937294773699375734693742945769372719757477272467697397276394729132717357471590004772277571745672917475947295729175469146727476727274172971476198496847494636894688936372917575567373357632957678458693757762974567294767624674276862945672578627558729696467943896798346753896749534860341864968258436936756849679639640369357594972394067055985654986464975439678939468595702305830356834360345762837658374595648394655645868496476945876457456849586793765969078495385036834764539659769464657655763857683535");
    //string text = ".Р.ВЦТУ ЮИ?ЫЫ ГАГЕ-ЬЬАФ Й ВАХ,СЧ?ОСУКТЭБЙ ГЕФОЬНЙ ЬЕЯЬСР ЖЬЕЯИР,КИСХ ЧЕ .ОЯЕЭАДЬКТЭБЦ ГАЮЫЖ ЭУИШЪХСВ.ЕИА?ЛЭНЪЙСВКЭДО? АС БЭНЯЫ, ХЕ-ЬЛ Р ЯАЬЕИСН,КЧДОКЭДО? ЫОХ ЩУХЕД .О?ОЯ  Д РАВЖЯУ !ЧШС?ЬР,КЛПБУИСИКУГПЦХШ ХЛР ?ЕЩЯЛ ДЫК-СОХИ- ЪЗСССММХКДАБ,ЫЖ Ъ ЕМЯЫЖ ЭЮЬЕЫ,СК ТАРЁХСЯКЗ-АИ.СТУОР ЗНЭРФИР,КТЪОЪ ЙУ?К. Ъ ДВ ЯСУЭЫЩКС ЪСЦГЬАКПВИЯОГЯ? ВАХОГТЖ . УДАХЯОЪЕЯИЭ ?Е:,ККДОКТЭБЙ АК,УЯАЦТТ Й ЩЛСГАДСРЭНКЗШ ?ОУ ВТА ?ЫСМ ЙСД,УЫ Ъ ИТ  ДЫКВГЕФДШ ФОДОУ БОХДЭРШАДЬКМЭНЙ.СЯКНШДЦЮГЬМ ИТ  ОТ ТСДЦНН ,ОЯДЦН.ЯКБЕДЦТСНСЧШЛ МСН ВАЙКГ?АУЫСВКТЪОЦЙСЖЪЗ-ИМ БОЭНАЙКДЕХ В-ОФОСИКЛ.ЧЯОГТЯОЫОКРАС?АТ Й АЧЦНН ?ЕЩЯКЛПБЭЮСИКЖЭЛСЮУ ВТАБЁ !АШДМЙКДЭНЖ ЩЫЭ -А.О?НЦНСЛИБАВЖЮСИКУЪАШЕ-ИЦМУ С !АШДМЙКШШГКБМЛКНШП,АЪЛЦНСККТЪОЪМСМЦЧДАЮ . БЕ?ЯЮ.СП:СДЬКТЪОЙ ЯИЩНН ТУЬЕ? БОЭНШ ?ЕБЛС,ССУЕДАКИСД БВАМ . .УГТЖ ДЫКВГЕФДШ ТУЬЕГЬСЗЯАДЬМ ИТ  ДВ ИСД,У ЬЙ . !ОБА,Т.ЙБЫСЛИБРТКИСЦЦНРТКТЭБЙ.СДЭЯСП,ОЬОЭЖЭНЪЯСРССЙИ-РАВЬИСЭ?ОЫОКСАОТЩЭНЪЯСЗСЙЬИКНШ !А,ТКСШШС-БРЪВЭТЛРЁ!КЕКЕКРШЗКССДЯЕ: ,ОЯДЦН.ЯМ ГАГАФ ! ЕВСЖЭНЪЕ:,КТЪОЫ !ОЭЛЭГС,СБЦЛМЙКЛ.БЦРДА,ИШНЦЦТ";
    //[Benchmark(Description = "Bernstein-Yang Jacobi, batch = 256", Baseline = true)]
    //public int Test1() => ArithmeticFunctions.BYJacobiSymbol(f1, f2, 256);

    //[Benchmark(Description = "Bernstein-Yang Jacobi, batch = 512")]
    //public int Test2() => ArithmeticFunctions.BYJacobiSymbol(f1, f2, 512);

    //[Benchmark(Description = "Bernstein-Yang Jacobi, batch = 1024")]
    //public int Test3() => ArithmeticFunctions.BYJacobiSymbol(f1, f2, 1024);

    //[Benchmark(Description = "Bernstein-Yang Jacobi, batch = 2048")]
    //public int Test4() => ArithmeticFunctions.BYJacobiSymbol(f1, f2, 2048);

    [Benchmark(Description = "SQUFOF")]
    public BigInteger[] Test1() => Factorization.SQUFOFMethod(n);

    //[Benchmark(Description = "B-Y Jacobi")]
    //public int Test2() => ArithmeticFunctions.BYJacobiSymbol(f1, f2, 512);

}

class Program
{
    static void Main()
    {
        //Console.WriteLine($"log10_n = {(int)Math.Ceiling(BigInteger.Log10(Benc.n))}");
        //Console.WriteLine($"log2_n = {(int)Math.Ceiling(BigInteger.Log(Benc.n, 2))}");
        BenchmarkRunner.Run<Benc>();
        //BigInteger n = 1_500_000;
        //BigInteger f1 = Sequences.Fib(n);
        //BigInteger f2 = Sequences.Fib(n + 1);
        //var r = ArithmeticFunctions.BYJacobiSymbol(f1, f2, 512);
        //Console.WriteLine(r);
        //r = ArithmeticFunctions.BYJacobiSymbol1(f1, f2, 512);
        //Console.WriteLine(r);
        //Console.WriteLine(f2.IsEven);
        //Console.WriteLine(f1.GetBitLength());
        //Console.WriteLine(f2.GetBitLength());
        //BigInteger number = BigInteger.Parse("12345678901234567890");
        //var p = Sieves.MyAtkinSieve(10_000_000);
        //Console.WriteLine(p.Length);
        //        for (int x = 0; x < 420; x++)
        //        {
        //            for (int y = 0; y < 420; y++)
        //            {
        //                if (rems1.Contains(4 * x * x + y * y))
        //                    Console.WriteLine($"{x} {y}");
        //            }
        //        }
        //BigInteger squareRoot = KaratsubaSquareRootAlgorithm(number);
        //Console.WriteLine($"Square root of {number} is {squareRoot}");
        //Console.WriteLine($"Square root of {number} is {number.FloorSqrt()}");
        //BitArray[] matrix = new BitArray[10];
        //Random random = new Random();
        //for (int i = 0; i < 10; i++)
        //{
        //    matrix[i] = new BitArray(10);
        //    for (int j = 0; j < 10; j++)
        //    {
        //        matrix[i][j] = random.Next(2) == 1;
        //    }
        //}

        //// Случайно сгенерированный вектор правой части
        //BitArray vector = new BitArray(10);
        //for (int i = 0; i < 10; i++)
        //{
        //    vector[i] = random.Next(2) == 1;
        //}

        //Console.WriteLine("Матрица коэффициентов:");
        //for (int i = 0; i < matrix.Length; i++)
        //{
        //    for (int j = 0; j < matrix[i].Length; j++)
        //    {
        //        Console.Write(matrix[i][j] ? "1 " : "0 ");
        //    }
        //    Console.WriteLine();
        //}

        //Console.WriteLine("Вектор правой части:");
        //for (int i = 0; i < vector.Length; i++)
        //{
        //    Console.WriteLine("b{0} = {1}", i, vector[i] ? 1 : 0);
        //}

        //// Решение СЛАУ
        //BitArray solution = GaussZ2.Solve(matrix, vector);

        //// Выводим результат


        //Console.WriteLine("Решение СЛАУ:");
        //for (int i = 0; i < solution.Length; i++)
        //{
        //    Console.WriteLine("x{0} = {1}", i, solution[i] ? 1 : 0);
        //}
        //var n = (BigInteger)16769023 * 4398042316799;
        //var r = DixonFactorization.Factorize(n);
        //for (int i = 0; i < r.Count; i++)
        //{
        //    Console.WriteLine(r[i]);
        //}
        //var array = new int[][]
        //{
        //    new int[] {2,0,0},
        //    new int[] {0,1,1},
        //    new int[] {0,0,1},
        //};
        //var matrix = new MatrixNxN<int>(array);
        //var blocks = matrix.GetBlocks();
        //Console.WriteLine(blocks.Item1.ToString());
        //Console.WriteLine(blocks.Item2.ToString());
        //Console.WriteLine(blocks.Item3.ToString());
        //Console.WriteLine(blocks.Item4.ToString());
        //BigInteger n = (BigInteger)1125899839733759 * 18014398241046527;
        //var r = Factorization.LehmanMethod(n);
        //Console.WriteLine(r[0]);
        //Console.WriteLine(r[1]);
        //BigInteger n = (BigInteger)1125899839733759 * 4398042316799;//(BigInteger)1125899839733759 * 2305843009213693951;
        //Console.WriteLine(BigInteger.Log10(n));
        //var watch = new Stopwatch();
        //watch.Start();
        //var r = Factorization.SQUFOFMethod(n);
        //watch.Stop();
        //Console.WriteLine(watch.Elapsed.TotalMinutes);

        //int n;
        //while (true)
        //{
        //    n = int.Parse(Console.ReadLine());
        //    Console.WriteLine(n % 4);
        //    Console.WriteLine(n & 3);
        //}
        //UInt128 n1 = (UInt128)1125899839733759 * 2305843009213693951;
        //BigInteger n2 = (BigInteger)1125899839733759 * 2305843009213693951;
        //var watch = new Stopwatch();
        //watch.Start();
        //var r = Factorization.SQUFOFMethod(n1);
        //watch.Stop();
        //Console.WriteLine(watch.Elapsed.TotalMinutes);
        //watch.Restart();
        //var m = Factorization.SQUFOFMethod(n2);
        //watch.Stop();
        //Console.WriteLine(watch.Elapsed.TotalMinutes);
        //var s = Sequences.Fib1(10_000_000);
        //Console.WriteLine(Sequences.Fib(1000));
        //Console.WriteLine(Sequences.Fib1(1000));
        Console.ReadKey();
    }

    public static BigInteger KaratsubaSquareRootAlgorithm(BigInteger n)
    {
        if (n < uint.MaxValue)
            return (BigInteger)Math.Sqrt((uint)n);
        // Split n into four parts of k bits each
        int k = (int)n.GetBitLength() >> 2;
        BigInteger b = 1 << k;
        BigInteger a3 = n >> (3 * k);
        BigInteger a2 = (n >> (k << 1)) & (b - 1);
        BigInteger a1 = (n >> k) & (b - 1);
        BigInteger a0 = n & (b - 1);

        // Normalize a3
        if ((a3 & (b >> 1)) == 0)
        {
            a3 <<= 1;
            k++;
        }

        // Square root of the high two parts
        BigInteger s1 = KaratsubaSquareRootAlgorithm(a3 * b + a2);
        BigInteger r1 = a3 * b + a2 - s1 * s1;

        // Approximation to the desired root
        BigInteger s = s1 * b;
        BigInteger q, u;

        // Recursive application of the algorithm
        while (true)
        {
            q = BigInteger.DivRem(r1 * b + a1, 2 * s1, out u);
            s += q;
            BigInteger r = u * b + a0 - q * q;

            // Check for normalization
            if (r < 0)
            {
                r += 2 * s - 1;
                s--;
            }

            if (r >= 0)
                return s;
        }
    }
}



