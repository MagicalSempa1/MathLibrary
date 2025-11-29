using MathLibrary.Factorization.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary.Factorization.PolynomialSource
{
    public interface IPolynomialSource
    {
        void Reset(MpqsContext ctx);
        bool TryNext(MpqsContext ctx, out QSPolynomial poly);
        int BlocksPerPolynomial { get; }
    }

    public interface ISiqsPolynomialSource
    {
        /// <summary>Сколько блоков решета отрабатываем на одном полиноме.</summary>
        int BlocksPerPolynomial { get; }

        /// <summary>Текущий набор B_v для семьи полиномов (используется ситом).</summary>
        BigInteger[] CurrentBTerms { get; }

        /// <summary>Инициализировать новую семью полиномов (новое A, новый набор B_v).</summary>
        void Reset(MpqsContext ctx);

        /// <summary>
        /// Сгенерировать следующий полином.
        /// flippedIndex = -1 означает "новая семья" (первый полином),
        /// flipSign = 0 — нет дельты (точка входа для новой семьи).
        /// </summary>
        bool TryNext(
            MpqsContext ctx,
            out QSPolynomial poly,
            out int flippedIndex,
            out int flipSign);
    }
}
