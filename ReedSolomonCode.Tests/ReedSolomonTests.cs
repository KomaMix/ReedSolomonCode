using ReedSolomonCode.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReedSolomonCode.Tests
{
    public class ReedSolomonTests
    {
        [Fact]
        public void RedSolomonGenerator_Test()
        {
            var g = new Polynomial(0x02, 0x01) * new Polynomial(0x04, 0x01)
                * new Polynomial(0x08, 0x01) * new Polynomial(0x10, 0x01);

            g *= new Polynomial(0x00, 0x00, 0x00, 0x00, 0x01);


            Assert.Equal(new Polynomial(0, 0, 0, 0, 116, 231, 216, 30, 1), g);
        }


        [Fact]
        public void Encode_CorrectCode_Test()
        {
            var rs = new ReedSolomonCodeService(4);
            var message = new byte[] { 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 };
            var encoded = rs.Encode(message);


            byte[] expectedResult = { 0xdb, 0x22, 0x58, 0x5c, 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 };
            Assert.Equal(expectedResult, encoded);
        }

        [Fact]
        public void Decode_CorrectDecodeForCorrectReedSolomonCode_Test()
        {
            var rs = new ReedSolomonCodeService(4);
            byte[] encodedMessage = { 0xdb, 0x22, 0x58, 0x5c, 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 };

            var decoded = rs.Decode(encodedMessage);

            byte[] originalMessage = { 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 };

            Assert.Equal(originalMessage, decoded);
        }

        [Fact]
        public void Decode_WithErrors_Test()
        {
            var rs = new ReedSolomonCodeService(4);
            byte[] encodedMessageWithError = { 0xdb, 0x22, 0x58, 0x5c, 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x42 };

            // Добавление ошибок в сообщение
            encodedMessageWithError[encodedMessageWithError.Length - 1] = 0x42;

            // Декодирование
            var decoded = rs.Decode(encodedMessageWithError);

            Assert.Equal(new byte[] { 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 }, decoded); // Ошибки должны быть исправлены
        }

        [Fact]
        public void Decode_WithTwoErrors_Test()
        {
            var rs = new ReedSolomonCodeService(4);
            byte[] encodedMessageWithError = { 0xdb, 0x22, 0x58, 0x5c, 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x42 };

            // Добавление ошибок в сообщение
            encodedMessageWithError[encodedMessageWithError.Length - 1] = 0x42;
            encodedMessageWithError[encodedMessageWithError.Length - 3] = 0x00;

            // Декодирование
            var decoded = rs.Decode(encodedMessageWithError);

            Assert.Equal(new byte[] { 0x44, 0x4f, 0x4e, 0x27, 0x54, 0x20, 0x50, 0x41, 0x4e, 0x49, 0x43 }, decoded); // Ошибки должны быть исправлены
        }


        [Fact]
        public void Decode_WithErrorPositions_Test()
        {
            var rs = new ReedSolomonCodeService(4);
            var message = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            var encoded = rs.Encode(message);

            // Добавляем ошибки
            encoded[1] ^= 0xFF;
            encoded[5] ^= 0xAA;

            // Декодирование
            var decoded = rs.Decode(encoded);

            Assert.Equal(message, decoded); // Проверяем, что сообщение восстановлено
        }
    }
}
