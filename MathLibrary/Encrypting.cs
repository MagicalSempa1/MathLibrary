using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathLibrary
{
    public static class Encrypting
    {
        private const string alphabetRU = "АБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЫЬЭЮЯ .,!?:-";//"абвгдеёжзийклмнопрстуфхцчшщъыьэюя";
        public static string CaesarCipher(string text, int shift)
        {
            shift %= alphabetRU.Length;
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                int index = alphabetRU.IndexOf(text[i]);
                if (index == -1)
                {
                    result += text[i];
                    continue;
                }
                index += shift;
                if (shift > 0)
                    if (index >= alphabetRU.Length)
                        index -= alphabetRU.Length;
                if (shift < 0)
                    if (index < 0)
                        index += alphabetRU.Length;
                result += alphabetRU[index];
            }
            return result;
        }

        public static string VigenereCipher(string text, string key)
        {
            while (key.Length < text.Length)
                key += key;
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                int index = alphabetRU.IndexOf(text[i]);
                index = (index + alphabetRU.IndexOf(key[i])) % alphabetRU.Length;
                result += alphabetRU[index];
            }
            return result;
        }
    }
}
