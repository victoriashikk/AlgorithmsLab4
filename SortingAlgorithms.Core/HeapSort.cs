using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public class HeapSort : ISortingAlgorithm
{
    public string Name => "Пирамидальная сортировка";
    public string Description => "Строим пирамиду из чисел и постепенно разбираем её! 🏔️";
    
    public event Action<int[]>? ArrayUpdated;
    public event Action<string>? LogAdded;
    public event Action<int, int>? ElementsCompared;
    public event Action<int, int>? ElementsSwapped;

    public async Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default)
    {
        LogAdded?.Invoke("🚀 Начинаем пирамидальную сортировку!");
        
        int n = array.Length;

        // Построение max-кучи
        LogAdded?.Invoke("🏗️ Строим пирамиду из элементов...");
        for (int i = n / 2 - 1; i >= 0; i--)
        {
            await Heapify(array, n, i, delayMs, cancellationToken);
        }

        // Извлечение элементов из кучи
        LogAdded?.Invoke("📦 Разбираем пирамиду...");
        for (int i = n - 1; i >= 0; i--)
        {
            // Перемещаем текущий корень в конец
            ElementsSwapped?.Invoke(0, i);
            LogAdded?.Invoke($"🔄 Перемещаем корень {array[0]} в конец на позицию {i}");
            
            (array[0], array[i]) = (array[i], array[0]);
            ArrayUpdated?.Invoke(array);
            await Task.Delay(delayMs, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            // Вызываем heapify на уменьшенной куче
            await Heapify(array, i, 0, delayMs, cancellationToken);
        }
        
        LogAdded?.Invoke("✅ Пирамидальная сортировка завершена!");
    }

    private async Task Heapify(int[] array, int n, int i, int delayMs, CancellationToken cancellationToken)
    {
        int largest = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        // Сравниваем с левым потомком
        if (left < n)
        {
            ElementsCompared?.Invoke(left, largest);
            LogAdded?.Invoke($"🔍 Сравниваем левого потомка {array[left]} с текущим {array[largest]}");
            
            if (array[left] > array[largest])
            {
                largest = left;
                LogAdded?.Invoke($"📈 Левый потомок больше! Новый корень: {array[largest]}");
            }
        }

        // Сравниваем с правым потомком
        if (right < n)
        {
            ElementsCompared?.Invoke(right, largest);
            LogAdded?.Invoke($"🔍 Сравниваем правого потомка {array[right]} с текущим {array[largest]}");
            
            if (array[right] > array[largest])
            {
                largest = right;
                LogAdded?.Invoke($"📈 Правый потомок больше! Новый корень: {array[largest]}");
            }
        }

        // Если largest не корень
        if (largest != i)
        {
            ElementsSwapped?.Invoke(i, largest);
            LogAdded?.Invoke($"🔄 Меняем местами {array[i]} и {array[largest]}");
            
            (array[i], array[largest]) = (array[largest], array[i]);
            ArrayUpdated?.Invoke(array);
            await Task.Delay(delayMs, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return;

            // Рекурсивно heapify затронутая поддерево
            await Heapify(array, n, largest, delayMs, cancellationToken);
        }
    }
}