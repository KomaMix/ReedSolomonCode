using ReedSolomonCode.Helpers;

namespace ReedSolomonCode.Services
{
    public class ReedSolomonCodeService
    {
        private readonly int numParitySymbols; // Количество контрольных символов
        private readonly Polynomial generator; // Порождающий полином

        public ReedSolomonCodeService(int numParitySymbols)
        {
            if (numParitySymbols < 1)
            {
                throw new ArgumentException("Количество контрольных символов должно быть больше 0.");
            }

            this.numParitySymbols = numParitySymbols;
            this.generator = GenerateGeneratorPolynomial(numParitySymbols);
        }

        // Генерация порождающего полинома
        private Polynomial GenerateGeneratorPolynomial(int degree)
        {
            Polynomial generator = new Polynomial(1); // Изначально 1

            for (int i = 1; i < degree + 1; i++)
            {
                // generator *= (x + α^i)
                generator *= new Polynomial(GaloisField.Exp(i), 1);
            }

            return generator;
        }

        // Кодирование сообщения
        public byte[] Encode(byte[] message)
        {
            if (message == null || message.Length == 0)
            {
                throw new ArgumentException("Сообщение не может быть пустым.");
            }

            // Полином сообщения
            Polynomial messagePolynomial = new Polynomial(message);

            // Умножаем на x^numParitySymbols
            byte[] shiftCoefficients = new byte[numParitySymbols + 1];
            shiftCoefficients[numParitySymbols] = 1; // Коэффициент при x^numParitySymbols
            Polynomial messageShifted = messagePolynomial * new Polynomial(shiftCoefficients);

            // Получаем остаток от деления на порождающий полином
            var (_, remainder) = Polynomial.Divide(messageShifted, generator);

            Polynomial encrypted = remainder + messageShifted;

            return encrypted.Coefficients;
        }

        // Раскодирование сообщения
        public byte[] Decode(byte[] received)
        {
            if (received == null || received.Length <= numParitySymbols)
            {
                throw new ArgumentException("Длина сообщения должна быть больше количества контрольных символов.");
            }

            // Преобразуем в полином
            Polynomial receivedPolynomial = new Polynomial(received);

            // Вычисляем синдромы
            byte[] syndromes = ComputeSyndromes(receivedPolynomial);

            // Если все синдромы равны 0, ошибок нет
            if (IsSyndromeZero(syndromes))
            {
                return ExtractMessage(received);
            }

            // Ищем локатор ошибок
            Polynomial errorLocator = FindErrorLocator(syndromes);
            var errorPositions = FindErrorPositions(errorLocator);

            // Исправляем ошибки
            var correctedMessage = CorrectErrors(receivedPolynomial, errorPositions, syndromes);

            // Возвращаем исправленное сообщение
            return ExtractMessage(correctedMessage.Coefficients);
        }

        // Вычисление синдромов
        private byte[] ComputeSyndromes(Polynomial received)
        {
            byte[] syndromes = new byte[numParitySymbols];
            for (int i = 0; i < numParitySymbols; i++)
            {
                syndromes[i] = received.Evaluate(GaloisField.Exp(i + 1));
            }
            return syndromes;
        }

        // Проверка, равны ли все синдромы 0
        private bool IsSyndromeZero(byte[] syndromes)
        {
            foreach (var syndrome in syndromes)
            {
                if (syndrome != 0) return false;
            }
            return true;
        }

        // Поиск локатора ошибок с использованием алгоритма Берхлекэмпа-Месси
        private Polynomial FindErrorLocator(byte[] syndromes)
        {
            Polynomial locator = new Polynomial(1); // Инициализируем локатор как 1 (полином нулевой степени)
            Polynomial locatorOld = new Polynomial(1); // Копия локатора для корректировок

            int syndromeShift = 0; // Смещение для синдромов

            for (int i = 0; i < numParitySymbols; i++)
            {
                int k = i + syndromeShift;
                byte delta = syndromes[k];

                // Вычисление дельты: скалярное произведение синдромов и локатора
                for (int j = 1; j < locator.Degree + 1; j++)
                {
                    delta ^= GaloisField.Multiply(locator[j], syndromes[k - j]);
                }

                // Сдвиг локатора на x
                locatorOld = locatorOld * new Polynomial(0, 1);

                if (delta != 0)
                {
                    if (locatorOld.Degree > locator.Degree)
                    {
                        // Сохраняем текущий локатор
                        Polynomial locatorNew = locatorOld * new Polynomial(delta);
                        locatorOld = locator * new Polynomial(GaloisField.Inverse(delta));
                        locator = locatorNew;
                    }

                    // Обновляем локатор
                    locator += locatorOld * new Polynomial(delta);
                }
            }

            return locator;
        }



        // Поиск позиций ошибок
        private List<int> FindErrorPositions(Polynomial errorLocator)
        {
            var errorPositions = new List<int>();

            // Проверяем все элементы поля Галуа
            for (int i = 0; i < 255; i++)
            {
                // Если значение полинома в точке α^i равно 0, добавляем позицию
                if (errorLocator.Evaluate(GaloisField.Exp(i)) == 0)
                {
                    errorPositions.Add(i); // Сохраняем индекс как есть
                }
            }

            // Преобразование в индексы для сообщения
            for (int j = 0; j < errorPositions.Count; j++)
            {
                errorPositions[j] = 255 - errorPositions[j];
                
                if (errorPositions[j] == 255)
                {
                    errorPositions[j] = 0;
                }
            }

            return errorPositions;
        }

        // Исправление ошибок
        private Polynomial CorrectErrors(Polynomial received, List<int> errorPositions, byte[] syndromes)
        {
            // Вычисляем полином локатора ошибок
            Polynomial errorLocator = FindErrorLocator(syndromes);

            // Умножаем синдромы на локатор ошибок
            Polynomial product = new Polynomial(syndromes) * errorLocator;

            // Оставляем младшие степени для вычисления полинома ошибок
            Polynomial errorPolynomial = Polynomial.DiscardHigherDegrees(product, numParitySymbols - 1);

            // Производная локатора ошибок
            Polynomial errorLocatorDerivative = errorLocator.Derivative();

            // Инициализируем массив амплитуд ошибок
            byte[] errorMagnitudes = new byte[received.Coefficients.Length];

            // Перебираем все позиции ошибок
            foreach (int position in errorPositions)
            {
                // Обратное значение для x = α^(255 - position)
                byte xiInverse = GaloisField.Exp(255 - position);

                // Значение полинома ошибок при x = xi
                byte errorValue = errorPolynomial.Evaluate(xiInverse);

                // Значение производной локатора ошибок при x = xi
                byte locatorDerivativeValue = errorLocatorDerivative.Evaluate(xiInverse);

                // Амплитуда ошибки
                byte magnitude = GaloisField.Divide(errorValue, locatorDerivativeValue);

                // Применяем амплитуду к соответствующей позиции
                errorMagnitudes[position] = magnitude;
            }

            // Исправляем сообщение
            for (int i = 0; i < received.Coefficients.Length; i++)
            {
                received[i] ^= errorMagnitudes[i];
            }

            return received;
        }

        // Извлечение исходного сообщения
        private byte[] ExtractMessage(byte[] received)
        {
            return received.Skip(numParitySymbols).ToArray();
        }
    }
}
