using System;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using SixLabors.ImageSharp.Memory;
using Spectre.Console;

class Program
{


    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;
        Console.Clear();

        while (true)
        {
            ShowMenu();
        }
    }

    static void ShowAbout()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("About NeuroCode");
        Console.WriteLine("──────────────────────");
        Console.ResetColor();

        Console.WriteLine("NeuroCode is a simplified Arduino IDE designed for accessibility,");
        Console.WriteLine("especially for neurodivergent learners. It provides a friendlier,");
        Console.WriteLine("easier way to write, convert, and upload Arduino code.");
        Console.WriteLine();
        Console.WriteLine("Developed by: Sigma Baguette");
        Console.WriteLine("Version: 1.0.0");
        Console.WriteLine();
        Console.Write("Press any key to return to the main menu...");
        Console.ReadKey(true);
    }


    public static class GradientPrinter
    {
        public static void ShowGradientText(string input)
        {
            var startColor = new Color(156, 39, 176);  // Purple
            var endColor = new Color(0, 188, 212);     // Sky Blue

            var markup = new Markup(GenerateGradientMarkup(input, startColor, endColor));
            AnsiConsole.Write(markup);
            AnsiConsole.WriteLine(); // For newline after output
        }

        private static string GenerateGradientMarkup(string text, Color start, Color end)
        {
            int length = Math.Max(1, text.Length - 1);
            var sb = new System.Text.StringBuilder();

            for (int i = 0; i < text.Length; i++)
            {
                float t = (float)i / length;

                byte r = (byte)(start.R + (end.R - start.R) * t);
                byte g = (byte)(start.G + (end.G - start.G) * t);
                byte b = (byte)(start.B + (end.B - start.B) * t);

                var color = new Color(r, g, b);
                sb.Append($"[{color.ToMarkup()}]{text[i]}[/]");
            }

            return sb.ToString();
        }
    }



    static int Interpolate(int start, int end, int step, int totalSteps)
    {
        if (totalSteps <= 1) return start;
        return start + ((end - start) * step) / (totalSteps - 1);
    }


    public static void ShowMenu()
    {
        Console.Clear();
        GradientPrinter.ShowGradientText(" ██████   █████                                          █████████               █████         \r\n░░██████ ░░███                                          ███░░░░░███             ░░███          \r\n ░███░███ ░███   ██████  █████ ████ ████████   ██████  ███     ░░░   ██████   ███████   ██████ \r\n ░███░░███░███  ███░░███░░███ ░███ ░░███░░███ ███░░███░███          ███░░███ ███░░███  ███░░███\r\n ░███ ░░██████ ░███████  ░███ ░███  ░███ ░░░ ░███ ░███░███         ░███ ░███░███ ░███ ░███████ \r\n ░███  ░░█████ ░███░░░   ░███ ░███  ░███     ░███ ░███░░███     ███░███ ░███░███ ░███ ░███░░░  \r\n █████  ░░█████░░██████  ░░████████ █████    ░░██████  ░░█████████ ░░██████ ░░████████░░██████ \r\n░░░░░    ░░░░░  ░░░░░░    ░░░░░░░░ ░░░░░      ░░░░░░    ░░░░░░░░░   ░░░░░░   ░░░░░░░░  ░░░░░░  ");

        Console.ResetColor();
        Console.WriteLine("\nUse ⬆️ and ⬇️ to navigate and press \u001b[36mEnter/Return\u001b[0m to select:");

        (int left, int top) = Console.GetCursorPosition();
        int option = 1;
        string decorator = "✅ \u001b[36m";
        ConsoleKeyInfo key;
        bool isSelected = false;

        while (!isSelected)
        {
            Console.SetCursorPosition(left, top);
            Console.WriteLine($"{(option == 1 ? decorator : "   ")}Create New Project\u001b[0m");
            Console.WriteLine($"{(option == 2 ? decorator : "   ")}About\u001b[0m");
            Console.WriteLine($"{(option == 3 ? decorator : "   ")}Exit\u001b[0m");
            Console.WriteLine($"{(option == 4 ? decorator : "   ")}Import .ino file\u001b[0m");


            key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    option = option == 1 ? 4 : option - 1;
                    break;
                case ConsoleKey.DownArrow:
                    option = option == 4 ? 1 : option + 1;

                    break;
                case ConsoleKey.Enter:
                    isSelected = true;
                    break;
            }
        }
        static string ArduinoToSimplified(string line)
        {
            line = line.Trim();

            return line switch
            {
                "void setup() {" => "onstart",
                "void loop() {" => "repeat",
                "pinMode(13, OUTPUT);" => "set 13 to output",
                "digitalWrite(13, HIGH);" => "turn on 13",
                "digitalWrite(13, LOW);" => "turn off 13",
                "delay(1000);" => "wait 1000 ms",
                "}" => "end",
                _ => $"// Unknown or unsupported: {line}"
            };
        }
        static string SimplifiedToArduino(string line)
        {
            // Example translation logic (you can expand this)
            if (line.StartsWith("blink led"))
            {
                return "digitalWrite(LED_BUILTIN, HIGH); delay(1000); digitalWrite(LED_BUILTIN, LOW); delay(1000);";
            }
            else if (line.StartsWith("set pin"))
            {
                var parts = line.Split(' ');
                if (parts.Length >= 4)
                {
                    string pin = parts[2];
                    string state = parts[3].ToLower() == "high" ? "HIGH" : "LOW";
                    return $"digitalWrite({pin}, {state});";
                }
            }

            // Unknown or not supported yet
            return $"// Unknown command: {line}";
        }


        static void AutoCompileAndUpload(string inoFilePath, string portName = "COM3", string boardFQBN = "arduino:avr:uno")
        {
            string sketchFolder = Path.GetDirectoryName(inoFilePath);
            string inoFileName = Path.GetFileName(inoFilePath);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n⚙️  Compiling {inoFileName}...");
            Console.ResetColor();

            ProcessStartInfo compileInfo = new ProcessStartInfo
            {
                FileName = "arduino-cli",
                Arguments = $"compile --fqbn {boardFQBN} \"{sketchFolder}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process compileProcess = Process.Start(compileInfo))
            {
                string output = compileProcess.StandardOutput.ReadToEnd();
                string errors = compileProcess.StandardError.ReadToEnd();
                compileProcess.WaitForExit();

                if (compileProcess.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Compilation Successful!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Compilation Failed:\n" + errors);
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📤 Uploading to board...");
            Console.ResetColor();

            ProcessStartInfo uploadInfo = new ProcessStartInfo
            {
                FileName = "arduino-cli",
                Arguments = $"upload -p {portName} --fqbn {boardFQBN} \"{sketchFolder}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process uploadProcess = Process.Start(uploadInfo))
            {
                string output = uploadProcess.StandardOutput.ReadToEnd();
                string errors = uploadProcess.StandardError.ReadToEnd();
                uploadProcess.WaitForExit();

                if (uploadProcess.ExitCode == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Upload Successful!");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ Upload Failed:\n" + errors);
                }
            }

            Console.ResetColor();
        }

        static void CompileCode(List<string> output)
        {
            Console.WriteLine("\n🔧 Compiling...");
            Thread.Sleep(1000);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Compilation complete! Arduino output:");
            Console.ResetColor();

            Console.WriteLine("\n--- Arduino Code Preview ---\n");
            foreach (var line in output)
            {
                Console.WriteLine(line);
            }

            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey(true);
        }

        static void LaunchEditor(List<string> inputLines, List<string> output)
        {
            Console.Clear();
            Console.WriteLine("🎯 You can now enter NeuroCode lines. Type 'compile' to finish, 'back' to remove the last line, or 'exit' to cancel.\n");

            int currentLine = inputLines.Count;
            int lineNumberPadding = 3;

            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{(currentLine + 1).ToString().PadLeft(lineNumberPadding)}]");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write(" > ");
                Console.ResetColor();

                string? line = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                if (line.ToLower() == "exit")
                {
                    Console.WriteLine("🚪 Exiting without compiling...");
                    break;
                }
                if (line?.ToLower() == "help")
                {
                    HelpCenter.Show();
                    Console.WriteLine("\n[Press Enter to return to the editor...]");
                    Console.ReadLine();
                    continue;
                }

                if (line.ToLower() == "back")
                {
                    if (currentLine > 0)
                    {
                        currentLine--;
                        inputLines.RemoveAt(inputLines.Count - 1);
                        output.RemoveAt(output.Count - 1);
                        Console.WriteLine("↩️  Removed the last line.");
                    }
                    continue;
                }

                if (line.ToLower() == "compile")
                {
                    Console.WriteLine("Enter path to the folder where you want to save your file");
                    var Sigma = Console.ReadLine();
                    AutoCompileAndUpload(Sigma);
                    break;
                }

                inputLines.Add(line);
                currentLine++;

                string translated = SimplifiedToArduino(line);
                output.Add(translated);

                if (translated.StartsWith("// Unknown command:"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("⚠️  Unknown command. This line will be ignored in Arduino output.");
                    Console.ResetColor();
                }
            }
        }


        Console.Clear();
        switch (option)
        {
            case 1:
                CreateNewProject();
                break;
            case 2:
                ShowAbout();
                break;
            case 3:
                Console.WriteLine("\u001b[31mExiting program... Goodbye!\u001b[0m");
                Environment.Exit(0);
                break;
            case 4:
                Console.Clear();
                Console.Write("📂 Enter path to the .ino file to import: ");
                string? path = Console.ReadLine();

                if (!File.Exists(path))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("❌ File not found.");
                    Console.ResetColor();
                    Thread.Sleep(1500);
                    break;
                }

                List<string> arduinoLines = File.ReadAllLines(path).ToList();
                List<string> neuroLines = new();
                List<string> output = new();

                foreach (var line in arduinoLines)
                {
                    string simplified = ArduinoToSimplified(line.Trim());
                    neuroLines.Add(simplified);
                    output.Add(line);
                }

                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ File imported and translated to NeuroCode.");
                Console.ResetColor();
                Thread.Sleep(1000);

                LaunchEditor(neuroLines, output);
                break;

        }
    }

    static void CreateNewProject()
    {
        Console.Clear();
        Console.CursorVisible = false;

        int duration = new Random().Next(1000, 3001);
        int elapsed = 0;
        int interval = 500;

        string baseText = "⚙️  Creating new project";
        string[] dots = { ".", "..", "..." };

        while (elapsed < duration)
        {
            foreach (var dot in dots)
            {
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{baseText}{dot}   ");
                Console.ResetColor();
                Thread.Sleep(interval);
                elapsed += interval;
                if (elapsed >= duration)
                    break;
            }
        }

        Console.Clear();

        // 🔧 Your custom project setup logic here
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("✅ New project created successfully!");
        Console.ResetColor();

        List<string> output = new List<string>();
        List<string> inputLines = new List<string>();
        int currentLine = 0;

        // Setup console UI and interaction
        SetupConsole();
        int lineNumberPadding = 3; // Adjust based on expected number of lines


        while (true)
        {
            // Display the input prompt with line numbers and user-friendly commands
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[{(currentLine + 1).ToString().PadLeft(lineNumberPadding)}]");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(" > ");
            Console.ResetColor();

            string line = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(line)) continue;

            // Exit the program when the user types "exit"
            if (line.ToLower() == "exit") break;

            // Handle "back" command to remove previous line
            if (line.ToLower() == "back")
            {
                if (currentLine > 0)
                {
                    currentLine--;
                    inputLines.RemoveAt(inputLines.Count - 1);
                    output.RemoveAt(output.Count - 1);
                    Console.WriteLine("Went back one line.");
                }
                continue;
            }
            if (line.ToLower() == "help")
            {
                HelpCenter.Show();
                Console.WriteLine("\n[Press Enter to return to the editor...]");
                Console.ReadLine();
                continue;
            }
            // Handle "compile" command to generate the Arduino file
            if (line.ToLower() == "compile")
            {
                CompileCode(output);
                break;
            }

            inputLines.Add(line);
            currentLine++;
            string translated = SimplifiedToArduino(line);
            output.Add(translated);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.ResetColor();

            if (translated.StartsWith("// Unknown command:"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unknown command. This will be skipped in Arduino.");
                Console.ResetColor();
            }
        }

        static void SetupConsole()
        {
            Console.Title = "Simple Arduino IDE";
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine(" _   _                       ____          _      \r\n| \\ | | ___ _   _ _ __ ___  / ___|___   __| | ___ \r\n|  \\| |/ _ \\ | | | '__/ _ \\| |   / _ \\ / _` |/ _ \\\r\n| |\\  |  __/ |_| | | | (_) | |__| (_) | (_| |  __/\r\n|_| \\_|\\___|\\__,_|_|  \\___/ \\____\\___/ \\__,_|\\___|");
            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine("Commands:");
            Console.WriteLine("  compile  - generate Arduino file");
            Console.WriteLine("  back     - go back one line");
            Console.WriteLine("  exit     - quit the program");
            Console.WriteLine("  help     - show list of commands and uses");
            Console.ResetColor();
            Console.WriteLine();
        }

        // Convert the simplified command into Arduino C++ code
        static string SimplifiedToArduino(string line)
        {
            string lowerLine = line.ToLower();
            Match match;


            if ((match = Regex.Match(lowerLine, "^say \"(.*)\"$")).Success)
                return $"Serial.println(\"{match.Groups[1].Value.Replace("\"", "\\\"")}\");";
            if ((match = Regex.Match(lowerLine, "^turn on pin (\\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, HIGH);";
            if ((match = Regex.Match(lowerLine, "^turn off pin (\\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, LOW);";
            if ((match = Regex.Match(lowerLine, "^set pin (\\d+) to output$")).Success)
                return $"pinMode({match.Groups[1]}, OUTPUT);";
            if ((match = Regex.Match(lowerLine, "^set pin (\\d+) to input$")).Success)
                return $"pinMode({match.Groups[1]}, INPUT);";
            if ((match = Regex.Match(lowerLine, "^wait (\\d+) second[s]?$")).Success)
                return $"delay({int.Parse(match.Groups[1].Value) * 1000});";
            if ((match = Regex.Match(lowerLine, "^wait (\\d+) millisecond[s]?$")).Success)
                return $"delay({match.Groups[1]});";
            if ((match = Regex.Match(lowerLine, "^analog write pin (\\d+) to (\\d+)$")).Success)
                return $"analogWrite({match.Groups[1]}, {match.Groups[2]});";
            if ((match = Regex.Match(lowerLine, "^read analog pin (a\\d+)$")).Success)
                return $"analogRead({match.Groups[1].Value.ToUpper()});";
            if ((match = Regex.Match(lowerLine, "^read digital pin (\\d+)$")).Success)
                return $"digitalRead({match.Groups[1]});";

            // Handle if condition
            if ((match = Regex.Match(lowerLine, "^if pin (\\d+) is (pressed|high|low)$")).Success)
                return $"if (digitalRead({match.Groups[1]}) == {(match.Groups[2].Value == "low" ? "LOW" : "HIGH")}) {{";

            // Handle other commands like loops, servo, and hydropump
            return HandleExtendedCommands(lowerLine);
        }

        // Handle extended commands like servo motors, hydropump, math functions, etc.
        static string HandleExtendedCommands(string lowerLine)
        {
            Match match;

            if (lowerLine == "include servo library")
                return "#include <Servo.h>";
            if ((match = Regex.Match(lowerLine, @"^create servo called (\w+)$")).Success)
                return $"Servo {match.Groups[1]};";
            if ((match = Regex.Match(lowerLine, @"^attach (\w+) to pin (\d+)$")).Success)
                return $"{match.Groups[1]}.attach({match.Groups[2]});";
            if ((match = Regex.Match(lowerLine, @"^move (\w+) to (\d+)$")).Success)
                return $"{match.Groups[1]}.write({match.Groups[2]});";
            if ((match = Regex.Match(lowerLine, @"^turn on pin (\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, HIGH);";

            if ((match = Regex.Match(lowerLine, @"^turn off pin (\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, LOW);";

            if ((match = Regex.Match(lowerLine, @"^toggle pin (\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, !digitalRead({match.Groups[1]}));";

            if ((match = Regex.Match(lowerLine, @"^set pin (\d+) as input$")).Success)
                return $"pinMode({match.Groups[1]}, INPUT);";

            if ((match = Regex.Match(lowerLine, @"^set pin (\d+) as input pullup$")).Success)
                return $"pinMode({match.Groups[1]}, INPUT_PULLUP);";

            if ((match = Regex.Match(lowerLine, @"^set pin (\d+) as output$")).Success)
                return $"pinMode({match.Groups[1]}, OUTPUT);";
            if ((match = Regex.Match(lowerLine, @"^read analog from pin (\d+)$")).Success)
                return $"analogRead({match.Groups[1]});";

            if ((match = Regex.Match(lowerLine, @"^write analog (\d+) to pin (\d+)$")).Success)
                return $"analogWrite({match.Groups[2]}, {match.Groups[1]});";
            if ((match = Regex.Match(lowerLine, @"^start serial at (\d+)$")).Success)
                return $"Serial.begin({match.Groups[1]});";

            if ((match = Regex.Match(lowerLine, @"^print line ""(.+)""$")).Success)
                return $"Serial.println(\"{match.Groups[1]}\");";

            if ((match = Regex.Match(lowerLine, @"^print line (\w+)$")).Success)
                return $"Serial.println({match.Groups[1]});";
            if ((match = Regex.Match(lowerLine, @"^wait (\d+)$")).Success)
                return $"delay({match.Groups[1]});";

            if ((match = Regex.Match(lowerLine, @"^wait for (\d+) seconds$")).Success)
                return $"delay({match.Groups[1]} * 1000);";

            if ((match = Regex.Match(lowerLine, @"^set timer to current millis$")).Success)
                return $"unsigned long timer = millis();";

            if ((match = Regex.Match(lowerLine, @"^if millis minus timer is greater than (\d+)$")).Success)
                return $"if (millis() - timer > {match.Groups[1]}) {{";
            if ((match = Regex.Match(lowerLine, @"^set (\w+) to (\d+)$")).Success)
                return $"int {match.Groups[1]} = {match.Groups[2]};";

            if ((match = Regex.Match(lowerLine, @"^change (\w+) by (\d+)$")).Success)
                return $"{match.Groups[1]} += {match.Groups[2]};";
            if ((match = Regex.Match(lowerLine, @"^if (\w+) is equal to (\d+)$")).Success)
                return $"if ({match.Groups[1]} == {match.Groups[2]}) {{";

            if ((match = Regex.Match(lowerLine, @"^if (\w+) is greater than (\d+)$")).Success)
                return $"if ({match.Groups[1]} > {match.Groups[2]}) {{";

            if ((match = Regex.Match(lowerLine, @"^if (\w+) is less than (\d+)$")).Success)
                return $"if ({match.Groups[1]} < {match.Groups[2]}) {{";

            if ((match = Regex.Match(lowerLine, @"^else$")).Success)
                return "} else {";

            if ((match = Regex.Match(lowerLine, @"^end$")).Success)
                return "}";

            if ((match = Regex.Match(lowerLine, @"^repeat (\d+) times$")).Success)
                return $"for (int i = 0; i < {match.Groups[1]}; i++) {{";

            if ((match = Regex.Match(lowerLine, @"^forever$")).Success)
                return "while (true) {";
            if ((match = Regex.Match(lowerLine, @"^(\w+) and (\w+)$")).Success)
                return $"({match.Groups[1]} && {match.Groups[2]})";

            if ((match = Regex.Match(lowerLine, @"^(\w+) or (\w+)$")).Success)
                return $"({match.Groups[1]} || {match.Groups[2]})";

            if ((match = Regex.Match(lowerLine, @"^not (\w+)$")).Success)
                return $"!{match.Groups[1]}";
            if ((match = Regex.Match(lowerLine, @"^absolute of (\w+)$")).Success)
                return $"abs({match.Groups[1]});";

            if ((match = Regex.Match(lowerLine, @"^square root of (\w+)$")).Success)
                return $"sqrt({match.Groups[1]});";
            if ((match = Regex.Match(lowerLine, @"^read button on pin (\d+)$")).Success)
                return $"digitalRead({match.Groups[1]});";

            if ((match = Regex.Match(lowerLine, @"^while button on pin (\d+) is pressed$")).Success)
                return $"while (digitalRead({match.Groups[1]}) == HIGH) {{";
            if ((match = Regex.Match(lowerLine, @"^create list (\w+) with size (\d+)$")).Success)
                return $"int {match.Groups[1]}[{match.Groups[2]}];";

            if ((match = Regex.Match(lowerLine, @"^set (\w+)\[(\d+)\] to (\d+)$")).Success)
                return $"{match.Groups[1]}[{match.Groups[2]}] = {match.Groups[3]};";
            if ((match = Regex.Match(lowerLine, @"^play tone on pin (\d+) at (\d+) hz for (\d+) ms$")).Success)
                return $"tone({match.Groups[1]}, {match.Groups[2]}, {match.Groups[3]});";
            if ((match = Regex.Match(lowerLine, @"^define function (\w+)$")).Success)
                return $"void {match.Groups[1]}() {{";

            if ((match = Regex.Match(lowerLine, @"^call (\w+)$")).Success)
                return $"{match.Groups[1]}();";


            // Hydropump relay control
            if ((match = Regex.Match(lowerLine, @"^set pin (\d+) as pump output$")).Success)
                return $"pinMode({match.Groups[1]}, OUTPUT);";
            if ((match = Regex.Match(lowerLine, @"^turn on hydropump at pin (\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, HIGH);";
            if ((match = Regex.Match(lowerLine, @"^turn off hydropump at pin (\d+)$")).Success)
                return $"digitalWrite({match.Groups[1]}, LOW);";

            // Math and Random functionsmath and random funtion math and random functions math n
            if ((match = Regex.Match(lowerLine, @"^map (\w+) from (\d+)-(\d+) to (\d+)-(\d+)$")).Success)
                return $"map({match.Groups[1]}, {match.Groups[2]}, {match.Groups[3]}, {match.Groups[4]}, {match.Groups[5]});";
            if ((match = Regex.Match(lowerLine, @"^random number between (\d+) and (\d+)$")).Success)
                return $"random({match.Groups[1]}, {match.Groups[2]});";

            // Time and interrupts
            if ((match = Regex.Match(lowerLine, @"^if time passed is more than (\d+)ms$")).Success)
                return $"if (millis() > {match.Groups[1]}) {{";
            if ((match = Regex.Match(lowerLine, @"^current time in milliseconds$")).Success)
                return $"millis();";
            if ((match = Regex.Match(lowerLine, @"^stop interrupts$")).Success)
                return "noInterrupts();";
            if ((match = Regex.Match(lowerLine, @"^start interrupts$")).Success)
                return "interrupts();";

            return "// Unknown command: " + lowerLine;
        }

        static string CompileCode(List<string> output)
        {
            Console.Write("\nEnter filename to save (without .ino): ");
            string fileName = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(fileName)) fileName = "output";

            Console.Write("Choose directory to save (leave blank for current folder): ");
            string dir = Console.ReadLine();
            Console.WriteLine("When a prompt appears, please press OK. Press any key to continue...");
            Console.ReadKey();
            if (string.IsNullOrWhiteSpace(dir)) dir = Directory.GetCurrentDirectory();

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string fullPath = Path.Combine(dir, fileName + ".ino");
            File.WriteAllLines(fullPath, output);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ Translation complete! File saved as: {fullPath}");
            Console.ResetColor();

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "arduino", // Or full path: @"C:\Program Files (x86)\Arduino\arduino.exe"
                    Arguments = $"\"{fullPath}\"",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Could not open Arduino IDE: " + ex.Message);
                Console.ResetColor();

                //open the .ino file in default editor
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = fullPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception inner)
                {
                    Console.WriteLine("Could not open file: " + inner.Message);
                }
            }

            //Wait for IDE to launch
            Thread.Sleep(15000);
            Console.Clear();
            return fullPath;
        }

    }


    public static class HelpCenter
    {
        private static readonly Dictionary<string, List<HelpEntry>> HelpCategories = new()
        {
            ["Control Flow"] = new()
        {
            new("if", "if [condition] then", "if button_pressed then"),
            new("else", "else", "else"),
            new("repeat", "repeat [times] times", "repeat 3 times"),
            new("while", "while [condition]", "while temperature > 30")
        },
            ["Functions"] = new()
        {
            new("define", "define function [name]", "define function blink"),
            new("call", "call [function_name]", "call blink")
        },
            ["Sensors"] = new()
        {
            new("read", "read [sensor_name]", "read temperature"),
            new("button", "button [pin]", "button A")
        },
            ["LEDs"] = new()
        {
            new("led", "led [pin] [on/off]", "led 13 on"),
            new("rgb", "rgb [pin] [r,g,b]", "rgb 9 255,0,0")
        },
            ["Sound"] = new()
        {
            new("tone", "tone [pin] [frequency] [duration]", "tone 8 440 500"),
            new("noTone", "noTone [pin]", "noTone 8")
        },
            ["Timers"] = new()
        {
            new("wait", "wait [milliseconds]", "wait 1000"),
            new("delay", "delay [ms]", "delay 500")
        }
        };

        public static void Show()
        {
            var selected = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[cyan bold]Select a category to view help:[/]")
                    .PageSize(7)
                    .AddChoices(HelpCategories.Keys)
            );

            var entries = HelpCategories[selected];

            var table = new Table()
                .Title($"[bold lime]Help: {selected} Commands[/]")
                .AddColumn("[deepskyblue1]Command[/]")
                .AddColumn("[gold1]Syntax[/]")
                .AddColumn("[orchid]Example[/]");

            foreach (var entry in entries)
            {
                table.AddRow(
    $"[aqua]{Markup.Escape(entry.Command)}[/]",
    $"[lightgoldenrod1]{Markup.Escape(entry.Syntax)}[/]",
    $"[mediumorchid1]{Markup.Escape(entry.Example)}[/]"
);
            }

            AnsiConsole.Write(table);
        }
    }




    public record HelpEntry(string Command, string Syntax, string Example);
}



