using Newtonsoft.Json;
using QueenIO;
using QueenIO.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ishtar.IO
{
    public class Merger
    {
        public static void Merge(string[] infiles, string[] Tables)
        {
            foreach (string pak in infiles)
            {
                if (Directory.Exists("Temp"))
                    Directory.Delete("Temp", true);
                if (Path.GetFileName(pak) == "ZZZZZ-MergePatch_P.pak")
                    continue;

                Helpers.Log("richTextBox1", $"Checking {Path.GetFileName(pak)} for Data Tables...");
                string[] list = Helpers.ListPak(pak);
                if (list.Any(x => Tables.Any(y => Path.GetFileNameWithoutExtension(y) == Path.GetFileNameWithoutExtension(x))))
                {
                    var uassetList = list.Where(x => Tables.Any(y => Path.GetFileName(y).Replace(".json", ".uasset") == Path.GetFileName(x))).ToList();

                    Helpers.ExtractPak(pak);
                    foreach (string file in uassetList)
                    {
                        if (!Directory.Exists("Merged"))
                            Directory.CreateDirectory("Merged");

                        Helpers.Log("richTextBox1", $"Merging {Path.GetFileName(file)}...");
                        string fullpath = $"{Directory.GetCurrentDirectory()}\\Temp\\{file}";
                        Relic relic = new Relic();
                        relic = Blood.Open(fullpath);
                        string outpath = $"ZZZZZ-MergePatch\\CodeVein\\Content\\{fullpath.Substring(fullpath.LastIndexOf("Temp\\")).Replace("Temp\\", "")}";
                        if (!Directory.Exists(Path.GetDirectoryName(outpath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(outpath));

                        //check if existing merge exist, use that for the base table instead if so
                        string tbl;
                        if (File.Exists($"Merged\\{Path.GetFileNameWithoutExtension(file)}.json"))
                            tbl = $"Merged\\{Path.GetFileNameWithoutExtension(file)}.json";
                        else
                            tbl = $"Tables\\{Path.GetFileNameWithoutExtension(file)}.json";

                        Successor successor = new Successor();
                        if (tbl.Contains("DT_AccessoryPreset"))
                            successor = Accessory(tbl, relic);
                        else if (tbl.Contains("DT_InnerList"))
                            successor = Inner(tbl, relic);
                        else if (tbl.Contains("DT_FacePaintMask"))
                            successor = FacePaint(tbl, relic);
                        else if (tbl.Contains("DT_HairList"))
                            successor = Hair(tbl, relic);
                        else if (tbl.Contains("DT_InnerFrame") || tbl.Contains("DT_OuterMask"))
                            successor = Mask(tbl, relic);
                        else if (tbl.Contains("DT_InnerPartsVisibilityByOuter"))
                            successor = Visibility(tbl, relic);
                        else
                            successor = Common(tbl, relic);

                        string outname = $"Merged\\{Path.GetFileName(tbl)}";
                        string json = JsonConvert.SerializeObject(successor.Table);
                        File.WriteAllText(outname, json);
                        relic.WriteDataTable(successor);
                        Blood.Save(relic, outpath);

                    }
                    Directory.Delete("Temp", true);
                    continue;
                }
                else
                {
                    continue;
                }
            }
        }

        private static Successor Accessory(string basetable, Relic relic)
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
            Successor successor = accessoryList.Make();
            successor.Table = accessoryList;
            return successor;
        }

        private static Successor Inner(string basetable, Relic relic)
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
            Successor successor = innerList.Make();
            successor.Table = innerList;
            return successor;
        }

        private static Successor FacePaint(string basetable, Relic relic)
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
            Successor successor = facePaintList.Make();
            successor.Table = facePaintList;
            return successor;
        }

        private static Successor Hair(string basetable, Relic relic)
        {
            HairListData hairListData = new HairListData();
            HairListData hairListData1 = new HairListData();
            hairListData = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(basetable));
            hairListData1 = new HairListData();
            hairListData1.Read(relic.GetDataTable());
            foreach (var hair in hairListData1.HairDataList)
            {
                if (!hairListData.HairDataList.Contains(hairListData.HairDataList.FirstOrDefault(x => x.Name == hair.Name)))
                    hairListData.HairDataList.Add(hair);
            }
            Successor successor = hairListData.Make();
            successor.Table = hairListData;
            return successor;
        }

        private static Successor Mask(string basetable, Relic relic)
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
            Successor successor = maskListData.Make();
            successor.Table = maskListData;
            return successor;
        }

        private static Successor Visibility(string basetable, Relic relic)
        {
            InnerPartsVisibilityByOuter visibilityByOuter = new InnerPartsVisibilityByOuter();
            InnerPartsVisibilityByOuter visibilityByOuter1 = new InnerPartsVisibilityByOuter();
            visibilityByOuter = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(basetable));
            visibilityByOuter1 = new InnerPartsVisibilityByOuter();
            visibilityByOuter1.Read(relic.GetDataTable());
            foreach (var inner in visibilityByOuter1.partsVisibilities)
            {
                if (!visibilityByOuter.partsVisibilities.Contains(visibilityByOuter.partsVisibilities.FirstOrDefault(x => x.Name == inner.Name)))
                    visibilityByOuter.partsVisibilities.Add(inner);
            }
            Successor successor = visibilityByOuter.Make();
            successor.Table = visibilityByOuter;
            return successor;
        }

        private static Successor Common(string basetable, Relic relic)
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
            Successor successor = basicCustomizationListData.Make();
            successor.Table = basicCustomizationListData;
            return successor;
        }
    }
}
