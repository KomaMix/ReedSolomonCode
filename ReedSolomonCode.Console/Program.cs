using ReedSolomonCode.Helpers;

byte a = 0x57; // Пример числа
byte b = 0x83; // Пример числа

// Сложение
byte sum = GaloisField.Add(a, b);

// Умножение
byte product = GaloisField.Multiply(a, b);

// Деление
byte quotient = GaloisField.Divide(a, b);

// Обратный элемент
byte inverse = GaloisField.Inverse(a);

Console.WriteLine($"Sum: {sum:X2}, Product: {product:X2}, Quotient: {quotient:X2}, Inverse: {inverse:X2}");
