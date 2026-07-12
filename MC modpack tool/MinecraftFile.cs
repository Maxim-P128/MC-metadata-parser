using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MC_modpack_tool
{
    public class MinecraftFile
    {
        public string Name { get; set; }
        public string FullPath { get; set; }
        public double SizeInKb { get; set; }
        public bool IsCorrupted { get; set; }
        public string MinecraftVersion { get; set; }
        public string ModVersion { get; set; }
        public string Loader { get; set; }

        public MinecraftFile(string name, string fullPath, double sizeInKb, bool isCorrupted)        
        { 
        Name = name;
        FullPath = fullPath;
        SizeInKb = sizeInKb; 
        IsCorrupted = isCorrupted;
        Loader = "Unknown";
        MinecraftVersion = "Unknown";
        ModVersion = "Unknown";
        }

    }
}
