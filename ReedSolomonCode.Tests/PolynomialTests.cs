using ReedSolomonCode.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonCode.Tests
{
    public class PolynomialTests
    {
        [Fact]
        public void Add_Polynomials_Test()
        {
            var p1 = new Polynomial(0x03, 0x08, 0x0b, 0x07);
            var p2 = new Polynomial(0x13, 0x00, 0x06, 0x00);

            var result = p1 + p2;

            Assert.Equal(new Polynomial(0x10, 0x08, 0x0d, 0x07), result);
        }

        [Fact]
        public void Multiply_Polynomials_Test()
        {
            var p1 = new Polynomial(0x08, 0x09, 0x02);
            var p2 = new Polynomial(0x06, 0x0b);

            var result = p1 * p2;

            Assert.Equal(new Polynomial(0x30, 0x6e, 0x5f, 0x16), result);
        }

        [Fact]
        public void Divide_Polynomials_Test()
        {
            var dividend = new Polynomial(0x43, 0x56, 0x88, 0x44);
            var divisor = new Polynomial(0x06, 0x0b, 0x07);

            var (quotient, remainder) = Polynomial.Divide(dividend, divisor);

            Assert.Equal(new Polynomial(0xe8, 0x73), quotient);
            Assert.Equal(new Polynomial(0x09, 0x57), remainder);
        }

        [Fact]
        public void Evaluate_Polynomial_Test()
        {
            var p = new Polynomial(0x01, 0x57);

            var result = p.Evaluate(0x02);

            Assert.Equal(0xAF, result);
        }

        [Fact]
        public void Derivative_Polynomial_Test()
        {
            var p = new Polynomial(1, 45, 165, 198, 140, 223);

            var result = p.Derivative();

            Assert.Equal(result, new Polynomial(45, 0, 198, 0, 223));
        }

        [Fact]
        public void Evaluate_ConstantPolynomial_Test()
        {
            var polynomial = new Polynomial(7, 12, 3);
            byte result = polynomial.Evaluate(4);
            Assert.Equal(7, result);
        }
    }
}
