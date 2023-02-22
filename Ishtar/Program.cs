using Ishtar.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Ishtar
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
    
    public static class Global
    {
        public static Settings settings;

        public class Settings
        {
            public string Mods_Path { get; set; }
            public Dictionary<string, string> Paks { get; set; } = new Dictionary<string, string>();
            public bool Nest { get; set; }

            public static void Read(string infile)
            {
                string json = File.ReadAllText(infile);
                settings = JsonConvert.DeserializeObject<Settings>(json);
            }

            public static void Save()
            {
                string json = JsonConvert.SerializeObject(settings);
                if (File.Exists("Settings.json"))
                    File.Delete("Settings.json");
                File.WriteAllText("Settings.json", json);
            }
        }
    }
}
