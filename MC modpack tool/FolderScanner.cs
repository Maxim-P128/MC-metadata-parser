using System;
using System.IO;
using System.IO.Compression;
using System.Text.Json;

namespace MC_modpack_tool
{
    public class FolderScanner
    {
        public event Action<string> OnErrorFound;

        public event Action<MinecraftFile> OnModFound;

        private readonly string[] McVersions = { "1.20.1", "1.16.5", "1.12", "1.12.2" };


        public void ScanFolder(string folderPath)
        {
            try
            {
                string[] jarFiles = Directory.GetFiles(folderPath, "*.jar");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nMods found: {jarFiles.Length}");
                Console.ResetColor();

                foreach (string jarFile in jarFiles)
                {
                    FileInfo fileInfo = new FileInfo(jarFile);
                    bool IsCorrupted = fileInfo.Length == 0;
                    double size = (double)fileInfo.Length / 1024;

                    MinecraftFile mod = new MinecraftFile(fileInfo.Name, jarFile, size, IsCorrupted);

                   ParseModName(mod);

                    ScanJar(mod);

                    OnModFound?.Invoke(mod);
                }
            }
            catch (UnauthorizedAccessException)
            {
                OnErrorFound?.Invoke("The program does not have permission to read this folder");
            }
            catch (Exception ex)
            {
                OnErrorFound?.Invoke($"Unexpected error: {ex.Message}");
            }

        }

        private void ScanJar(MinecraftFile mod)
        {
            using (ZipArchive jarFile = ZipFile.OpenRead(mod.FullPath))
            {

                ZipArchiveEntry modInfo = jarFile.GetEntry("META-INF/mods.toml");
                if (modInfo != null)
                {
                    mod.Loader = "Forge";
                    return;
                }
                modInfo = jarFile.GetEntry("mcmod.info");

                if (modInfo != null)
                {
                     mod.Loader = "Forge";

                     using (Stream stream = modInfo.Open())

                     using (JsonDocument doc = JsonDocument.Parse(stream))
                     {
                        JsonElement root = doc.RootElement;

                        if (root.ValueKind == JsonValueKind.Array)
                        {
                            JsonElement firstElement = root[0];

                            mod.ModVersion = firstElement.GetProperty("version").GetString();
                        }

                        if (root.ValueKind == JsonValueKind.Object)
                        {
                            JsonElement firstElement = root.GetProperty("modList")[0];

                            mod.ModVersion = firstElement.GetProperty("version").GetString();

                            if (firstElement.TryGetProperty("mcversion", out JsonElement mcVersionElement))
                            {
                                mod.MinecraftVersion = mcVersionElement.GetString();
                            }
                        }
                    }
                }
                    return;
            }
        }
        private void ParseModName(MinecraftFile mod)
        {
            for (int i = 0; i < McVersions.Length; i++)
            {
                if (mod.Name.Contains(McVersions[i]))
                    mod.MinecraftVersion = McVersions[i];
            }
        }
    }
}
