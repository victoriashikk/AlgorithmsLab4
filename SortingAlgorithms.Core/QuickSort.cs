using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class QuickSort : ISortingAlgorithm
{
    public string Name => "Быстрая сортировка";
    public string Description => "Разделяй и властвуй! Как в игре 'горячо-холодно' - находим опорный элемент и сортируем вокруг него! 🎯";
    
    public event Action<int[]>? ArrayUpdated;
    public event Action<string>? LogAdded;
    public event Action<int, int>? ElementsCompared;
    public event Action<int, int>? ElementsSwapped;

    public async Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        LogAdded?.Invoke("🚀 Начинаем быструю сортировку!");
        await QuickSortRecursive(array, 0, array.Length - 1, delayMs, cancellationToken);
        LogAdded?.Invoke("✅ Быстрая сортировка завершена!");
    }

    private async Task QuickSortRecursive(int[] array, int low, int high, int delayMs, CancellationToken cancellationToken)
    {
        if (low < high)
        {
            LogAdded?.Invoke($"📊 Сортируем часть массива от {low} до {high}");
            
            int pivotIndex = await Partition(array, low, high, delayMs, cancellationToken);
            
            LogAdded?.Invoke($"🎯 Опорный элемент {array[pivotIndex]} на позиции {pivotIndex}");
            
            await QuickSortRecursive(array, low, pivotIndex - 1, delayMs, cancellationToken);
            await QuickSortRecursive(array, pivotIndex + 1, high, delayMs, cancellationToken);
        }
    }

    private async Task<int> Partition(int[] array, int low, int high, int delayMs, CancellationToken cancellationToken)
    {
        int pivot = array[high];
        LogAdded?.Invoke($"🎯 Выбираем опорный элемент: {pivot} (последний в диапазоне)");
        
        int i = low - 1;

        for (int j = low; j < high; j++)
        {
            ElementsCompared?.Invoke(j, high);
            LogAdded?.Invoke($"🔍 Сравниваем {array[j]} с опорным {pivot}");
            
            if (array[j] <= pivot)
            {
                i++;
                
                if (i != j)
                {
                    ElementsSwapped?.Invoke(i, j);
                    LogAdded?.Invoke($"🔄 Меняем местами {array[i]} и {array[j]}");
                    
                    (array[i], array[j]) = (array[j], array[i]);
                    ArrayUpdated?.Invoke(array);
                    
                    await Task.Delay(delayMs, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return i;
                }
            }
        }

        if (i + 1 != high)
        {
            ElementsSwapped?.Invoke(i + 1, high);
            LogAdded?.Invoke($"🎯 Ставим опорный элемент {pivot} на правильную позицию {i + 1}");
            
            (array[i + 1], array[high]) = (array[high], array[i + 1]);
            ArrayUpdated?.Invoke(array);
            
            await Task.Delay(delayMs, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return i + 1;
        }

        return i + 1;
    }
}