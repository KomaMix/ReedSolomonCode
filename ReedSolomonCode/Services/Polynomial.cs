using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReedSolomonCode.Helpers;

namespace ReedSolomonCode.Services
{
    public class Polynomial : IEquatable<Polynomial>
    {
        public byte[] Coefficients { get; private set; }

        // Конструктор принимает массив коэффициентов (начиная с x^0)
        public Polynomial(params byte[] coefficients)
        {
            // Убираем нулевые старшие коэффициенты
            this.Coefficients = coefficients.Reverse().SkipWhile(c => c == 0).Reverse().ToArray();
        }

        // Степень полинома
        public int Degree => Coefficients.Length - 1;

        // Индексатор для доступа к коэффициентам (по возрастанию степеней)
        public byte this[int degree]
        {
            get => degree < Coefficients.Length ? Coefficients[degree] : (byte)0;
            set
            {
                //if (degree >= Coefficients.Length)
                //{
                //    // Увеличиваем массив до нужного размера
                //    Array.Resize(ref Coefficients, degree + 1);
                //}
                Coefficients[degree] = value;
            }
        }

        // Сложение двух полиномов
        public static Polynomial operator +(Polynomial p1, Polynomial p2)
        {
            int maxDegree = Math.Max(p1.Degree, p2.Degree);
            byte[] result = new byte[maxDegree + 1];

            for (int i = 0; i <= maxDegree; i++)
            {
                result[i] = GaloisField.Add(p1[i], p2[i]);
            }

            return new Polynomial(result);
        }

        // Умножение двух полиномов
        public static Polynomial operator *(Polynomial p1, Polynomial p2)
        {
            byte[] result = new byte[p1.Degree + p2.Degree + 1];

            for (int i = 0; i <= p1.Degree; i++)
            {
                for (int j = 0; j <= p2.Degree; j++)
                {
                    result[i + j] = GaloisField.Add(result[i + j],
                        GaloisField.Multiply(p1[i], p2[j]));
                }
            }

            return new Polynomial(result);
        }

        // Деление полиномов: возвращает (частное, остаток)
        public static (Polynomial Quotient, Polynomial Remainder) Divide(Polynomial dividend, Polynomial divisor)
        {
            if (divisor.Degree < 0)
                throw new DivideByZeroException("Деление на нулевой полином.");

            byte[] quotient = new byte[dividend.Degree - divisor.Degree + 1];
            Polynomial remainder = new Polynomial(dividend.Coefficients);

            while (remainder.Degree >= divisor.Degree)
            {
                int degreeDiff = remainder.Degree - divisor.Degree;
                byte scale = GaloisField.Divide(remainder[remainder.Degree], divisor[divisor.Degree]);

                quotient[degreeDiff] = scale;

                byte[] scaledDivisor = new byte[degreeDiff + divisor.Coefficients.Length];
                for (int i = 0; i < divisor.Coefficients.Length; i++)
                {
                    scaledDivisor[degreeDiff + i] = GaloisField.Multiply(divisor[i], scale);
                }

                remainder = remainder + new Polynomial(scaledDivisor);
            }

            return (new Polynomial(quotient), remainder);
        }

        public Polynomial Derivative()
        {
            if (Degree == 0) // Производная константы — нулевой полином
                return new Polynomial(0);

            byte[] result = new byte[Degree]; // Массив для новой степени (на одну меньше)

            for (int i = 1; i < Coefficients.Length; i++)
            {
                if (i % 2 == 1) // Только нечетные степени
                {
                    result[i - 1] = Coefficients[i];
                }
            }

            return new Polynomial(result);
        }

        public static Polynomial DiscardHigherDegrees(Polynomial polynomial, int maxDegree)
        {
            if (polynomial.Degree <= maxDegree)
                return polynomial;

            byte[] truncatedCoefficients = new byte[maxDegree + 1];
            Array.Copy(polynomial.Coefficients, 0, truncatedCoefficients, 0, maxDegree + 1);

            return new Polynomial(truncatedCoefficients);
        }

        // Вычисление значения полинома в точке x
        public byte Evaluate(byte x)
        {
            byte result = 0;
            byte power = 1;

            foreach (byte coef in Coefficients)
            {
                var t = GaloisField.Multiply(coef, power);

                result = GaloisField.Add(result, t);
                power = GaloisField.Multiply(power, x);
            }

            return result;
        }

        public override bool Equals(object obj)
        {
            return obj is Polynomial polynomial && Equals(polynomial);
        }

        public bool Equals(Polynomial other)
        {
            if (other == null) return false;

            // Сравнение коэффициентов
            if (Degree != other.Degree) return false;

            for (int i = 0; i <= Degree; i++)
            {
                if (this[i] != other[i]) return false;
            }

            return true;
        }

        // Переопределение GetHashCode
        public override int GetHashCode()
        {
            // Используем хеш-сумму коэффициентов
            return Coefficients.Aggregate(17, (hash, coef) => hash * 31 + coef);
        }

        // Строковое представление полинома
        public override string ToString()
        {
            if (Coefficients.Length == 0)
                return "0";

            List<string> terms = new List<string>();

            for (int i = 0; i < Coefficients.Length; i++)
            {
                byte coef = Coefficients[i];
                if (coef == 0) continue;

                string term = coef.ToString("X2");
                if (i > 0) term += $"x^{i}";
                terms.Add(term);
            }

            return string.Join(" + ", terms);
        }
    }
}
