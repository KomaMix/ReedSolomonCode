using ReedSolomonCode.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonCode.Tests
{
    public class GaloisFieldTests
    {
        [Fact]
        public void Add_Test()
        {
            // Сложение в поле Галуа эквивалентно побитовому XOR
            Assert.Equal(0xD4, GaloisField.Add(0x57, 0x83));
            Assert.Equal(0x00, GaloisField.Add(0xAA, 0xAA)); // XOR с самим собой дает 0
            Assert.Equal(0xFF, GaloisField.Add(0x00, 0xFF)); // XOR с нулем возвращает исходное число
        }

        [Fact]
        public void Multiply_Test()
        {
            // Проверка умножения в поле Галуа
            Assert.Equal(0x5d, GaloisField.Multiply(0x50, 0x04));
            Assert.Equal(0x1d, GaloisField.Multiply(0x74, 0x47));
            Assert.Equal(0x47, GaloisField.Multiply(0x8e, 0x8e));
        }

        [Fact]
        public void Divide_Test()
        {
            // Проверка деления в поле Галуа
            Assert.Equal(0x47, GaloisField.Divide(0xad, 0x8e));
            Assert.Equal(0x04, GaloisField.Divide(0x83, 0xe9));
        }

        [Fact]
        public void Inverse_Test()
        {
            // Проверка нахождения обратного элемента
            byte inverse = GaloisField.Inverse(0x08);

            Assert.Equal(0xad, inverse);
        }

        [Fact]
        public void Multiply_And_Divide_Are_Inverses()
        {
            // Проверка, что умножение и деление являются обратными операциями
            byte a = 0x57;
            byte b = 0x83;

            byte product = GaloisField.Multiply(a, b);
            byte quotient = GaloisField.Divide(product, b);

            Assert.Equal(a, quotient); // a * b / b == a
        }
    }
}
