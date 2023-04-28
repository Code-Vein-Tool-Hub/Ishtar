using Newtonsoft.Json;
using QueenIO;
using QueenIO.Tables;
using QueenIO.Structs;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QueenIO.Mods;
using System;
using System.Collections.Generic;
using UAssetAPI;
using System.Xml.Linq;
using static Ishtar.IO.IshtarPatch;
using Newtonsoft.Json.Linq;

namespace Ishtar.IO
{
    public class Merger
    {
        private class MergeFile
        {
            public string Path { get; set; }
            public Relic relic { get; set; }
            public object Table { get; set; }
            public object VanillaTable { get; set; }
            public types Type { get; set; }

            public enum types
            {
                Accessory,
                Inner,
                FacePaint,
                Hair,
                Mask,
                Visibility,
                Common,
                Mod,
                ProgressSymbols,
            }
        }

        private static ConcurrentDictionary<string, MergeFile> MergeFiles = new ConcurrentDictionary<string, MergeFile>();
        private static ConcurrentDictionary<string, string> LoggedPaks = new ConcurrentDictionary<string, string>();

        public static void ListMergeablePaks(string[] infiles, string[] Tables)
        {
            
            List<string> scanList = new List<string>();

            if (Global.settings.patchMerge)
            {
                Helpers.Log("richTextBox1", $"Filtering mods with Ishtar Patches...");
                foreach (string pak in infiles)
                {
                    if (!File.Exists(Path.ChangeExtension(pak, ".ishtarPatch")))
                        scanList.Add(pak);
                    else
                        LoggedPaks.TryAdd(Path.GetFileNameWithoutExtension(pak), pak);
                }
                Helpers.Log("richTextBox1", $"{LoggedPaks.Count} paks filtered");
            }
            else
            {
                scanList = infiles.ToList();
            }
            
            if (Global.settings.scanMerge)
            {
                Helpers.Log("richTextBox1", $"Scanning Paks for mergeable files...");
                if (Global.settings.parallel == true)
                {
                    Parallel.ForEach(scanList, pak =>
                    {
                        string[] list = Helpers.ListPak(pak);
                        if (list.Any(x => Tables.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                        {
                            bool added = LoggedPaks.TryAdd(Path.GetFileNameWithoutExtension(pak), pak);
                        }
                    });
                }
                else
                {
                    foreach (string pak in scanList)
                    {
                        string[] list = Helpers.ListPak(pak);
                        if (list.Any(x => Tables.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                        {
                            bool added = LoggedPaks.TryAdd(Path.GetFileNameWithoutExtension(pak), pak);
                        }
                    }
                }
            }
            
            Helpers.Log("richTextBox1", $"Found {LoggedPaks.Count} paks for merging");
        }



        public static void Merge(string[] infiles, string[] Tables, bool PartialMerge = false)
        {

            foreach (string pak in LoggedPaks.Values)
            {
                if (File.Exists(Path.ChangeExtension(pak, ".ishtarPatch")) && Global.settings.patchMerge)
                {
                    PatchMerge(pak, PartialMerge);
                }
                else if (Global.settings.scanMerge)
                {
                    ScanMerge(pak, Tables, PartialMerge);
                }
            }
            Export();
        }

        public static void ScanMerge(string pak, string[] Tables, bool PartialMerge = false)
        {
            string staging = Path.GetFileNameWithoutExtension(pak);
            if (Directory.Exists(staging))
                Directory.Delete(staging, true);
            if (Path.GetFileName(pak) == "ZZZZZ-MergePatch_P.pak")
                return;

            Helpers.ExtractPak(pak, staging);
            Helpers.Log("richTextBox1", $"Merging Pak {staging}:");

            string[] list = Directory.GetFiles(staging, "*.*", SearchOption.AllDirectories);
            var uassetList = list.Where(x => Tables.Any(y => Path.GetFileName(y).Replace(".json", ".uasset") == Path.GetFileName(x))).ToList();

            foreach (string file in uassetList)
            {
                if (!Directory.Exists("Merged"))
                    Directory.CreateDirectory("Merged");

                Helpers.Log("richTextBox1", $"    Merging {Path.GetFileName(file)}...");
                string fullpath = $"{Directory.GetCurrentDirectory()}\\{file}";
                Relic relic = new Relic();
                relic = Blood.Open(fullpath);
                string outpath = $"ZZZZZ-MergePatch\\CodeVein\\Content\\{fullpath.Substring(fullpath.LastIndexOf($"{staging}\\")).Replace($"{staging}\\", "")}";
                //if (!Directory.Exists(Path.GetDirectoryName(outpath)))
                //    Directory.CreateDirectory(Path.GetDirectoryName(outpath));

                //check if existing merge exist, use that for the base table instead if so
                string tbl = $"Tables\\{Path.GetFileNameWithoutExtension(file)}.json";
                string name = Path.GetFileNameWithoutExtension(file);

                if (tbl.Contains("DT_AccessoryPreset"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        AccessoryListData data = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                        AccessoryListData vanilla = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)0 });
                    }
                    Accessory((AccessoryListData)MergeFiles[name].Table, (AccessoryListData)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_InnerList"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        InnerList data = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                        InnerList vanilla = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)1 });
                    }
                    Inner((InnerList)MergeFiles[name].Table, (InnerList)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_FacePaintMask"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        FacePaintList data = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                        FacePaintList vanilla = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)2 });
                    }
                    FacePaint((FacePaintList)MergeFiles[name].Table, (FacePaintList)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_HairList"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        HairListData data = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                        HairListData vanilla = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)3 });
                    }
                    Hair((HairListData)MergeFiles[name].Table, (HairListData)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_InnerFrame") || tbl.Contains("DT_OuterMask"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        MaskListData data = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                        MaskListData vanilla = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)4 });
                    }
                    Mask((MaskListData)MergeFiles[name].Table, (MaskListData)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_InnerPartsVisibilityByOuter"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        InnerPartsVisibilityByOuter data = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                        InnerPartsVisibilityByOuter vanilla = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)5 });
                    }
                    Visibility((InnerPartsVisibilityByOuter)MergeFiles[name].Table, (InnerPartsVisibilityByOuter)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_SpawnerList"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        ModControlFrameworkListData data = JsonConvert.DeserializeObject<ModControlFrameworkListData>(File.ReadAllText(tbl));
                        ModControlFrameworkListData vanilla = JsonConvert.DeserializeObject<ModControlFrameworkListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)7 });
                    }
                    ModControl((ModControlFrameworkListData)MergeFiles[name].Table, (ModControlFrameworkListData)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (tbl.Contains("DT_ProgressSymbol"))
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        ProgressSymbol data = JsonConvert.DeserializeObject<ProgressSymbol>(File.ReadAllText(tbl));
                        ProgressSymbol vanilla = JsonConvert.DeserializeObject<ProgressSymbol>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)8 });
                    }
                    ProgressSymbols((ProgressSymbol)MergeFiles[name].Table, (ProgressSymbol)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }
                else
                {
                    if (!MergeFiles.ContainsKey(name))
                    {
                        BasicCustomizationListData data = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                        BasicCustomizationListData vanilla = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)6 });
                    }
                    Common((BasicCustomizationListData)MergeFiles[name].Table, (BasicCustomizationListData)MergeFiles[name].VanillaTable, relic, pak, PartialMerge);
                }

            }
            Directory.Delete(staging, true);

            if (Global.settings.MakePatch)
            {
                IshtarPatch.GenerateIshtarPatch(pak);
                Helpers.Log("richTextBox1", $"Generated new Ishtar Patch for {Path.GetFileName(pak)}");
            }
        }

        public static void PatchMerge(string pak, bool PartialMerge = false)
        {
            IshtarPatch ishtarPatch = JsonConvert.DeserializeObject<IshtarPatch>(File.ReadAllText(Path.ChangeExtension(pak, ".ishtarPatch")));
            Helpers.Log("richTextBox1", $"Merging Pak {Path.GetFileNameWithoutExtension(pak)}:");
            bool makePatchFlag = false;

            //Patch Checks
            if (Helpers.HashFile(File.ReadAllBytes(pak)) != ishtarPatch.Hash)
            {
                Helpers.Log("richTextBox1", $"File Hash mismatch for {Path.GetFileName(pak)}, patch will still merge but it might be missing things.");
                if (Global.settings.MakePatch) makePatchFlag = true;
            }
            if (Path.GetFileName(pak) != ishtarPatch.FileName)
            {
                Helpers.Log("richTextBox1", $"File name mismatch for {Path.GetFileName(pak)}, patch will still merge but might be for the wrong mod");
                if (Global.settings.MakePatch) makePatchFlag = true;
            }

            if (makePatchFlag)
            {
                IshtarPatch.GenerateIshtarPatch(pak);
                Helpers.Log("richTextBox1", $"Generated new Ishtar Patch for {Path.GetFileName(pak)}");
            }

            foreach (IshtarPatch.Table table in ishtarPatch.tables)
            {
                if (!Directory.Exists("Merged"))
                    Directory.CreateDirectory("Merged");
                if (table.Type == types.AssetRegistry)
                    continue;

                Helpers.Log("richTextBox1", $"    Merging {table.Name}...");

                string outpath = $"ZZZZZ-MergePatch\\CodeVein\\Content\\{table.Path.Replace("/Game", "").Replace("/", "\\")}\\{table.Name}.uasset";
                string tbl = $"Tables\\{table.Name}.json";
                Relic relic = new Relic();

                using (var sr = new FileStream($"Tables\\Assets\\{table.Name}.json", FileMode.Open))
                {
                    relic = UAsset.DeserializeJson(sr) as Relic;
                }

                if (table.Type == IshtarPatch.types.Inner)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        InnerList data = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                        InnerList vanilla = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)1 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<InnerList>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Inner((InnerList)MergeFiles[table.Name].Table, (InnerList)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.Accessory)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        AccessoryListData data = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                        AccessoryListData vanilla = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)0 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<AccessoryListData>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Accessory((AccessoryListData)MergeFiles[table.Name].Table, (AccessoryListData)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.FacePaint)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        FacePaintList data = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                        FacePaintList vanilla = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)2 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<FacePaintList>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    FacePaint((FacePaintList)MergeFiles[table.Name].Table, (FacePaintList)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.Hair)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        HairListData data = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                        HairListData vanilla = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)3 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<HairListData>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Hair((HairListData)MergeFiles[table.Name].Table, (HairListData)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.Mask)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        MaskListData data = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                        MaskListData vanilla = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)4 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<MaskListData>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Mask((MaskListData)MergeFiles[table.Name].Table, (MaskListData)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.Visibility)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        InnerPartsVisibilityByOuter data = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                        InnerPartsVisibilityByOuter vanilla = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)5 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Visibility((InnerPartsVisibilityByOuter)MergeFiles[table.Name].Table, (InnerPartsVisibilityByOuter)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.Mod)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        ModControlFrameworkListData data = JsonConvert.DeserializeObject<ModControlFrameworkListData>(File.ReadAllText(tbl));
                        ModControlFrameworkListData vanilla = JsonConvert.DeserializeObject<ModControlFrameworkListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)7 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<ModControlFrameworkListData>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    ModControl((ModControlFrameworkListData)MergeFiles[table.Name].Table, (ModControlFrameworkListData)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else if (table.Type == IshtarPatch.types.ProgressSymbol)
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        ProgressSymbol data = JsonConvert.DeserializeObject<ProgressSymbol>(File.ReadAllText(tbl));
                        ProgressSymbol vanilla = JsonConvert.DeserializeObject<ProgressSymbol>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)8 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<ProgressSymbol>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    ProgressSymbols((ProgressSymbol)MergeFiles[table.Name].Table, (ProgressSymbol)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }
                else
                {
                    if (!MergeFiles.ContainsKey(table.Name))
                    {
                        BasicCustomizationListData data = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                        BasicCustomizationListData vanilla = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                        MergeFiles.TryAdd(table.Name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)6 });
                    }
                    var relic2 = JsonConvert.DeserializeObject<BasicCustomizationListData>(table.DataTable.ToString());
                    relic.WriteDataTable(relic2.Make());
                    Common((BasicCustomizationListData)MergeFiles[table.Name].Table, (BasicCustomizationListData)MergeFiles[table.Name].VanillaTable, relic, pak, PartialMerge);
                }


            }
        }

        private static void Export()
        {
            Helpers.Log("richTextBox1", "Writing all Tables to uassets...");
            foreach (var file in MergeFiles.Values)
            {
                switch (file.Type)
                {
                    case MergeFile.types.Accessory:
                        file.relic.WriteDataTable(((AccessoryListData)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\Accessories\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.Inner:
                        file.relic.WriteDataTable(((InnerList)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\Inners\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.FacePaint:
                        file.relic.WriteDataTable(((FacePaintList)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\FacePaints\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.Hair:
                        file.relic.WriteDataTable(((HairListData)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\Hairs\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.Mask:
                        file.relic.WriteDataTable(((MaskListData)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\Mask\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.Visibility:
                        file.relic.WriteDataTable(((InnerPartsVisibilityByOuter)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Costumes\\Inners\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.Common:
                        file.relic.WriteDataTable(((BasicCustomizationListData)file.Table).Make());
                        break;
                    case MergeFile.types.Mod:
                        file.relic.WriteDataTable(((ModControlFrameworkListData)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Characters\\Blueprints\\Player\\Core\\ModControlFramework\\{Path.GetFileName(file.Path)}";
                        break;
                    case MergeFile.types.ProgressSymbols:
                        file.relic.WriteDataTable(((ProgressSymbol)file.Table).Make());
                        file.Path = $"ZZZZZ-MergePatch\\CodeVein\\Content\\Modes\\{Path.GetFileName(file.Path)}";
                        break;
                    default:
                        return;
                }
                if (!Directory.Exists(Path.GetDirectoryName(file.Path)))
                    Directory.CreateDirectory(Path.GetDirectoryName(file.Path));
                Blood.Save(file.relic, file.Path);
            }
        }

        private static void Accessory(AccessoryListData accessoryList, AccessoryListData Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            AccessoryListData accessoryList1 = new AccessoryListData();
            accessoryList1.Read(relic.GetDataTable());
            foreach (AccessoryData accessory in accessoryList1.Accessories)
            {
                AccessoryData Temp = accessoryList.Accessories.FirstOrDefault(x => x.Name == accessory.Name);
                AccessoryData Temp2 = Vanilla.Accessories.FirstOrDefault(x => x.Name == accessory.Name);
                if (!accessoryList.Accessories.Contains(Temp))
                {
                    accessoryList.Accessories.Add(accessory);
                }
                else if (!PartialMerge && !Temp.Equals(accessory) && !Temp2.Equals(accessory))
                {
                    int i = accessoryList.Accessories.IndexOf(Temp);
                    accessoryList.Accessories[i] = accessory;
                }
            }
        }

        private static void Inner(InnerList innerList, InnerList Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            InnerList innerList1 = new InnerList();
            innerList1.Read(relic.GetDataTable());
            foreach (var inner in innerList1.Inners)
            {
                InnerData Temp = innerList.Inners.FirstOrDefault(x => x.Name == inner.Name);
                InnerData Temp2 = Vanilla.Inners.FirstOrDefault(x => x.Name == inner.Name);
                if (!innerList.Inners.Contains(Temp))
                    innerList.Inners.Add(inner);
                else if (!PartialMerge && !Temp.Equals(inner) && !Temp2.Equals(inner))
                {
                    int i = innerList.Inners.IndexOf(Temp);
                    innerList.Inners[i] = inner;
                }
            }
        }

        private static void FacePaint(FacePaintList facePaintList, FacePaintList Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            FacePaintList facePaintList1 = new FacePaintList();
            facePaintList1.Read(relic.GetDataTable());
            foreach (var facepaint in facePaintList1.FacePaints)
            {
                FacePaintData Temp = facePaintList.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name);
                FacePaintData Temp2 = Vanilla.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name);
                if (!facePaintList.FacePaints.Contains(Temp))
                    facePaintList.FacePaints.Add(facepaint);
                else if (!PartialMerge && !Temp.Equals(facepaint) && !Temp2.Equals(facepaint))
                {
                    int i = facePaintList.FacePaints.IndexOf(Temp);
                    facePaintList.FacePaints[i] = facepaint;
                }
            }
        }

        private static void Hair(HairListData hairListData, HairListData Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            HairListData hairListData1 = new HairListData();
            hairListData1.Read(relic.GetDataTable());
            foreach (var hair in hairListData1.HairDataList)
            {
                HairData Temp = hairListData.HairDataList.FirstOrDefault(x => x.Name == hair.Name);
                HairData Temp2 = Vanilla.HairDataList.FirstOrDefault(x => x.Name == hair.Name);
                if (!hairListData.HairDataList.Contains(Temp))
                    hairListData.HairDataList.Add(hair);
                else if (!PartialMerge && !Temp.Equals(hair) && !Temp2.Equals(hair))
                {
                    int i = hairListData.HairDataList.IndexOf(Temp);
                    hairListData.HairDataList[i] = hair;
                }
            }
        }

        private static void Mask(MaskListData maskListData, MaskListData Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            MaskListData maskListData1 = new MaskListData();
            maskListData1.Read(relic.GetDataTable());
            foreach (var mask in maskListData1.Masks)
            {
                MaskData Temp = maskListData.Masks.FirstOrDefault(x => x.Name == mask.Name);
                MaskData Temp2 = Vanilla.Masks.FirstOrDefault(x => x.Name == mask.Name);
                if (!maskListData.Masks.Contains(Temp))
                {
                    maskListData.Masks.Add(mask);
                }
                else if (!PartialMerge && !Temp.Equals(mask) && !Temp2.Equals(mask))
                {
                    int i = maskListData.Masks.IndexOf(Temp);
                    maskListData.Masks[i] = mask;
                }
            }
        }

        private static void Visibility(InnerPartsVisibilityByOuter visibilityByOuter, InnerPartsVisibilityByOuter Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            InnerPartsVisibilityByOuter visibilityByOuter1 = new InnerPartsVisibilityByOuter();
            visibilityByOuter1.Read(relic.GetDataTable());
            foreach (var inner in visibilityByOuter1.partsVisibilities)
            {
                PartsVisibilityByOuter Temp = visibilityByOuter.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name);
                PartsVisibilityByOuter Temp2 = Vanilla.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name);
                if (!visibilityByOuter.partsVisibilities.Contains(Temp))
                    visibilityByOuter.partsVisibilities.Add(inner);
                else if (!PartialMerge && !Temp.Equals(inner) && !Temp2.Equals(inner))
                {
                    int i = visibilityByOuter.partsVisibilities.IndexOf(Temp);
                    visibilityByOuter.partsVisibilities[i] = inner;
                }
            }
        }

        private static void Common(BasicCustomizationListData basicCustomizationListData, BasicCustomizationListData Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            BasicCustomizationListData basicCustomizationListData1 = new BasicCustomizationListData();
            basicCustomizationListData1.Read(relic.GetDataTable());
            foreach (var basic in basicCustomizationListData1.basicCustomizationDatas)
            {
                BasicCustomizationData Temp = basicCustomizationListData.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name);
                BasicCustomizationData Temp2 = Vanilla.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name);
                if (!basicCustomizationListData.basicCustomizationDatas.Contains(Temp))
                    basicCustomizationListData.basicCustomizationDatas.Add(basic);
                else if (!PartialMerge && !Temp.Equals(basic) && !Temp2.Equals(basic))
                {
                    int i = basicCustomizationListData.basicCustomizationDatas.IndexOf(Temp);
                    basicCustomizationListData.basicCustomizationDatas[i] = basic;
                }
            }
        }

        private static void ModControl(ModControlFrameworkListData modControlFrameworkListData, ModControlFrameworkListData Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            ModControlFrameworkListData modControlFrameworkListData1 = new ModControlFrameworkListData();
            modControlFrameworkListData1.Read(relic.GetDataTable());
            foreach(var modcontrol in modControlFrameworkListData1.SpawnerList)
            {
                ModControlFrameworkData mod = modControlFrameworkListData.SpawnerList.FirstOrDefault(x => x.Name == modcontrol.Name);
                if (!modControlFrameworkListData.SpawnerList.Contains(mod))
                    modControlFrameworkListData.SpawnerList.Add(modcontrol);
            }
        }

        private static void ProgressSymbols(ProgressSymbol progressSymbol, ProgressSymbol Vanilla, Relic relic, string pakname, bool PartialMerge = false)
        {
            ProgressSymbol progressSymbol1 = new ProgressSymbol();
            progressSymbol1.Read(relic.GetDataTable());
            foreach (var basic in progressSymbol1.progressFlags)
            {
                var Temp = progressSymbol.progressFlags.FirstOrDefault(x => x.Name == basic.Name);
                var Temp2 = Vanilla.progressFlags.FirstOrDefault(x => x.Name == basic.Name);
                if (!progressSymbol.progressFlags.Contains(Temp))
                    progressSymbol.progressFlags.Add(basic);
                else if (!PartialMerge && !Temp.Equals(basic) && !Temp2.Equals(basic))
                {
                    int i = progressSymbol.progressFlags.IndexOf(Temp);
                    progressSymbol.progressFlags[i] = basic;
                }
            }
        }
    }
}
