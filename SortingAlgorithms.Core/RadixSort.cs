using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class RadixSort : ISortingAlgorithm, ITextSortingAlgorithm{
    public string Name => "Radix сортировка";
    public string Description => "Сортировка поразрядно! Как сортировать слова по алфавиту, начиная с последней буквы! 🔤";
    
    public event Action<int[]>? ArrayUpdated;
    public event Action<string>? LogAdded;
    public event Action<int, int>? ElementsCompared;
    public event Action<int, int>? ElementsSwapped;

    // Для работы с массивами чисел (реализация интерфейса)
    public async Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        // Преобразуем числа в строки для демонстрации
        string[] stringArray = array.Select(x => x.ToString()).ToArray();
        await SortStrings(stringArray, delayMs, cancellationToken);
        
        // Обратно в числа
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = int.Parse(stringArray[i]);
        }
        ArrayUpdated?.Invoke(array);
    }

    // Основная реализация для строк
    public async Task SortStrings(string[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        LogAdded?.Invoke("🚀 Начинаем Radix сортировку строк!");
        
        if (array.Length == 0) return;

        // Находим максимальную длину строки
        int maxLength = array.Max(s => s.Length);
        LogAdded?.Invoke($"📏 Максимальная длина слова: {maxLength} символов");

        // Сортируем по каждому разряду, начиная с последнего
        for (int digit = maxLength - 1; digit >= 0; digit--)
        {
            LogAdded?.Invoke($"🔍 Сортируем по {digit + 1}-й букве с конца");
            await CountingSortByDigit(array, digit, delayMs, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested) return;
        }
        
        LogAdded?.Invoke("✅ Radix сортировка завершена!");
    }

    private async Task CountingSortByDigit(string[] array, int digit, int delayMs, CancellationToken cancellationToken)
    {
        const int range = 256; // ASCII characters
        
        string[] output = new string[array.Length];
        int[] count = new int[range + 1];

        // Подсчитываем частоты
        for (int i = 0; i < array.Length; i++)
        {
            int charIndex = GetCharIndex(array[i], digit);
            count[charIndex + 1]++;
        }

        // Накопительные суммы
        for (int i = 1; i < count.Length; i++)
        {
            count[i] += count[i - 1];
        }

        // Строим отсортированный массив
        for (int i = array.Length - 1; i >= 0; i--)
        {
            int charIndex = GetCharIndex(array[i], digit);
            output[count[charIndex] - 1] = array[i];
            count[charIndex]--;
        }

        // Копируем обратно
        for (int i = 0; i < array.Length; i++)
        {
            if (!array[i].Equals(output[i]))
            {
                LogAdded?.Invoke($"🔄 Перемещаем '{array[i]}' -> '{output[i]}' по {digit + 1}-й букве");
                array[i] = output[i];
                
                // Для визуализации преобразуем обратно в числа
                int[] tempArray = array.Select(s => int.Parse(s)).ToArray();
                ArrayUpdated?.Invoke(tempArray);
                
                await Task.Delay(delayMs, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
        }
    }

    private int GetCharIndex(string str, int digit)
    {
        if (digit >= str.Length)
            return 0; // Для строк короче - считаем как пробел
        
        return (int)str[digit];
    }
    // Явная реализация для ITextSortingAlgorithm
    event Action<string[]> ITextSortingAlgorithm.ArrayUpdated
    {
        add { _textArrayUpdated += value; }
        remove { _textArrayUpdated -= value; }
    }

    private event Action<string[]> _textArrayUpdated;

    async Task ITextSortingAlgorithm.Sort(string[] words, int delayMs, CancellationToken cancellationToken)
    {
        await SortStrings(words, delayMs, cancellationToken);
    }
}