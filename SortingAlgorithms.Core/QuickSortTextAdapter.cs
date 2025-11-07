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

        for (int j = low; j < high; j++)
        {
            LogAdded?.Invoke($"🔤 Сравниваем '{words[j]}' с '{pivot}'");
            
            if (string.Compare(words[j], pivot, StringComparison.Ordinal) <= 0)
            {
                i++;
                
                if (i != j)
                {
                    LogAdded?.Invoke($"🔄 Меняем местами '{words[i]}' и '{words[j]}'");
                    
                    (words[i], words[j]) = (words[j], words[i]);
                    ArrayUpdated?.Invoke(words);
                    
                    await Task.Delay(delayMs, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return i;
                }
            }
        }

        if (i + 1 != high)
        {
            LogAdded?.Invoke($"🎯 Ставим '{pivot}' на позицию {i + 1}");
            
            (words[i + 1], words[high]) = (words[high], words[i + 1]);
            ArrayUpdated?.Invoke(words);
            
            await Task.Delay(delayMs, cancellationToken);
        }

        return i + 1;
    }
}