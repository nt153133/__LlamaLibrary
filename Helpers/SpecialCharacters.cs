using System.Text;

namespace LlamaLibrary.Helpers
{
    public static class SpecialCharacters
    {
        public const char Dice = '\uE03e';
        public const char Clock = '\ue031';
        public const char ArrowUp = '\u25b2';
        public const char ArrowDown = '\u25bc';
        public const char ServerSeparatorSend = '\uE05D';
        public const char ServerSeparatorReceive = '\u2740';
        public static readonly char[] ServerSeparators = new char[] { ServerSeparatorSend, ServerSeparatorReceive };
        public static readonly char[] Numbers = new char[] { '\ue060', '\ue061', '\ue062', '\ue063', '\ue064', '\ue065', '\ue066', '\ue067', '\ue068', '\ue066' };
        public static readonly char[] BlockNumbers = new char[] { '\ue08f', '\ue090', '\ue091', '\ue092', '\ue093', '\ue094', '\ue095', '\ue096', '\ue097', '\ue098' };

        public static string GetNumberString(int number)
        {
            var numberString = number.ToString();
            var result = new StringBuilder();
            foreach (var c in numberString)
            {
                result.Append(Numbers[int.Parse(c.ToString())]);
            }

            return result.ToString();
        }

        public static string GetBlockNumberString(int number)
        {
            var numberString = number.ToString();
            var result = new StringBuilder();
            foreach (var c in numberString)
            {
                result.Append(BlockNumbers[int.Parse(c.ToString())]);
            }

            return result.ToString();
        }
    }
}