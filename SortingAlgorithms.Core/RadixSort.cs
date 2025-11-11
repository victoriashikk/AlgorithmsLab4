using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class RadixSort : ITextSortingAlgorithm
{
    public string Name => "Radix сортировка";
    public string Description => "Сортировка поразрядно! Смотрим на каждую букву в словах! 🔤";
    
    public event Action<string[]>? ArrayUpdated;
    public event Action<string>? LogAdded;

    public async Task Sort(string[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        LogAdded?.Invoke("🚀 Начинаем Radix сортировку!");
        LogAdded?.Invoke("📖 Будем сортировать слова, начиная с ПОСЛЕДНЕЙ буквы!");
        
        if (array.Length == 0) return;

        // Находим самое длинное слово
        int maxLength = array.Max(s => s?.Length ?? 0);
        LogAdded?.Invoke($"📏 Самое длинное слово: {maxLength} букв");

        // Сортируем по каждой позиции (с ПОСЛЕДНЕЙ до первой)
        for (int position = maxLength - 1; position >= 0; position--)
        {
            LogAdded?.Invoke($"\n🔤 ШАГ {maxLength - position}: Сортируем по {position + 1}-й букве с КОНЦА");
            
            await CountingSortByPosition(array, position, delayMs, cancellationToken);
            
            if (cancellationToken.IsCancellationRequested) return;
        }
        
        LogAdded?.Invoke("\n🎉 Все буквы обработаны!");
        LogAdded?.Invoke("✅ Radix сортировка завершена!");
    }

    private async Task CountingSortByPosition(string[] array, int position, int delayMs, CancellationToken cancellationToken)
    {
        const int bucketCount = 27; // 26 букв + 1 для коротких слов
        
        // Создаем ведра для каждой буквы
        List<string>[] buckets = new List<string>[bucketCount];
        for (int i = 0; i < bucketCount; i++)
        {
            buckets[i] = new List<string>();
        }

        // Распределяем слова по ведрам
        foreach (var word in array)
        {
            int bucketIndex = GetBucketIndex(word, position);
            buckets[bucketIndex].Add(word);
        }

        // Показываем распределение
        LogAdded?.Invoke($"📊 Распределение по буквам:");
        for (int i = 0; i < bucketCount; i++)
        {
            if (buckets[i].Count > 0)
            {
                string bucketName = i == 0 ? "короткие" : $"{(char)('a' + i - 1)}";
                LogAdded?.Invoke($"   🪣 Буква '{bucketName}': {buckets[i].Count} слов");
            }
        }

        // Собираем обратно в массив
        int currentIndex = 0;
        for (int i = 0; i < bucketCount; i++)
        {
            foreach (var word in buckets[i])
            {
                array[currentIndex] = word;
                currentIndex++;

                // Анимация каждые 10 слов
                if (currentIndex % 10 == 0)
                {
                    ArrayUpdated?.Invoke(array);
                    await Task.Delay(delayMs, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                }
            }
        }

        // Финальное обновление
        ArrayUpdated?.Invoke(array);
        await Task.Delay(delayMs, cancellationToken);
    }

    private int GetBucketIndex(string word, int position)
    {
        if (position >= word.Length)
            return 0; // Ведро для коротких слов
        
        char c = char.ToLowerInvariant(word[position]);
        if (c >= 'a' && c <= 'z')
            return c - 'a' + 1; // Буквы a-z -> ведра 1-26
        
        return 0; // Не-буквенные символы -> в ведро для коротких
    }
}