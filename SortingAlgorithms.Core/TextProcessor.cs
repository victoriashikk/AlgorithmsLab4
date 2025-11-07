using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortingAlgorithms.Core;

public class TextProcessor
{
    public static string[] SplitTextIntoWords(string text)
    {
        // Удаляем знаки препинания и разбиваем на слова
        var cleanText = Regex.Replace(text, @"[^\w\s]", "");
        var words = cleanText.Split(new[] { ' ', '\n', '\r', '\t' }, 
            StringSplitOptions.RemoveEmptyEntries);
        
        // Приводим к нижнему регистру
        return words.Select(word => word.ToLower()).ToArray();
    }

    public static Dictionary<string, int> CountWordFrequency(string[] words)
    {
        var frequency = new Dictionary<string, int>();
        
        foreach (var word in words)
        {
            if (frequency.ContainsKey(word))
                frequency[word]++;
            else
                frequency[word] = 1;
        }
        
        return frequency;
    }

    public static string[] GenerateTestText(int wordCount)
    {
        var random = new Random();
        var testWords = new[] { "the", "and", "is", "in", "to", "of", "a", "that", "it", "with" };
        var result = new string[wordCount];
        
        for (int i = 0; i < wordCount; i++)
        {
            result[i] = testWords[random.Next(testWords.Length)];
        }
        
        return result;
    }
}