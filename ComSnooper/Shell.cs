using System.Globalization;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;


namespace ComSnooper
{
    public static partial class Shell
    {
        private static ComPort comA = null!, comB = null!;
        private static int a_baud = 38400, b_baud = 38400;
        private static int a_dbits = 8, b_dbits = 8;
        private static int a_port = 1, b_port = 2;
        private static int indentation = 40;
        private static Parity a_parity = Parity.None, b_parity = Parity.None;
        private static StopBits a_stopbits = StopBits.One, b_stopbits = StopBits.One;
        private static FileStream? log = null;

        [GeneratedRegex(@"[\t\r\n]+")]
        private static partial Regex WSRegex();

        public static void Go()
        {
            QueryA();
            QueryB();
            while (true)
            {
                Console.Write("\r\n# ");
                string? cmd;
                try { cmd = Console.ReadLine(); } catch { cmd = null; }
                cmd = cmd == null ? "Q" : WSRegex().Replace(cmd, " ").Trim();
                string[] p = cmd.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int cnt = p.Length;
                string p0s = cnt > 0 ? p[0].ToUpper() : string.Empty;
                string p1s = cnt > 1 ? p[1] : string.Empty;
                string p2s = cnt > 2 ? p[2].ToUpper() : string.Empty;
                string p1tc = p1s.TitleCase();
                int p0 = p0s.ParseNN();
                int p1 = p1s.ParseNN();
                bool setA = p2s != "B";
                bool setB = p2s != "A";
                switch (p0s)
                {
                    case "":
                        break;
                    default:
                        Out($"Unknown Command {p0s}");
                        break;
                    case "I":
                    case "INDENT":
                        if (cnt > 1)
                        {
                            if (p1 >= 0 && p1 <= 160)
                                indentation = p1;
                            else
                            {
                                Out($"Invalid Indentation {p1}");
                                break;
                            }
                        }
                        Out($"Indentation: {indentation}");
                        break;
                    case "D":
                    case "DB":
                    case "DATABITS":
                        if (cnt > 1)
                        {
                            if (p1 > 0 && p1 <= 32)
                            {
                                if (setA) a_dbits = p1;
                                if (setB) b_dbits = p1;
                            }
                            else
                            {
                                Out($"Invalid Data Bits ({p1})");
                                break;
                            }
                        }
                        if (setA) QueryA();
                        if (setB) QueryB();
                        break;
                    case "U":
                    case "BAUD":
                        if (cnt > 1)
                        {
                            if (p1 > 0 && p1 < 1000000)
                            {
                                if (setA) a_baud = p1;
                                if (setB) b_baud = p1;
                            }
                            else
                            {
                                Out($"Invalid Baud Rate ({p1})");
                                break;
                            }
                        }
                        if (setA) QueryA();
                        if (setB) QueryB();
                        break;
                    case "X":
                    case "SB":
                    case "STOPBITS":
                        if (cnt > 1)
                        {
                            if (Enum.TryParse(typeof(StopBits), p1tc, out object? newStopbits))
                            {
                                if (setA) a_stopbits = (StopBits)newStopbits;
                                if (setB) b_stopbits = (StopBits)newStopbits;
                            }
                            else
                            {
                                Out($"Invalid StopBits ({p1tc})");
                                break;
                            }
                        }
                        if (setA) QueryA();
                        if (setB) QueryB();
                        break;
                    case "P":
                    case "PARITY":
                        if (cnt > 1)
                        {
                            if (Enum.TryParse(typeof(Parity), p1tc, out object? newParity))
                            {
                                if (setA) a_parity = (Parity)newParity;
                                if (setB) b_parity = (Parity)newParity;
                            }
                            else
                            {
                                Out($"Invalid Parity ({p1tc})");
                                break;
                            }
                        }
                        if(setA) QueryA();
                        if(setB) QueryB();
                        break;
                    case "S":
                    case "PORTS":
                        Out("Available Serial Ports.\r\n");
                        foreach (var prt in SerialPort.GetPortNames())
                            Out(prt);
                        break;
                    case "?":
                    case "HELP":
                        Out("Command Help\r\n");
                        Out("A  [portNumber]\t\tSet or Query port A.");
                        Out("B  [portNumber]\t\tSet or Query port B.");
                        Out("AB\t\t\tQuery ports A & B.");
                        Out("Q\t\t\tQuit application.");
                        Out("I  [indentation]\tSet or Query message conversation style indentation amount.");
                        Out("R\t\t\tRun - Starts COM port snooping.");
                        Out("Z\t\t\tAbort - Stops COM port snooping.");
                        Out("L  [fileName]\t\tSet or Query logging to file.");
                        Out("L  OFF\t\t\tStops logging to file.");
                        Out("S\t\t\tList available serial ports.");
                        Out("P  [parity] [A|B]\tSet bit parity.");
                        Out("X  [stopbits] [A|B]\tSet stop bits.");
                        Out("U  [baudrate] [A|B]\tSet baud rate.");
                        Out("D  [databits] [A|B]\tSet data bits.");
                        Out("?\t\t\tShow help.\r\n");
                        Out("Aliases");
                        Out("AB\tBA");
                        Out("Q\tQUIT EXIT");
                        Out("I\tINDENT");
                        Out("R\tRUN");
                        Out("Z\tABORT CLOSE");
                        Out("L\tLOG");
                        Out("S\tPORTS");
                        Out("P\tPARITY");
                        Out("X\tSB STOPBITS");
                        Out("U\tBAUD");
                        Out("D\tDB DATABITS");
                        Out("?\tHELP");
                        break;
                    case "AB":
                    case "BA":
                        QueryA();
                        QueryB();
                        break;
                    case "A":
                    case "B":
                        bool isA = p0s == "A";
                        if (cnt > 1)
                        {
                            if (p1 == -1 && p1s.StartsWith("COM", StringComparison.InvariantCultureIgnoreCase))
                                p1 = p1s[3..].ParseNN();
                            if (p1 < 1000 && p1 > 0)
                            {
                                if (isA)
                                    a_port = p1;
                                else
                                    b_port = p1;
                            }
                            else
                            {
                                Out($"Bad COM port number ({p1})");
                                break;
                            }
                        }
                        if (isA) 
                            QueryA();
                        else 
                            QueryB();
                        break;
                    case "Q":
                    case "QUIT":
                    case "EXIT":
                        Abort(true);
                        return;
                    case "R":
                    case "RUN":
                        if ((comA?.Active ?? false) || (comB?.Active ?? false))
                            Out("Already running, aborting current session.");
                        Abort(true);
                        p1 = a_port;
                        comA = new(a_port, a_baud, a_parity, a_dbits, a_stopbits, ReceivedA);
                        if (comA.Active)
                        {
                            p1 = b_port;
                            comB = new(b_port, b_baud, b_parity, b_dbits, b_stopbits, ReceivedB);
                            if (comB.Active)
                            {
                                Out($"Running COM{a_port} <-> COM{b_port}");
                                break;
                            }
                        }
                        Out($"Cannot open COM{p1}");
                        Abort();
                        break;
                    case "Z":
                    case "ABORT":
                    case "CLOSE":
                        Abort();
                        break;
                    case "L":
                    case "LOG":
                        if (cnt >= 2)
                        {
                            if (log != null)
                            {
                                Out($"Closing log file: {log.Name}");
                                using (log)
                                {
                                    try { log.Close(); } catch { }
                                }
                                log = null;
                            }
                            if (!p1s.ToUpper().Equals("OFF") && !p1s.ToUpper().Equals("STOP"))
                            {
                                try
                                {
                                    log = new(string.Join(" ", p[1..]), FileMode.Create);
                                }
                                catch { Out("Error creating log file."); }
                            }
                        }
                        ReportLogStatus();
                        break;
                }
            }
        }

        private static void QueryA()
        {
            Out($"Port A = COM{a_port}:{a_baud},{a_dbits},{a_parity},{a_stopbits}");
        }

        private static void QueryB()
        {
            Out($"Port B = COM{b_port}:{b_baud},{b_dbits},{b_parity},{b_stopbits}");
        }

        private static void LogHex(byte[] hex, int from, int to, int indent = 0)
        {
            string hexd = Util.AsHexDump(hex, $"COM{from} -> COM{to}", indent);
            Out(hexd);
            try
            {
                byte[] utf = Encoding.UTF8.GetBytes(hexd);
                log?.Write(utf, 0, utf.Length);
            }
            catch { }
        }

        private static void ReceivedA(byte[] data)
        {
            comB.Send(data);
            LogHex(data, a_port, b_port);
        }
        
        private static void ReceivedB(byte[] data)
        {
            comA.Send(data);
            LogHex(data, b_port, a_port, indentation);
        }

        private static void ReportLogStatus()
        {
            Out($"Logging {(log == null ? "is off." : $"to: {log.Name}\r\nRemember to use a monospaced font when viewing log files.")}");
        }

        private static void Abort(bool quiet = false)
        {
            comA?.Close();
            comB?.Close();
            if (!quiet) Out("Aborted, not running.");
        }

        private static void Out(string text)
        {
            Util.Out(text);
        }

        private static int ParseNN(this string s)
        {
            return int.TryParse(s.Replace('-', 'A'), out int i) ? i : -1;
        }

        private static string TitleCase(this string str)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str.ToLower());
        }
    }
}
