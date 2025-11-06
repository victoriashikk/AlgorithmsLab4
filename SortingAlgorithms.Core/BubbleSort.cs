using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class BubbleSort : ISortingAlgorithm
{
    public string Name => "Сортировка пузырьком";
    public string Description => "Как пузырьки в газировке - большие числа всплывают вверх! 🫧";
    
    public event Action<int[]>? ArrayUpdated;
    public event Action<string>? LogAdded;
    public event Action<int, int>? ElementsCompared;
    public event Action<int, int>? ElementsSwapped;

    public async Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        var n = array.Length;
        
        for (var i = 0; i < n - 1; i++)
        {
            for (var j = 0; j < n - i - 1; j++)
            {
                ElementsCompared?.Invoke(j, j + 1);
                LogAdded?.Invoke($"🔍 Сравниваем: {array[j]} и {array[j + 1]}");
                
                if (array[j] > array[j + 1])
                {
                    ElementsSwapped?.Invoke(j, j + 1);
                    LogAdded?.Invoke($"🔄 Меняем местами: {array[j]} ⇄ {array[j + 1]}");
                    
                    (array[j], array[j + 1]) = (array[j + 1], array[j]);
                    ArrayUpdated?.Invoke(array);
                    
                    await Task.Delay(delayMs, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                }
            }
        }
        
        LogAdded?.Invoke("✅ Сортировка завершена!");
    }
}