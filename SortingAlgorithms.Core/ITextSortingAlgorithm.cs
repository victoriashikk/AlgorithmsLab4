using System;
using System.Threading;
using System.Threading.Tasks;

namespace SortingAlgorithms.Core;

public interface ITextSortingAlgorithm
{
    string Name { get; }
    string Description { get; }
    event Action<string[]> ArrayUpdated;
    event Action<string> LogAdded;
    
    Task Sort(string[] words, int delayMs = 100, CancellationToken cancellationToken = default);
}