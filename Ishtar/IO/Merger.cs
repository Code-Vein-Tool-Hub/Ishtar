using Newtonsoft.Json;
using QueenIO;
using QueenIO.Tables;
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

        public static void Merge(string[] infiles, string[] Tables)
        {

            foreach (string pak in infiles)
            {
                string staging = Path.GetFileNameWithoutExtension(pak);
                if (Directory.Exists(staging))
                    Directory.Delete(staging, true);
                if (Path.GetFileName(pak) == "ZZZZZ-MergePatch_P.pak")
                    continue;

                Helpers.Log("richTextBox1", $"Checking {Path.GetFileName(pak)} for Data Tables...");
                string[] list = Helpers.ListPak(pak);
                if (list.Any(x => Tables.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                {
                    var uassetList = list.Where(x => Tables.Any(y => Path.GetFileName(y).Replace(".json", ".uasset") == Path.GetFileName(x))).ToList();

                    Helpers.ExtractPak(pak, staging);
                    foreach (string file in uassetList)
                    {
                        if (!Directory.Exists("Merged"))
                            Directory.CreateDirectory("Merged");

                        Helpers.Log("richTextBox1", $"Merging {Path.GetFileName(file)}...");
                        string fullpath = $"{Directory.GetCurrentDirectory()}\\{staging}\\{file}";
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
                                AccessoryListData data = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)0 });
                            }
                            Accessory((AccessoryListData)MergeFiles[name].Table, relic);
                        }
                        else if (tbl.Contains("DT_InnerList"))
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                InnerList data = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)1 });
                            }
                            Inner((InnerList)MergeFiles[name].Table, relic);
                        }
                        else if (tbl.Contains("DT_FacePaintMask"))
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                FacePaintList data = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)2 });
                            }
                            FacePaint((FacePaintList)MergeFiles[name].Table, relic);
                        }
                        else if (tbl.Contains("DT_HairList"))
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                HairListData data = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)3 });
                            }
                            Hair((HairListData)MergeFiles[name].Table, relic);
                        }
                        else if (tbl.Contains("DT_InnerFrame") || tbl.Contains("DT_OuterMask"))
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                MaskListData data = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)4 });
                            }
                            Mask((MaskListData)MergeFiles[name].Table, relic);
                        }
                        else if (tbl.Contains("DT_InnerPartsVisibilityByOuter"))
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                InnerPartsVisibilityByOuter data = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)5 });
                            }
                            Visibility((InnerPartsVisibilityByOuter)MergeFiles[name].Table, relic);
                        }
                        else
                        {
                            if (!MergeFiles.ContainsKey(name))
                            {
                                BasicCustomizationListData data = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                                MergeFiles.TryAdd(name, new MergeFile() { Path = outpath, relic = relic, Table = data, Type = (MergeFile.types)6 });
                            }
                            Common((BasicCustomizationListData)MergeFiles[name].Table, relic);
                        }

                    }
                    Directory.Delete(staging, true);
                    continue;
                }
                else
                {
                    continue;
                }
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

        private static void Accessory(AccessoryListData accessoryList, Relic relic)
        {
            AccessoryListData accessoryList1 = new AccessoryListData();
            accessoryList1.Read(relic.GetDataTable());
            foreach (AccessoryData accessory in accessoryList1.Accessories)
            {
                if (!accessoryList.Accessories.Contains(accessoryList.Accessories.FirstOrDefault(x => x.Name == accessory.Name)))
                {
                    accessoryList.Accessories.Add(accessory);
                }
            }
        }

        private static void Inner(InnerList innerList, Relic relic)
        {
            InnerList innerList1 = new InnerList();
            innerList1.Read(relic.GetDataTable());
            foreach (var inner in innerList1.Inners)
            {
                if (!innerList.Inners.Contains(innerList.Inners.FirstOrDefault(x => x.Name == inner.Name)))
                    innerList.Inners.Add(inner);
            }
        }

        private static void FacePaint(FacePaintList facePaintList, Relic relic)
        {
            FacePaintList facePaintList1 = new FacePaintList();
            facePaintList1.Read(relic.GetDataTable());
            foreach (var facepaint in facePaintList1.FacePaints)
            {
                if (!facePaintList.FacePaints.Contains(facePaintList.FacePaints.FirstOrDefault(x => x.Name == facepaint.Name)))
                    facePaintList.FacePaints.Add(facepaint);
            }
        }

        private static void Hair(HairListData hairListData, Relic relic)
        {
            HairListData hairListData1 = new HairListData();
            hairListData1.Read(relic.GetDataTable());
            foreach (var hair in hairListData1.HairDataList)
            {
                if (!hairListData.HairDataList.Contains(hairListData.HairDataList.FirstOrDefault(x => x.Name == hair.Name)))
                    hairListData.HairDataList.Add(hair);
            }
        }

        private static void Mask(MaskListData maskListData, Relic relic)
        {
            MaskListData maskListData1 = new MaskListData();
            maskListData1.Read(relic.GetDataTable());
            foreach (var mask in maskListData1.Masks)
            {
                if (!maskListData.Masks.Contains(maskListData.Masks.FirstOrDefault(x => x.Name == mask.Name)))
                    maskListData.Masks.Add(mask);
            }
        }

        private static void Visibility(InnerPartsVisibilityByOuter visibilityByOuter, Relic relic)
        {
            InnerPartsVisibilityByOuter visibilityByOuter1 = new InnerPartsVisibilityByOuter();
            visibilityByOuter1.Read(relic.GetDataTable());
            foreach (var inner in visibilityByOuter1.partsVisibilities)
            {
                if (!visibilityByOuter.partsVisibilities.Contains(visibilityByOuter.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name)))
                    visibilityByOuter.partsVisibilities.Add(inner);
            }
        }

        private static void Common(BasicCustomizationListData basicCustomizationListData, Relic relic)
        {
            BasicCustomizationListData basicCustomizationListData1 = new BasicCustomizationListData();
            basicCustomizationListData1.Read(relic.GetDataTable());
            foreach (var basic in basicCustomizationListData1.basicCustomizationDatas)
            {
                if (!basicCustomizationListData.basicCustomizationDatas.Contains(basicCustomizationListData.basicCustomizationDatas.FirstOrDefault(x => x.Name == basic.Name)))
                    basicCustomizationListData.basicCustomizationDatas.Add(basic);
            }
        }
    }
}
