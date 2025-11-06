using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class InsertionSort : ISortingAlgorithm
{
    public string Name => "Сортировка вставками";
    public string Description => "Как вставлять карты в руку - находим правильное место для каждого элемента! 🃏";
    
    public event Action<int[]>? ArrayUpdated;
    public event Action<string>? LogAdded;
    public event Action<int, int>? ElementsCompared;
    public event Action<int, int>? ElementsSwapped;

    public async Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        for (var i = 1; i < array.Length; i++)
        {
            var key = array[i];
            var j = i - 1;
            
            LogAdded?.Invoke($"🎯 Обрабатываем элемент: {key} на позиции {i}");

            while (j >= 0 && array[j] > key)
            {
                ElementsCompared?.Invoke(j, i);
                LogAdded?.Invoke($"📤 Сдвигаем {array[j]} вправо");
                
                array[j + 1] = array[j];
                ElementsSwapped?.Invoke(j, j + 1);
                
                j--;
                
                ArrayUpdated?.Invoke(array);
                await Task.Delay(delayMs, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return;
            }
            
            array[j + 1] = key;
            LogAdded?.Invoke($"📥 Вставляем {key} на позицию {j + 1}");
            ArrayUpdated?.Invoke(array);
        }
        
        LogAdded?.Invoke("✅ Сортировка завершена!");
    }
}