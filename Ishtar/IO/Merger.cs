using Newtonsoft.Json;
using QueenIO;
using QueenIO.Tables;
using QueenIO.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            }
        }

        private static ConcurrentDictionary<string, MergeFile> MergeFiles = new ConcurrentDictionary<string, MergeFile>();
        private static ConcurrentDictionary<string, string> LoggedPaks = new ConcurrentDictionary<string, string>();

        public static void ListMergeablePaks(string[] infiles, string[] Tables)
        {
            Helpers.Log("richTextBox1", $"Checking Paks for mergeable files...");
            Parallel.ForEach(infiles, pak =>
            {
                string[] list = Helpers.ListPak(pak);
                if (list.Any(x => Tables.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                {
                    bool added = LoggedPaks.TryAdd(Path.GetFileNameWithoutExtension(pak), pak);
                    if (added)
                    {
                        //Helpers.Log("richTextBox1", $"Meragable files found in {Path.GetFileNameWithoutExtension(pak)}");
                    }
                }
            });
            Helpers.Log("richTextBox1", $"Found {LoggedPaks.Count} paks for merging");
        }



        public static void Merge(string[] infiles, string[] Tables, bool PartialMerge = false)
        {

            foreach (string pak in LoggedPaks.Values)
            {
                string staging = Path.GetFileNameWithoutExtension(pak);
                if (Directory.Exists(staging))
                    Directory.Delete(staging, true);
                if (Path.GetFileName(pak) == "ZZZZZ-MergePatch_P.pak")
                    continue;

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
                    if (!Directory.Exists(Path.GetDirectoryName(outpath)))
                        Directory.CreateDirectory(Path.GetDirectoryName(outpath));

                    //check if existing merge exist, use that for the base table instead if so
                    string tbl = $"Tables\\{Path.GetFileNameWithoutExtension(file)}.json";
                    string name = Path.GetFileNameWithoutExtension(file);

                    if (tbl.Contains("DT_AccessoryPreset"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Accessory((AccessoryListData)MergeFiles[name].Table, (AccessoryListData)MergeFiles[name].VanillaTable, relic);
                            AccessoryListData data = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                            AccessoryListData vanilla = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)0 });
                        }
                    }
                    else if (tbl.Contains("DT_InnerList"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Inner((InnerList)MergeFiles[name].Table, (InnerList)MergeFiles[name].VanillaTable, relic);
                            InnerList data = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                            InnerList vanilla = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)1 });
                        }
                    }
                    else if (tbl.Contains("DT_FacePaintMask"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            FacePaint((FacePaintList)MergeFiles[name].Table, (FacePaintList)MergeFiles[name].VanillaTable, relic);
                            FacePaintList data = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                            FacePaintList vanilla = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)2 });
                        }
                    }
                    else if (tbl.Contains("DT_HairList"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Hair((HairListData)MergeFiles[name].Table, (HairListData)MergeFiles[name].VanillaTable, relic);
                            HairListData data = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                            HairListData vanilla = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)3 });
                        }
                    }
                    else if (tbl.Contains("DT_InnerFrame") || tbl.Contains("DT_OuterMask"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Mask((MaskListData)MergeFiles[name].Table, (MaskListData)MergeFiles[name].VanillaTable, relic);
                            MaskListData data = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                            MaskListData vanilla = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)4 });
                        }
                    }
                    else if (tbl.Contains("DT_InnerPartsVisibilityByOuter"))
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Visibility((InnerPartsVisibilityByOuter)MergeFiles[name].Table, (InnerPartsVisibilityByOuter)MergeFiles[name].VanillaTable, relic);
                            InnerPartsVisibilityByOuter data = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                            InnerPartsVisibilityByOuter vanilla = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)5 });
                        }
                    }
                    else
                    {
                        if (!MergeFiles.ContainsKey(name))
                        {
                            Common((BasicCustomizationListData)MergeFiles[name].Table, (BasicCustomizationListData)MergeFiles[name].VanillaTable, relic);
                            BasicCustomizationListData data = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                            BasicCustomizationListData vanilla = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                            MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, VanillaTable = vanilla, Type = (MergeFile.types)6 });
                        }
                    }

                }
                Directory.Delete(staging, true);
            }
            Export();
        }

        private static void Export()
        {
            Helpers.Log("richTextBox1", "Writing all Tables to uassets...");
            Parallel.ForEach(MergeFiles.Values, file =>
            {
                switch (file.Type)
                {
                    case MergeFile.types.Accessory:
                        file.relic.WriteDataTable(((AccessoryListData)file.Table).Make());
                        break;
                    case MergeFile.types.Inner:
                        file.relic.WriteDataTable(((InnerList)file.Table).Make());
                        break;
                    case MergeFile.types.FacePaint:
                        file.relic.WriteDataTable(((FacePaintList)file.Table).Make());
                        break;
                    case MergeFile.types.Hair:
                        file.relic.WriteDataTable(((HairListData)file.Table).Make());
                        break;
                    case MergeFile.types.Mask:
                        file.relic.WriteDataTable(((MaskListData)file.Table).Make());
                        break;
                    case MergeFile.types.Visibility:
                        file.relic.WriteDataTable(((InnerPartsVisibilityByOuter)file.Table).Make());
                        break;
                    case MergeFile.types.Common:
                        file.relic.WriteDataTable(((BasicCustomizationListData)file.Table).Make());
                        break;
                    default:
                        return;
                }
                Blood.Save(file.relic, file.Path);
            });
        }

        private static void Accessory(AccessoryListData accessoryList, AccessoryListData Vanilla, Relic relic)
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
                else if (!Temp.Equals(accessory) && !Temp2.Equals(accessory))
                {
                    int i = accessoryList.Accessories.IndexOf(Temp);
                    accessoryList.Accessories[i] = accessory;
                }
            }
        }

        private static void Inner(InnerList innerList, InnerList Vanilla, Relic relic)
        {
            InnerList innerList1 = new InnerList();
            innerList1.Read(relic.GetDataTable());
            foreach (var inner in innerList1.Inners)
            {
                InnerData Temp = innerList.Inners.FirstOrDefault(x => x.Name == inner.Name);
                InnerData Temp2 = Vanilla.Inners.FirstOrDefault(x => x.Name == inner.Name);
                if (!innerList.Inners.Contains(Temp))
                    innerList.Inners.Add(inner);
                else if (!Temp.Equals(inner) && !Temp2.Equals(inner))
                {
                    int i = innerList.Inners.IndexOf(Temp);
                    innerList.Inners[i] = inner;
                }
            }
        }

        private static void FacePaint(FacePaintList facePaintList, FacePaintList Vanilla, Relic relic)
        {
            FacePaintList facePaintList1 = new FacePaintList();
            facePaintList1.Read(relic.GetDataTable());
            foreach (var facepaint in facePaintList1.FacePaints)
            {
                FacePaintData Temp = facePaintList.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name);
                FacePaintData Temp2 = Vanilla.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name);
                if (!facePaintList.FacePaints.Contains(Temp))
                    facePaintList.FacePaints.Add(facepaint);
                else if (!Temp.Equals(facepaint) && !Temp2.Equals(facepaint))
                {
                    int i = facePaintList.FacePaints.IndexOf(Temp);
                    facePaintList.FacePaints[i] = facepaint;
                }
            }
        }

        private static void Hair(HairListData hairListData, HairListData Vanilla, Relic relic)
        {
            HairListData hairListData1 = new HairListData();
            hairListData1.Read(relic.GetDataTable());
            foreach (var hair in hairListData1.HairDataList)
            {
                HairData Temp = hairListData.HairDataList.FirstOrDefault(x => x.Name == hair.Name);
                HairData Temp2 = Vanilla.HairDataList.FirstOrDefault(x => x.Name == hair.Name);
                if (!hairListData.HairDataList.Contains(Temp))
                    hairListData.HairDataList.Add(hair);
                else if (!Temp.Equals(hair) && !Temp2.Equals(hair))
                {
                    int i = hairListData.HairDataList.IndexOf(Temp);
                    hairListData.HairDataList[i] = hair;
                }
            }
        }

        private static void Mask(MaskListData maskListData, MaskListData Vanilla, Relic relic)
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
                else if (!Temp.Equals(mask) && !Temp2.Equals(mask))
                {
                    int i = maskListData.Masks.IndexOf(Temp);
                    maskListData.Masks[i] = mask;
                }
            }
        }

        private static void Visibility(InnerPartsVisibilityByOuter visibilityByOuter, InnerPartsVisibilityByOuter Vanilla, Relic relic)
        {
            InnerPartsVisibilityByOuter visibilityByOuter1 = new InnerPartsVisibilityByOuter();
            visibilityByOuter1.Read(relic.GetDataTable());
            foreach (var inner in visibilityByOuter1.partsVisibilities)
            {
                PartsVisibilityByOuter Temp = visibilityByOuter.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name);
                PartsVisibilityByOuter Temp2 = Vanilla.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name);
                if (!visibilityByOuter.partsVisibilities.Contains(Temp))
                    visibilityByOuter.partsVisibilities.Add(inner);
                else if (!Temp.Equals(inner) && !Temp2.Equals(inner))
                {
                    int i = visibilityByOuter.partsVisibilities.IndexOf(Temp);
                    visibilityByOuter.partsVisibilities[i] = inner;
                }
            }
        }

        private static void Common(BasicCustomizationListData basicCustomizationListData, BasicCustomizationListData Vanilla, Relic relic)
        {
            BasicCustomizationListData basicCustomizationListData1 = new BasicCustomizationListData();
            basicCustomizationListData1.Read(relic.GetDataTable());
            foreach (var basic in basicCustomizationListData1.basicCustomizationDatas)
            {
                BasicCustomizationData Temp = basicCustomizationListData.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name);
                BasicCustomizationData Temp2 = Vanilla.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name);
                if (!basicCustomizationListData.basicCustomizationDatas.Contains(Temp))
                    basicCustomizationListData.basicCustomizationDatas.Add(basic);
                else if (!Temp.Equals(basic) && !Temp2.Equals(basic))
                {
                    int i = basicCustomizationListData.basicCustomizationDatas.IndexOf(Temp);
                    basicCustomizationListData.basicCustomizationDatas[i] = basic;
                }
            }
        }
    }
}
