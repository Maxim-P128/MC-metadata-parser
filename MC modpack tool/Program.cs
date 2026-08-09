using System.IO;

namespace MC_modpack_tool
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var scanner = new FolderScanner();

            scanner.OnErrorFound += ProcessException;
            scanner.OnModFound += PrintModInfo;

            string folderPath = "";

            while (true)
            {
                Console.WriteLine("Enter the path to the folder:");
                while (true)
                {
                    folderPath = Console.ReadLine()!.Trim('"');
                    if (Directory.Exists(folderPath))
                    {
                        Console.WriteLine("Folder is founded");
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Folder not found. Enter again");
                    }
                }

                Console.WriteLine($"\n{"Mod name",-55} │ {"MC Version",-10} │ {"Mod Version",-20} │ {"Loader",-10} │ {"Size",-10}");
                Console.WriteLine(new string('─', 120));

                scanner.ScanFolder(folderPath);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Press enter to reset the console");
                Console.ResetColor();
                Console.ReadLine();
                Console.Clear();
            }
        }

        private static void ProcessException(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();

        }

        private static void PrintModInfo(MinecraftFile mod)
        {

            Console.Write($"{mod.Name, -55} │ ");

            if (mod.MinecraftVersion == "Unknown")
            {
                Console.ForegroundColor= ConsoleColor.Yellow;
                Console.Write($"{"Unknown",-10}");
                Console.ResetColor();
                Console.Write(" │ ");
            }
            else
                Console.Write($"{mod.MinecraftVersion,-10} │ ");


            if (mod.ModVersion == "Unknown")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"{"Unknown", -20}");
                Console.ResetColor();
                Console.Write(" │ ");
            }
            else
                Console.Write($"{mod.ModVersion,-20} │ ");


            Console.Write($"{mod.Loader,-10} │ ");
            if(mod.IsCorrupted == true)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("CORRUPTED");
                Console.ResetColor();
            }
            else
                Console.WriteLine($"{mod.SizeInKb,10:F2} Kb");
        }
    }
}
