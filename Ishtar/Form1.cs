using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using Newtonsoft.Json;
using QueenIO;
using QueenIO.Tables;

namespace Ishtar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (File.Exists("Settings.json"))
            {
                Settings.Read("Settings.json");
                TB_ModsPath.Text = settings.Mods_Path;
            }
            else
                settings = new Settings();
        }
        static Settings settings;

        private void B_GetModsPath_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    TB_ModsPath.Text = dialog.FileName;
                    settings.Mods_Path = dialog.FileName;
                    Settings.Save();
                }
            }
        }

        class Settings
        {
            public string Mods_Path { get; set; }

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

        private void Merge_Tables_Click(object sender, EventArgs e)
        {
            string[] paks = Directory.GetFiles(TB_ModsPath.Text, "*.pak", SearchOption.AllDirectories);
            string[] MergableFiles = Directory.GetFiles("Tables", "*.json");
            richTextBox1.AppendText($"Found {paks.Length} pak files\n");
            richTextBox1.AppendText($"Found {MergableFiles.Length} base tables for mergeing\n");
            if (Directory.Exists("Merged"))
                Directory.Delete("Merged", true);
            if (Directory.Exists("ZZZZZ-MergePatch"))
                Directory.Delete("ZZZZZ-MergePatch", true);
            richTextBox1.Clear();

            foreach (string pak in paks)
            {
                if (Directory.Exists("Temp"))
                    Directory.Delete("Temp", true);
                if (Path.GetFileName(pak) == "ZZZZZ-MergePatch_P.pak")
                    continue;

                richTextBox1.AppendText($"Checking {Path.GetFileName(pak)} for Data Tables...\n");
                string[] list = ListPak(pak);
                if (list.Any(x => MergableFiles.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                {
                    var uassetList = list.Where(x => MergableFiles.Any(y => Path.GetFileName(y).Replace(".json", ".uasset") == Path.GetFileName(x))).ToList();

                    ExtractPak(pak);
                    foreach (string file in uassetList)
                    {
                        if (!Directory.Exists("Merged"))
                            Directory.CreateDirectory("Merged");

                        string fullPath = $"{Directory.GetCurrentDirectory()}\\Temp\\{file}";

                        if (File.Exists($"Merged\\{Path.GetFileNameWithoutExtension(file)}.json"))
                            MergeFiles($"Merged\\{Path.GetFileNameWithoutExtension(file)}.json", fullPath);
                        else
                            MergeFiles($"Tables\\{Path.GetFileNameWithoutExtension(file)}.json", fullPath);
                    }
                    Directory.Delete("Temp", true);
                    continue;
                }
                else
                {
                    continue;
                }
            }
            if (Directory.Exists("ZZZZZ-MergePatch"))
            {
                MakePak("ZZZZZ-MergePatch");
                richTextBox1.AppendText($"Created Pak in {TB_ModsPath.Text}\\ZZZZZ-MergedPatch\\ZZZZZ-MergePatchP.pak");
            }
            richTextBox1.AppendText("\nDone.");
        }

        private void MergeFiles(string basetable, string infile)
        {
            Relic relic = new Relic();
            relic = Blood.Open(infile);
            richTextBox1.AppendText($"Merging {Path.GetFileName(infile)}...\n");
            string outpath = $"ZZZZZ-MergePatch\\CodeVein\\Content\\{infile.Substring(infile.LastIndexOf("Temp\\")).Replace("Temp\\","")}";
            if (!Directory.Exists(Path.GetDirectoryName(outpath)))
                Directory.CreateDirectory(Path.GetDirectoryName(outpath));
            if (basetable.Contains("DT_AccessoryPreset"))
            {
                AccessoryListData accessoryList = new AccessoryListData();
                accessoryList = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(basetable));
                AccessoryListData accessoryList1 = new AccessoryListData();
                accessoryList1.Read(relic.GetDataTable());
                foreach (AccessoryData accessory in accessoryList1.Accessories)
                {
                    if (!accessoryList.Accessories.Contains(accessoryList.Accessories.FirstOrDefault(x => x.Name == accessory.Name)))
                    {
                        accessoryList.Accessories.Add(accessory);
                    }
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(accessoryList);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(accessoryList.Make());
            }
            else if (basetable.Contains("DT_InnerList"))
            {
                InnerList innerList = new InnerList();
                innerList = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(basetable));
                InnerList innerList1 = new InnerList();
                innerList1.Read(relic.GetDataTable());
                foreach (var inner in innerList1.Inners)
                {
                    if (!innerList.Inners.Contains(innerList.Inners.FirstOrDefault(x => x.Name == inner.Name)))
                        innerList.Inners.Add(inner);
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(innerList);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(innerList.Make());
            }
            else if (basetable.Contains("DT_FacePaintMask"))
            {
                FacePaintList facePaintList = new FacePaintList();
                FacePaintList facePaintList1 = new FacePaintList();
                facePaintList = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(basetable));
                facePaintList1 = new FacePaintList();
                facePaintList1.Read(relic.GetDataTable());
                foreach (var facepaint in facePaintList1.FacePaints)
                {
                    if (!facePaintList.FacePaints.Contains(facePaintList.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name)))
                        facePaintList.FacePaints.Add(facepaint);
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(facePaintList);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(facePaintList.Make());
            }
            else if (basetable.Contains("DT_HairList"))
            {
                HairListData hairListData = new HairListData();
                HairListData hairListData1 = new HairListData();
                hairListData = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(basetable));
                hairListData1 = new HairListData();
                hairListData1.Read(relic.GetDataTable());
                foreach( var hair in hairListData1.HairDataList)
                {
                    if (!hairListData.HairDataList.Contains(hairListData.HairDataList.FirstOrDefault(x => x.Name == hair.Name)))
                        hairListData.HairDataList.Add(hair);
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(hairListData);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(hairListData.Make());
            }
            else if (basetable.Contains("DT_InnerFrame") || basetable.Contains("DT_OuterMask"))
            {
                MaskListData maskListData = new MaskListData();
                MaskListData maskListData1 = new MaskListData();
                maskListData = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(basetable));
                maskListData1 = new MaskListData();
                maskListData1.Read(relic.GetDataTable());
                foreach (var mask in maskListData1.Masks)
                {
                    if (!maskListData.Masks.Contains(maskListData.Masks.FirstOrDefault(x => x.Name == mask.Name)))
                        maskListData.Masks.Add(mask);
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(maskListData);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(maskListData.Make());
            }
            else
            {
                BasicCustomizationListData basicCustomizationListData = new BasicCustomizationListData();
                BasicCustomizationListData basicCustomizationListData1 = new BasicCustomizationListData();
                basicCustomizationListData = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(basetable));
                basicCustomizationListData1 = new BasicCustomizationListData();
                basicCustomizationListData1.Read(relic.GetDataTable());
                foreach (var basic in basicCustomizationListData1.basicCustomizationDatas)
                {
                    if (!basicCustomizationListData.basicCustomizationDatas.Contains(basicCustomizationListData.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name)))
                        basicCustomizationListData.basicCustomizationDatas.Add(basic);
                }
                string outname = $"Merged\\{Path.GetFileName(basetable)}";
                string json = JsonConvert.SerializeObject(basicCustomizationListData);
                File.WriteAllText(outname, json);
                relic.WriteDataTable(basicCustomizationListData.Make());
            }
            Blood.Save(relic, outpath);
        }

        private void ExtractPak(string infile)
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ExtractPak.bat";
                process.StartInfo.Arguments = $"\"{infile}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                //richTextBox1.AppendText(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            }
        }

        private string[] ListPak(string infile)
        {
            string[] list;
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "UnrealPak.exe";
                process.StartInfo.Arguments = $"\"{infile}\" -list";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
                list = output.Split(new string[] { "\r\n", "\r", "\n"}, StringSplitOptions.None);
            }
            List<string> result = new List<string>();
            foreach (string line in list)
            {
                if (!line.Contains("LogPakFile: Display:"))
                    continue;
                string file = line.Split(' ')[2];
                if (!file.Contains("\""))
                    continue;
                result.Add(file.Replace("\"","").Replace("/","\\"));
            }
            return result.ToArray();
        }

        public void MakePak(string outfile)
        {
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "MakePak.bat";
                process.StartInfo.Arguments = $"\"{outfile}\" \"{TB_ModsPath.Text}\\ZZZZZ-MergedPatch\\{outfile}_P\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                process.WaitForExit();
            }
            File.Delete("filelist.txt");
        }
    }
}
