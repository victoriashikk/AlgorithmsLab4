using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SortingAlgorithms.Core;

namespace SortingAlgorithms.GUI;

public partial class MainWindow : Window
{
    private ISortingAlgorithm? _currentAlgorithm;
    private CancellationTokenSource? _cancellationTokenSource;
    private int[]? _currentArray;

    public MainWindow()
    {
        InitializeComponent();
        SetupControls();
    }

    private void SetupControls()
    {
        // Обработчики кнопок
        StartButton.Click += (sender, e) => StartButton_Click(sender, e);
        ResetButton.Click += (sender, e) => ResetButton_Click(sender, e);
    }
    
    private async void StartButton_Click(object? sender, EventArgs e)
    {
        if (!TryParseArray()) return;

        _cancellationTokenSource = new CancellationTokenSource();
        await StartSorting();
    }

    private void ResetButton_Click(object? sender, EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        ResetVisualization();
    }

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

    private async Task StartSorting()
    {
        ISortingAlgorithm algorithm = BubbleSortRadio.IsChecked == true 
            ? new BubbleSort() 
            : InsertionSortRadio.IsChecked == true
                ? new InsertionSort()
                : QuickSortRadio.IsChecked == true
                    ? new QuickSort()
                    : new HeapSort();

        if (!TryParseArray()) return;

        _currentAlgorithm = algorithm;
    
        // Подписываемся на события
        algorithm.LogAdded += OnLogAdded;
        algorithm.ArrayUpdated += OnArrayUpdated;

        LogTextBox.Text = $"🚀 Запускаем {algorithm.Name}...\n";
        LogTextBox.Text += $"📖 {algorithm.Description}\n\n";

        var delay = (int)SpeedSlider.Value;
        await algorithm.Sort(_currentArray!, delay, _cancellationTokenSource!.Token);

        algorithm.LogAdded -= OnLogAdded;
        algorithm.ArrayUpdated -= OnArrayUpdated;
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
        VisualizationCanvas.Children.Clear();
        LogTextBox.Text = "";
    }
}