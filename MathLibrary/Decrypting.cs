using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class Decrypting
    {
        static string alphabet = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ .,!?:-";
        static string[] invalidCombinations = { "ГЙ", "ЩЫ", "ЩЮ", "ЩВ", "ЩЦ", "ЩЖ", "ЩР", "ЩГ", "СЙ", "ШЫ", "ШЙ", "ЧЙ", "ЗЙ", "БЙ", "ЙЙ", "КЙ", "ЖЙ", "РЙ", "ВЙ", "ЛЙ", "ЖЫ", "ЫЪ", "ЪЫ", "ЪЬ", "ЙЫ", "ЙЪ", "ЖЫ", "ЖЫ", "ЙЙ", "ШШ", "ЩЩ", "ЪЪ", "ЫЫ", "ЬЬ", "ЭЭ", "АЫ", "АЬ", "ГЪ", "ЕЭ", "ЙЖ", "ЖФ", "ЖЧ", "ЖШ", "ЖЩ", "ЗП", "ЗЩ", "ЙЬ", "ОЫ", "УЫ", "УЬ", "ФЦ", "ХЩ", "ЦЩ", "ЦЭ", "ЧЩ", "ЧЭ", "ШЩ", "ЬЫ", "ЫЭ", "АЪ", "ИЪ", "ЙЪ", "КЪ", "ЛЪ", "МЪ", "ОЪ", "ПЪ", "РЪ", "УЪ", "ФЪ", "ЦЪ", "ЧЪ", "ШЪ", "ЩЪ", "ЫЪ", "ЬЪ", "ЭЪ" };
        static int n = alphabet.Length;

        public static string Decipher(string cipherText, string key)
        {
            StringBuilder plainText = new StringBuilder();
            int keyIndex = 0;
            int[] keyIndices = new int[key.Length];

            // Precompute key indices
            for (int i = 0; i < key.Length; i++)
                keyIndices[i] = alphabet.IndexOf(key[i]);
            // Process each character in the cipher text
            foreach (var c in cipherText)
            {
                int j = alphabet.IndexOf(c);
                if (j >= 0)
                {
                    int m = (j - keyIndices[keyIndex] + n) % n;
                    plainText.Append(alphabet[m]);
                    keyIndex = (keyIndex + 1) % key.Length;
                }
                else
                    plainText.Append(c);
            }

            return plainText.ToString();
        }

        public static char Decipher(char cipherChar, char key) =>
            alphabet[(alphabet.IndexOf(cipherChar) - alphabet.IndexOf(key) + n) % n];

        public static string FindPossibleDecryptions(string cipherText)
        {
            List<string> possibleDecryptions = new List<string>();
            for (int i = 0; i < n; i++)
            {
                if (InvalidStartSymbol(alphabet[i]) || InvalidStartSymbol(Decipher(cipherText[0], alphabet[i])))
                    continue;
                for (int j = 0; j < n; j++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        for (int l = 0; l < n; l++)
                        {
                            string key = $"{alphabet[i]}{alphabet[j]}{alphabet[k]}{alphabet[l]}";
                            if (!InvalidCombinations(key))
                            {
                                string plainText = Decipher(cipherText, key);
                                // Check if plain text is valid
                                if (IsRussian(plainText) && !InvalidCombinations(plainText) && !plainText.Contains("  ") && !plainText.Contains(",,") && !plainText.Split(' ').Any(w => w.Length > 24))
                                    return $"Key = {key}: {plainText}";
                            }
                        }
                    }
                }
            }

            return "FAIL";
        }

        private static bool IsRussian(string text) =>
            text.All(c => char.IsWhiteSpace(c) || char.IsPunctuation(c) || IsRussianChar(c));

        private static bool IsRussianChar(char c) =>
            c >= 'А' && c <= 'я';

        private static bool InvalidCombinations(string text)
        {
            foreach (var comb in invalidCombinations)
                if (text.Contains(comb))
                    return true;
            return false;
        }

        private static bool InvalidStartSymbol(char text) =>
            new[] { 'Ъ', 'Ы', 'Ь', ' ', ',', '.', '!', '?', ':', '-' }.Contains(text);

        private static int CountRussianWords(string text)
        {
            string pattern = @"\\b[А-Яа-я]+\\b";
            Regex regex = new Regex(pattern);
            MatchCollection matches = regex.Matches(text);
            return matches.Count;
        }
    }
}
