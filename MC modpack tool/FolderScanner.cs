using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;

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
            try
            {
                using (ZipArchive jarFile = ZipFile.OpenRead(mod.FullPath))
                {


                    ZipArchiveEntry modInfo = jarFile.GetEntry("mcmod.info")!;

                    if (modInfo != null)
                    {
                        mod.Loader = "Forge";
                        ParseForgeJson(mod, modInfo);
                        return;
                    }

                    modInfo = jarFile.GetEntry("fabric.mod.json")!;
                    if (modInfo != null)
                    {
                        mod.Loader = "Fabric";
                        ParseFabricJson(mod, modInfo);
                        return;
                    }
                    modInfo = jarFile.GetEntry("META-INF/mods.toml")!;
                    if (modInfo != null)
                    {
                        mod.Loader = "Forge";
                        return;
                    }
                }
            }
            catch (InvalidDataException) { }
        }
        private void ParseModName(MinecraftFile mod)
        {

            for (int i = 0; i < McVersions.Length; i++)
            {
                if (mod.Name.Contains(McVersions[i]))
                    mod.MinecraftVersion = McVersions[i];
            }
        }
        private void ParseForgeJson(MinecraftFile mod, ZipArchiveEntry modInfo)
        {
            try
            {
                using (Stream stream = modInfo.Open())
                using (StreamReader sr = new StreamReader(stream))
                using (JsonTextReader reader = new JsonTextReader(sr))
                {
                    JToken root = JToken.Load(reader);

                    if (root.Type == JTokenType.Array && root.HasValues)
                    {
                        JToken firstElement = root[0]!;

                        string? modVersion = (string?)firstElement!["version"];
                        if (modVersion != null && modVersion != "${version}")
                        {
                            mod.ModVersion = modVersion;
                        }

                        string? mcVersion = (string?)firstElement["mcversion"];
                        if (mcVersion != null && mcVersion != "${mcversion}")
                        {
                            mod.MinecraftVersion = mcVersion.Split(',')[0];
                        }
                    }

                    if (root.Type == JTokenType.Object)
                    {
                        JToken? modList = root["modList"];
                        if (modList != null && modList.Type == JTokenType.Array && modList.HasValues)
                        {
                            JToken firstElement = modList[0]!;

                            string? modVersion = (string?)firstElement!["version"];
                            if (modVersion != null && modVersion != "${version}")
                            {
                                mod.ModVersion = modVersion;
                            }

                            string? mcVersion = (string?)firstElement["mcversion"];
                            if (mcVersion != null && mcVersion != "${mcversion}")
                            {
                                mod.MinecraftVersion = mcVersion.Split(',')[0];
                            }
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                OnErrorFound?.Invoke($"Failed to parse JSON of {mod.Name}: {ex.Message}");
            }
        }

        private void ParseFabricJson(MinecraftFile mod, ZipArchiveEntry modInfo)
        {
            using (Stream stream = modInfo.Open())
            using (StreamReader sr = new StreamReader(stream))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                JToken root = JToken.Load(reader);

                string? modVersion = (string?)root["version"];
                if (modVersion != null && modVersion != "${version}")
                {
                    mod.ModVersion = modVersion.Split('+')[0];
                }

                string? mcVersion = (string?)root["mcversion"];
                if (mcVersion != null && mcVersion != "${mcversion}")
                {
                    mod.MinecraftVersion = mcVersion.Split(',')[0];
                }
            }
        }
    }
}
