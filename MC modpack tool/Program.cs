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

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            string folderPath = "";
            List<MinecraftFile> modsList = new List<MinecraftFile>();

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

            Console.WriteLine($"\n{"Mod name",-60} | {"MC Version",-10} | {"Loader",-10} | {"Size",10}");
            Console.WriteLine(new string('-', 100));

            scanner.Scan(folderPath);

            Console.ReadLine();
        }

        private static void ProcessException(string message)
        {
            Console.WriteLine(message);
        }

        private static void PrintModInfo(MinecraftFile mod)
        {
            if (mod.IsCorrupted)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{mod.Name,-60} | БИТЫЙ ФАЙЛ");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"{mod.Name,-60} | {mod.MinecraftVersion,-10} | {mod.Loader,-10} | {mod.SizeInKb,10:F2} Kb");
        }
    }
}
