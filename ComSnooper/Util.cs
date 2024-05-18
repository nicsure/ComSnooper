using System.Text;


namespace ComSnooper
{
    public static class Util
    {
        private static readonly string dumpSep = "+" + new string('-', 68) + "+";
        public static string AsHexDump(byte[] data, string title = "", int indent = 0)
        {
            string indentation = new(' ', indent);
            var result = new StringBuilder();
            result.Append($"\r\n{indentation}{dumpSep}\r\n");
            string header = $"{title} (Length:{data.Length})";
            string padh = new(' ', 66 - header.Length);
            result.Append($"{indentation}| {header}{padh} |\r\n");
            result.Append($"{indentation}{dumpSep}\r\n");
            for (int i = 0; i < data.Length; i += 16)
            {
                StringBuilder hex = new(), chars = new();
                for (int j = i; j < i + 16 && j < data.Length; j++)
                {
                    hex.Append($"{data[j]:X2} ");
                    chars.Append(data[j] >= 32 && data[j] < 127 ? (char)data[j] : "·");
                }
                string pad1 = new(' ', 48 - hex.Length);
                string pad2 = new(' ', 16 - chars.Length);
                result.Append($"{indentation}| {hex}{pad1}| {chars}{pad2} |\r\n");
            }
            result.Append($"{indentation}{dumpSep}\r\n");
            return result.ToString();
        }

        public static void Out(string text)
        {
            Console.WriteLine(text);
        }
    }
}
