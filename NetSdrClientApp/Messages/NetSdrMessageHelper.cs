using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrClientApp.Messages
{
    // ... (Constants, Enums) ...

    public static class NetSdrMessageHelper
    {
        // ... (Other helper methods) ...

        // FIX: Новий метод-обгортка для розділення логіки
        public static IEnumerable<int> GetSamples(ushort sampleSize, byte[] body)
        {
            int bodySampleSize = CheckSamplesParameters(sampleSize); // Виклик перевірки
            return GetSamplesInternal(bodySampleSize, body); // Виклик ітератора
        }

        // FIX: Метод для перевірки параметрів
        private static int CheckSamplesParameters(ushort sampleSize)
        {
            int bodySampleSize = sampleSize / 8; //to bytes
            if (bodySampleSize > 4)
            {
                // FIX: Використання конструктора з деталізованим повідомленням
                throw new ArgumentOutOfRangeException(nameof(sampleSize), sampleSize, "Sample size must not exceed 32 bits (4 bytes).");
            }
            return bodySampleSize;
        }

        // FIX: Основний метод-ітератор
        private static IEnumerable<int> GetSamplesInternal(int bodySampleSize, byte[] body)
        {
            var bodyEnumerable = body as IEnumerable<byte>;
            var prefixBytes = Enumerable.Range(0, 4 - bodySampleSize)
                                      .Select(b => (byte)0);

            while (bodyEnumerable.Count() >= bodySampleSize)
            {
                yield return BitConverter.ToInt32(bodyEnumerable
                    .Take(bodySampleSize)
                    .Concat(prefixBytes)
                    .ToArray());
                bodyEnumerable = bodyEnumerable.Skip(bodySampleSize);
            }
        }

        // ... (GetHeader, TranslateHeader) ...
    }
}