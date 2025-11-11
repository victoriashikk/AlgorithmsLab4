using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class QuickSortTextAdapter : ITextSortingAlgorithm
{
    public string Name => "QuickSort для текста";
    public string Description => "Быстрая сортировка слов по алфавиту! 📚";
    
    public event Action<string[]>? ArrayUpdated;
    public event Action<string>? LogAdded;

    public async Task Sort(string[] words, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        LogAdded?.Invoke("🚀 Начинаем быструю сортировку текста!");
        await QuickSortRecursive(words, 0, words.Length - 1, delayMs, cancellationToken);
        LogAdded?.Invoke("✅ Текст отсортирован!");
    }

    private async Task QuickSortRecursive(string[] words, int low, int high, int delayMs, CancellationToken cancellationToken)
    {
        if (low < high)
        {
            LogAdded?.Invoke($"🔍 Сортируем слова с {low} по {high}");
            
            int pivotIndex = await Partition(words, low, high, delayMs, cancellationToken);
            
            LogAdded?.Invoke($"📖 Опорное слово: '{words[pivotIndex]}'");
            
            await QuickSortRecursive(words, low, pivotIndex - 1, delayMs, cancellationToken);
            await QuickSortRecursive(words, pivotIndex + 1, high, delayMs, cancellationToken);
        }
    }

    private async Task<int> Partition(string[] words, int low, int high, int delayMs, CancellationToken cancellationToken)
{
    string pivot = words[high];
    int i = low - 1;

    bool verboseLogging = words.Length < 50;

    for (int j = low; j < high; j++)
    {
        if (verboseLogging)
        {
            LogAdded?.Invoke($"🔤 Сравниваем '{words[j]}' с '{pivot}'");
        }
        
        // ИСПРАВЛЕНО: Правильное сравнение с учетом цифр и букв
        if (CompareWords(words[j], pivot) <= 0)
        {
            i++;
            
            if (i != j)
            {
                if (verboseLogging)
                {
                    LogAdded?.Invoke($"🔄 Меняем местами '{words[i]}' и '{words[j]}'");
                }
                
                (words[i], words[j]) = (words[j], words[i]);
                ArrayUpdated?.Invoke(words);
                
                await Task.Delay(delayMs, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return i;
            }
        }
    }

    if (i + 1 != high)
    {
        if (verboseLogging)
        {
            LogAdded?.Invoke($"🎯 Ставим '{pivot}' на позицию {i + 1}");
        }
        
        (words[i + 1], words[high]) = (words[high], words[i + 1]);
        ArrayUpdated?.Invoke(words);
        
        await Task.Delay(delayMs, cancellationToken);
    }

    return i + 1;
}
    
// НОВЫЙ МЕТОД: Правильное сравнение слов
    private int CompareWords(string a, string b)
    {
        // Сначала сравниваем по первому символу с учетом типа (цифра/буква)
        if (a.Length > 0 && b.Length > 0)
        {
            bool aStartsWithDigit = char.IsDigit(a[0]);
            bool bStartsWithDigit = char.IsDigit(b[0]);
            
            // Если один начинается с цифры, а другой с буквы - буквы идут первыми
            if (aStartsWithDigit && !bStartsWithDigit)
                return 1; // a > b (цифры после букв)
            if (!aStartsWithDigit && bStartsWithDigit)
                return -1; // a < b (буквы перед цифрами)
        }
        
        // Оба слова начинаются с цифр или оба с букв - сравниваем обычным способом
        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }
}