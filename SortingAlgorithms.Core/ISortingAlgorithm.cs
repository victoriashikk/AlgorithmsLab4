using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public interface ISortingAlgorithm
{
    string Name { get; }
    string Description { get; }
    event Action<int[]>? ArrayUpdated;
    event Action<string>? LogAdded;
    event Action<int, int>? ElementsCompared;
    event Action<int, int>? ElementsSwapped;
    
    Task Sort(int[] array, int delayMs = 100, CancellationToken cancellationToken = default);
}