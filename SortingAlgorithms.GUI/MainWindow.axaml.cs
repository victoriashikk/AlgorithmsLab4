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

        _currentWords = TextProcessor.SplitTextIntoWords(text);
        TextResultTextBox.Text = $"📝 Исходный текст: {_currentWords.Length} слов\n";
        TextResultTextBox.Text += string.Join(" ", _currentWords.Take(50)) + "...\n\n";

        _cancellationTokenSource = new CancellationTokenSource();

        ITextSortingAlgorithm algorithm = TextQuickSortRadio.IsChecked == true 
            ? new QuickSortTextAdapter() 
            : new RadixSort();

        await StartTextSorting(algorithm);
    }

    private async Task StartTextSorting(ITextSortingAlgorithm algorithm)
    {
        _currentTextAlgorithm = algorithm;
        
        algorithm.LogAdded += OnTextLogAdded;
        algorithm.ArrayUpdated += OnTextArrayUpdated;

        var stopwatch = Stopwatch.StartNew();
        
        TextResultTextBox.Text += $"🚀 Запускаем {algorithm.Name}...\n";
        TextResultTextBox.Text += $"📖 {algorithm.Description}\n\n";

        var delay = 10; // Быстрая анимация для текста
        await algorithm.Sort(_currentWords!, delay, _cancellationTokenSource!.Token);

        stopwatch.Stop();
        
        TextResultTextBox.Text += $"\n✅ Сортировка завершена за {stopwatch.Elapsed.TotalSeconds:F2} секунд\n";
        TextResultTextBox.Text += $"📊 Отсортированные слова:\n{string.Join(" ", _currentWords!.Take(100))}...";

        algorithm.LogAdded -= OnTextLogAdded;
        algorithm.ArrayUpdated -= OnTextArrayUpdated;
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
        // Для текста просто обновляем отображение
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