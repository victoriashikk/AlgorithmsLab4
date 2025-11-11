using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SortingAlgorithms.Core;

namespace SortingAlgorithms.GUI;

public partial class MainWindow : Window
{
    private ISortingAlgorithm? _currentAlgorithm;
    private ITextSortingAlgorithm? _currentTextAlgorithm;
    private CancellationTokenSource? _cancellationTokenSource;
    private int[]? _currentArray;
    private string[]? _currentWords;

    public MainWindow()
    {
        InitializeComponent();
        SetupControls();
    }

    private void SetupControls()
    {
        // Обработчики для числовой сортировки
        StartButton.Click += (sender, e) => StartNumericSorting();
        ResetButton.Click += (sender, e) => ResetVisualization();

        // Обработчики для текстовой сортировки
        TextSortButton.Click += (sender, e) => StartTextSorting();
        AnalyzeButton.Click += (sender, e) => AnalyzeText();
        Test100Words.Click += (sender, e) => GenerateTestText(100);
        Test500Words.Click += (sender, e) => GenerateTestText(500);
        Test1000Words.Click += (sender, e) => GenerateTestText(1000);

        SpeedSlider.PropertyChanged += (sender, e) => 
        {
            if (e.Property == Slider.ValueProperty)
            {
                SpeedValueText.Text = $"{(int)SpeedSlider.Value}мс";
            }
        };
    }

    #region Числовая сортировка
    private async void StartNumericSorting()
    {
        if (!TryParseArray()) return;

        _cancellationTokenSource = new CancellationTokenSource();
        
        ISortingAlgorithm algorithm = BubbleSortRadio.IsChecked == true 
            ? new BubbleSort() 
            : InsertionSortRadio.IsChecked == true
                ? new InsertionSort()
                : QuickSortRadio.IsChecked == true
                    ? new QuickSort()
                    : new HeapSort();

        await StartNumericSorting(algorithm);
    }

    private async Task StartNumericSorting(ISortingAlgorithm algorithm)
    {
        _currentAlgorithm = algorithm;
        
        algorithm.LogAdded += OnLogAdded;
        algorithm.ArrayUpdated += OnArrayUpdated;

        LogTextBox.Text = $"🚀 Запускаем {algorithm.Name}...\n";
        LogTextBox.Text += $"📖 {algorithm.Description}\n\n";

        var delay = (int)SpeedSlider.Value;
        await algorithm.Sort(_currentArray!, delay, _cancellationTokenSource!.Token);

        algorithm.LogAdded -= OnLogAdded;
        algorithm.ArrayUpdated -= OnArrayUpdated;
    }
    #endregion

    #region Текстовая сортировка
    private async void StartTextSorting()
    {
        var text = TextInputTextBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            TextResultTextBox.Text = "❌ Введите текст для сортировки!";
            return;
        }

        TextResultTextBox.Text = "⏳ Обрабатываем текст...\n";
        
        await Task.Run(() =>
        {
            _currentWords = TextProcessor.SplitTextIntoWords(text);
        });

        TextResultTextBox.Text += $"📝 Обработано слов: {_currentWords!.Length}\n\n";

        _cancellationTokenSource = new CancellationTokenSource();

        ITextSortingAlgorithm algorithm = TextQuickSortRadio.IsChecked == true 
            ? new QuickSortTextAdapter() 
            : new RadixSort();

        await StartTextSortingInternal(algorithm);
    }

    private async Task StartTextSortingInternal(ITextSortingAlgorithm algorithm)
    {
        _currentTextAlgorithm = algorithm;
        
        algorithm.LogAdded += OnTextLogAdded;
        algorithm.ArrayUpdated += OnTextArrayUpdated;

        var stopwatch = Stopwatch.StartNew();
        
        TextResultTextBox.Text += $"🚀 Запускаем {algorithm.Name}...\n";
        TextResultTextBox.Text += $"📖 {algorithm.Description}\n\n";

        var delay = _currentWords!.Length > 100 ? 1 : 10;
        await algorithm.Sort(_currentWords!, delay, _cancellationTokenSource!.Token);

        stopwatch.Stop();
        
        // Финальный результат
        bool isSorted = IsArraySorted(_currentWords!);
        TextResultTextBox.Text += $"\n⏱️ Время выполнения: {stopwatch.Elapsed.TotalSeconds:F2} секунд\n";
        TextResultTextBox.Text += $"{(isSorted ? "✅" : "❌")} Проверка сортировки: {(isSorted ? "УСПЕХ" : "ОШИБКА")}\n\n";
        
        // ПОЛНЫЙ ОТСОРТИРОВАННЫЙ ТЕКСТ
        TextResultTextBox.Text += $"📖 ПОЛНЫЙ ОТСОРТИРОВАННЫЙ ТЕКСТ:\n";
        TextResultTextBox.Text += string.Join(" ", _currentWords) + "\n\n";
        
        // ПОДСЧЕТ ЧАСТОТЫ КАЖДОГО СЛОВА
        var frequency = TextProcessor.CountWordFrequency(_currentWords!);
        TextResultTextBox.Text += $"📊 ЧАСТОТА СЛОВ (всего {frequency.Count} уникальных слов):\n";
        
        // Сортируем по алфавиту для удобства
        var sortedFrequency = frequency.OrderBy(pair => pair.Key).ToArray();
        
        foreach (var (word, count) in sortedFrequency)
        {
            TextResultTextBox.Text += $"{word}: {count} раз\n";
        }

        algorithm.LogAdded -= OnTextLogAdded;
        algorithm.ArrayUpdated -= OnTextArrayUpdated;
    }

    // Метод проверки сортировки
    private bool IsArraySorted(string[] words)
    {
        for (int i = 1; i < words.Length; i++)
        {
            int comparison = string.Compare(words[i-1], words[i], StringComparison.OrdinalIgnoreCase);
            if (comparison > 0)
            {
                // Найдена ошибка сортировки - показываем контекст
                TextResultTextBox.Text += $"\n❌ Ошибка сортировки:";
                TextResultTextBox.Text += $"\n   [{i-1}] '{words[i-1]}' > [{i}] '{words[i]}'";
                
                // Показываем окружающие слова для отладки
                int start = Math.Max(0, i-3);
                int end = Math.Min(words.Length, i+3);
                TextResultTextBox.Text += $"\n   Контекст: ...{string.Join(" ", words.Skip(start).Take(end-start))}...";
                
                return false;
            }
        }
        return true;
    }

    private void AnalyzeText()
    {
        var text = TextInputTextBox.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            StatsTextBox.Text = "❌ Введите текст для анализа!";
            return;
        }

        var words = TextProcessor.SplitTextIntoWords(text);
        var frequency = TextProcessor.CountWordFrequency(words);
        
        var topWords = frequency.OrderByDescending(pair => pair.Value)
                               .Take(10)
                               .ToArray();

        StatsTextBox.Text = $"📈 Статистика текста:\n";
        StatsTextBox.Text += $"📝 Всего слов: {words.Length}\n";
        StatsTextBox.Text += $"🔤 Уникальных слов: {frequency.Count}\n\n";
        StatsTextBox.Text += "🏆 Топ-10 частых слов:\n";
        
        foreach (var (word, count) in topWords)
        {
            StatsTextBox.Text += $"{word}: {count} раз\n";
        }
    }

    private void GenerateTestText(int wordCount)
    {
        var testWords = TextProcessor.GenerateTestText(wordCount);
        TextInputTextBox.Text = string.Join(" ", testWords);
        StatsTextBox.Text = $"✅ Сгенерирован тестовый текст из {wordCount} слов";
    }

    private void OnTextLogAdded(string message)
    {
        TextResultTextBox.Text += $"{message}\n";
    }

    private void OnTextArrayUpdated(string[] words)
    {
        _currentWords = words;
    }
    #endregion

    #region Общие методы
    private bool TryParseArray()
    {
        try
        {
            _currentArray = ArrayTextBox.Text.Split(',')
                .Select(x => int.Parse(x.Trim()))
                .ToArray();
            return true;
        }
        catch
        {
            LogTextBox.Text = "❌ Ошибка: введите числа через запятую!";
            return false;
        }
    }

    private void OnLogAdded(string message)
    {
        LogTextBox.Text += $"{message}\n";
    }

    private void OnArrayUpdated(int[] array)
    {
        DrawArrayVisualization(array);
    }

    private void DrawArrayVisualization(int[] array)
    {
        VisualizationCanvas.Children.Clear();

        if (array.Length == 0) return;

        var maxValue = array.Max();
        var canvasWidth = VisualizationCanvas.Bounds.Width;
        var canvasHeight = VisualizationCanvas.Bounds.Height;
        
        if (canvasWidth <= 0 || canvasHeight <= 0) return;

        var barWidth = canvasWidth / array.Length - 2;
        
        for (int i = 0; i < array.Length; i++)
        {
            var barHeight = (array[i] / (double)maxValue) * canvasHeight;
            
            var rect = new Avalonia.Controls.Shapes.Rectangle
            {
                Width = barWidth,
                Height = barHeight,
                Fill = Brushes.Blue,
                Margin = new Thickness(i * (barWidth + 2), canvasHeight - barHeight, 0, 0)
            };
            
            VisualizationCanvas.Children.Add(rect);
        }
    }

    private void ResetVisualization()
    {
        _cancellationTokenSource?.Cancel();
        VisualizationCanvas.Children.Clear();
        LogTextBox.Text = "";
        TextResultTextBox.Text = "";
        StatsTextBox.Text = "";
    }
    #endregion
}