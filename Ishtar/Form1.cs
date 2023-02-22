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
using QueenIO.Structs;
using Ishtar.IO;
using System.Threading;
using QueenIO.Mods;

namespace Ishtar
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (File.Exists("Settings.json"))
            {
                Global.Settings.Read("Settings.json");
                TB_ModsPath.Text = Global.settings.Mods_Path;
            }
            else
                Global.settings = new Global.Settings();
#if DEBUG
            button1.Visible = true;
#endif
        }
        
        private volatile int threads;

        private void MakeTables(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {
                ofd.IsFolderPicker = true;

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    string[] files = Directory.GetFiles(ofd.FileName, "*.uasset");
                    foreach (string file in files)
                    {
                        if (file.Contains("DT_AccessoryPreset"))
                        {
                            Relic relic = Blood.Open(file);
                            AccessoryListData accessoryList = new AccessoryListData();
                            accessoryList.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(accessoryList);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                            relic.WriteDataTable(accessoryList.Make());
                        }
                        else if (file.Contains("DT_InnerPartsVisibilityByOuter"))
                        {
                            Relic relic = Blood.Open(file);
                            InnerPartsVisibilityByOuter innerPartsVisibility = new InnerPartsVisibilityByOuter();
                            innerPartsVisibility.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(innerPartsVisibility);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else if (file.Contains("DT_FacePaint"))
                        {
                            Relic relic = Blood.Open(file);
                            FacePaintList data = new FacePaintList();
                            data.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(data);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else if (file.Contains("DT_InnerList"))
                        {
                            Relic relic = Blood.Open(file);
                            InnerList data = new InnerList();
                            data.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(data);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else if (file.Contains("DT_Hair"))
                        {
                            Relic relic = Blood.Open(file);
                            HairListData data = new HairListData();
                            data.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(data);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else if (file.Contains("DT_InnerFrame") || file.Contains("DT_OuterMask"))
                        {
                            Relic relic = Blood.Open(file);
                            MaskListData data = new MaskListData();
                            data.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(data);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else if (file.Contains("DT_SpawnerList"))
                        {
                            Relic relic = Blood.Open(file);
                            ModControlFrameworkListData data = new ModControlFrameworkListData();
                            data.Read(relic.GetDataTable());
                            string json = JsonConvert.SerializeObject(data);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                        else
                        {
                            Relic relic = Blood.Open(file);
                            BasicCustomizationListData basicCustomizationData = new BasicCustomizationListData();
                            basicCustomizationData.Read(relic.GetDataTable());

                            string json = JsonConvert.SerializeObject(basicCustomizationData);
                            File.WriteAllText($"Output\\{Path.GetFileNameWithoutExtension(file)}.json", json);
                        }
                    }
                }
            }
        }

        private void B_GetModsPath_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog dialog = new CommonOpenFileDialog())
            {
                dialog.IsFolderPicker = true;
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    TB_ModsPath.Text = dialog.FileName;
                    Global.settings.Mods_Path = dialog.FileName;
                    Global.Settings.Save();
                }
            }
        }

        private void Merge_Tables_Click(object sender, EventArgs e)
        {
            if (threads > 0)
            {
                MessageBox.Show("Please wait until all operations are finished.");
                return;
            }
            richTextBox1.Focus();
            new Thread(() =>
            {
                threads++;
                Merge();
                threads--;
            }).Start();
        }

        private void Merge()
        {
            string[] paks = Directory.GetFiles(TB_ModsPath.Text, "*.pak", SearchOption.AllDirectories);
            string[] MergableFiles = Directory.GetFiles("Tables", "*.json");
            Helpers.Log("richTextBox1", $"Found {paks.Length} pak files");
            Helpers.Log("richTextBox1", $"Found {MergableFiles.Length} base tables for mergeing");
            //if (Directory.Exists("Merged"))
            //    Directory.Delete("Merged", true);
            if (Directory.Exists("ZZZZZ-MergePatch"))
                Directory.Delete("ZZZZZ-MergePatch", true);
            Helpers.LogClear("richTextBox1");
            Merger.ListMergeablePaks(paks, MergableFiles);
            Merger.Merge(paks, MergableFiles, checkBox1.Checked);

            //AssetRegister Merge
            Helpers.Log("richTextBox1", $"Starting AssetRegistryMerge");
            AssetRegistryMerger.ListMergeablePaks(paks);
            AssetRegistryMerger.Merge(paks);

            if (Directory.Exists("ZZZZZ-MergePatch"))
            {
                MakePak("ZZZZZ-MergePatch");
                Helpers.Log("richTextBox1", $"Created Pak in {TB_ModsPath.Text}\\ZZZZZ-MergedPatch\\ZZZZZ-MergePatchP.pak");
            }
            Helpers.Log("richTextBox1", "Done.");
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

        private void Testing(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    AssetRegistry assetRegistry = new AssetRegistry();
                    //assetRegistry.Read(File.ReadAllBytes(ofd.FileName));
                    assetRegistry = JsonConvert.DeserializeObject<AssetRegistry>(File.ReadAllText(ofd.FileName));

                    //string json = JsonConvert.SerializeObject(assetRegistry, Formatting.Indented);
                    //File.WriteAllText($"AssetRegistry.json", json);

                    File.WriteAllBytes("AssetRegistry_out.bin", assetRegistry.Make());

                    Helpers.Log("richTextBox1", "\r\nDone");
                    Console.WriteLine();
                }
            }
        }
    }
}
