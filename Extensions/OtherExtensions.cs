using System;
using System.Text;

namespace LlamaLibrary.Extensions
{
    public static class OtherExtensions
    {
        public static string AddSpacesToEnum(this Enum toText)
        {
            var text = toText.ToString();
            if (string.IsNullOrWhiteSpace(text))
            {
                return "";
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && text[i - 1] != ' ')
                {
                    newText.Append(' ');
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }
    }
}