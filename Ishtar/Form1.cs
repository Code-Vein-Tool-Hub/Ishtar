﻿using System;
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
using System.Reflection;

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
                TSB_Nested.Checked = Global.settings.Nest;
                TSB_PartialMerge.Checked = Global.settings.partial;
                TSB_Patch.Checked = Global.settings.patchMerge;
                TSB_Scan.Checked = Global.settings.scanMerge;
                parallelScaningToolStripMenuItem.Checked = Global.settings.parallel;
                makeIshtarPatchAfterScanToolStripMenuItem.Checked = Global.settings.MakePatch;
            }
            else
                Global.settings = new Global.Settings();
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
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                Merge();
                stopWatch.Stop();

                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Helpers.Log("richTextBox1", $"Merging took {elapsedTime}");

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
            Merger.Merge(paks, MergableFiles, TSB_PartialMerge.Checked);

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
            string outpath = $"{TB_ModsPath.Text}\\ZZZZZ-MergedPatch\\{outfile}_P";
            if (!TSB_Nested.Checked)
            {
                outpath = $"{TB_ModsPath.Text}\\{outfile}_P";
            }

            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "MakePak.bat";
                process.StartInfo.Arguments = $"\"{outfile}\" \"{outpath}\"";
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                process.WaitForExit();
            }
            File.Delete("filelist.txt");
        }

        private void CreateIshtarPatch_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = Global.settings.Mods_Path;
                ofd.Filter = "Unreal Pak|*.pak";
                if (ofd.ShowDialog() == DialogResult.OK)
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
                        Helpers.LogClear("richTextBox1");
                        IshtarPatch.GenerateIshtarPatch(ofd.FileName);
                        threads--;
                    }).Start();
                }
            }
        }

        private void nestedPak_CheckedChanged(object sender, EventArgs e)
        {
            Global.settings.Nest = TSB_Nested.Checked;
            Global.Settings.Save();
        }

        private void TSB_PartialMerge_Click(object sender, EventArgs e)
        {
            Global.settings.partial = TSB_PartialMerge.Checked;
            Global.Settings.Save();
        }

        private void TSB_Patch_Click(object sender, EventArgs e)
        {
            Global.settings.patchMerge = TSB_Patch.Checked;
            Global.Settings.Save();
        }

        private void TSB_Scan_Click(object sender, EventArgs e)
        {
            Global.settings.scanMerge = TSB_Scan.Checked;
            Global.Settings.Save();
        }

        private void parallelScaningToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.settings.parallel = parallelScaningToolStripMenuItem.Checked;
            Global.Settings.Save();
        }

        private void makeIshtarPatchAfterScanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Global.settings.MakePatch = makeIshtarPatchAfterScanToolStripMenuItem.Checked;
            Global.Settings.Save();
        }
    }
}
