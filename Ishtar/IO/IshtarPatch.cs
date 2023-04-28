using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using QueenIO;
using QueenIO.Mods;
using QueenIO.Tables;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

namespace Ishtar.IO
{
    public class IshtarPatch
    {
        /// <summary>
        /// Mod Name, this is the easiest way to check which mod the patch is for
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// Hash used to check if patch is out of date, patch can still be merged but might not be complete
        /// </summary>
        public string Hash { get; set; }
        /// <summary>
        /// Each Table used in the mod stripped down to only contain the new/modified entires along with other info
        /// </summary>
        public List<Table> tables { get; set; } = new List<Table>();

        public class Table
        {
            /// <summary>
            /// Table name, easiest way to sperate tables of the same type
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// ingame file path to the file
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// The actual stripped data table itself as a QueenIO obejct
            /// </summary>
            public object DataTable { get; set; }
            /// <summary>
            /// The table type for quick sorting
            /// </summary>
            [JsonConverter(typeof(StringEnumConverter))]
            public types Type { get; set; }
        }

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
            ProgressSymbol,
            AssetRegistry,
        }

        /// <summary>
        /// Generates an Ishtar Patch file from a given mod.
        /// </summary>
        /// <param name="infile"></param>
        public static void GenerateIshtarPatch(string infile)
        {
            //Get List of mergeable tables
            string[] Tables = Directory.GetFiles("Tables", "*.json");

            //Check if mod contains tables legal for patching
            string[] pakFileList = Helpers.ListPak(infile);
            if (!pakFileList.Any(X => Tables.Any(Y => Path.GetFileNameWithoutExtension(Y) == Path.GetFileNameWithoutExtension(X))))
            {
                Helpers.Log("richTextBox1", $"No Patchable Tables found in {Path.GetFileName(infile)}");
                return;
            }

            //Initial patch setup
            IshtarPatch ishtarPatch = new IshtarPatch();
            ishtarPatch.FileName = Path.GetFileName(infile);
            ishtarPatch.Hash = Helpers.HashFile(File.ReadAllBytes(infile));

            //Setup a staging folder to temporarily hold the unpaked mod.
            string staging = Path.GetFileNameWithoutExtension(infile);
            if (Directory.Exists(staging))
                Directory.Delete(staging, true);

            //Extract pak and get list of mergeable files
            Helpers.ExtractPak(infile, staging);
            Helpers.Log("richTextBox1", $"Creating Ishtar Patch for {Path.GetFileName(infile)}");
            string[] list = Directory.GetFiles(staging, "*.*", SearchOption.AllDirectories);
            var uassetList = list.Where(x => Tables.Any(y => Path.GetFileName(y).Replace(".json", ".uasset") == Path.GetFileName(x))).ToList();

            //Process each uasset file
            foreach ( string uasset in uassetList )
            {
                Helpers.Log("richTextBox1", $"  Processing {Path.GetFileName(uasset)}");
                //Initial Patch Table setup
                Table patchTable = new Table();
                patchTable.Name = Path.GetFileNameWithoutExtension(uasset);

                string fullpath = $"{Directory.GetCurrentDirectory()}\\{uasset}";
                Relic relic = new Relic();
                relic = Blood.Open(fullpath);
                var namemap = relic.GetNameMapIndexList();
                patchTable.Path = namemap.FirstOrDefault(x => x.Value.Contains($"/{patchTable.Name}")).ToString();

                //Process file based on table type
                string tbl = $"Tables\\{Path.GetFileNameWithoutExtension(uasset)}.json";
                if (tbl.Contains("DT_InnerList"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Inner;
                    patchTable.DataTable = new InnerList();

                    //Read Merge table and Mod table
                    InnerList Table = JsonConvert.DeserializeObject<InnerList>(File.ReadAllText(tbl));
                    InnerList Mod = new InnerList();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.Inners)
                    {
                        InnerData Temp = Table.Inners.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.Inners.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((InnerList)patchTable.DataTable).Inners.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_AccessoryPreset"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Accessory;
                    patchTable.DataTable = new AccessoryListData();

                    //Read Merge table and Mod table
                    AccessoryListData Table = JsonConvert.DeserializeObject<AccessoryListData>(File.ReadAllText(tbl));
                    AccessoryListData Mod = new AccessoryListData();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.Accessories)
                    {
                        var Temp = Table.Accessories.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.Accessories.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((AccessoryListData)patchTable.DataTable).Accessories.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_FacePaintMask"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.FacePaint;
                    patchTable.DataTable = new FacePaintList();

                    //Read Merge table and Mod table
                    FacePaintList Table = JsonConvert.DeserializeObject<FacePaintList>(File.ReadAllText(tbl));
                    FacePaintList Mod = new FacePaintList();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.FacePaints)
                    {
                        var Temp = Table.FacePaints.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.FacePaints.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((FacePaintList)patchTable.DataTable).FacePaints.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_HairList"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Hair;
                    patchTable.DataTable = new HairListData();

                    //Read Merge table and Mod table
                    HairListData Table = JsonConvert.DeserializeObject<HairListData>(File.ReadAllText(tbl));
                    HairListData Mod = new HairListData();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.HairDataList)
                    {
                        var Temp = Table.HairDataList.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.HairDataList.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((HairListData)patchTable.DataTable).HairDataList.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_InnerFrame") || tbl.Contains("DT_OuterMask"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Mask;
                    patchTable.DataTable = new MaskListData();

                    //Read Merge table and Mod table
                    MaskListData Table = JsonConvert.DeserializeObject<MaskListData>(File.ReadAllText(tbl));
                    MaskListData Mod = new MaskListData();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.Masks)
                    {
                        var Temp = Table.Masks.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.Masks.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((MaskListData)patchTable.DataTable).Masks.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_InnerPartsVisibilityByOuter"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Visibility;
                    patchTable.DataTable = new InnerPartsVisibilityByOuter();

                    //Read Merge table and Mod table
                    InnerPartsVisibilityByOuter Table = JsonConvert.DeserializeObject<InnerPartsVisibilityByOuter>(File.ReadAllText(tbl));
                    InnerPartsVisibilityByOuter Mod = new InnerPartsVisibilityByOuter();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.partsVisibilities)
                    {
                        var Temp = Table.partsVisibilities.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.partsVisibilities.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((InnerPartsVisibilityByOuter)patchTable.DataTable).partsVisibilities.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_SpawnerList"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Mod;
                    patchTable.DataTable = new ModControlFrameworkListData();

                    //Read Merge table and Mod table
                    ModControlFrameworkListData Table = JsonConvert.DeserializeObject<ModControlFrameworkListData>(File.ReadAllText(tbl));
                    ModControlFrameworkListData Mod = new ModControlFrameworkListData();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.SpawnerList)
                    {
                        var Temp = Table.SpawnerList.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.SpawnerList.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((ModControlFrameworkListData)patchTable.DataTable).SpawnerList.Add(item);
                        }
                    }
                }
                else if (tbl.Contains("DT_ProgressSymbol"))
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.ProgressSymbol;
                    patchTable.DataTable = new ProgressSymbol();

                    //Read Merge table and Mod table
                    ProgressSymbol Table = JsonConvert.DeserializeObject<ProgressSymbol>(File.ReadAllText(tbl));
                    ProgressSymbol Mod = new ProgressSymbol();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.progressFlags)
                    {
                        var Temp = Table.progressFlags.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.progressFlags.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((ProgressSymbol)patchTable.DataTable).progressFlags.Add(item);
                        }
                    }
                }
                else 
                {
                    //Initial Patch Table setup per filetype
                    patchTable.Type = types.Common;
                    patchTable.DataTable = new BasicCustomizationListData();

                    //Read Merge table and Mod table
                    BasicCustomizationListData Table = JsonConvert.DeserializeObject<BasicCustomizationListData>(File.ReadAllText(tbl));
                    BasicCustomizationListData Mod = new BasicCustomizationListData();
                    Mod.Read(relic.GetDataTable());

                    //Add each item found only in the mod table
                    foreach (var item in Mod.basicCustomizationDatas)
                    {
                        var Temp = Table.basicCustomizationDatas.FirstOrDefault(x => x.Name == item.Name);
                        if (!Table.basicCustomizationDatas.Contains(Temp) || !Temp.Equals(item))
                        {
                            ((BasicCustomizationListData)patchTable.DataTable).basicCustomizationDatas.Add(item);
                        }
                    }
                }

                ishtarPatch.tables.Add(patchTable);
            }


            string[] registrylist = Directory.GetFiles(staging, "*.*", SearchOption.AllDirectories);
            string AssRegPath = list.FirstOrDefault(x => Path.GetFileName(x) == "AssetRegistry.bin");
            if (File.Exists(AssRegPath))
            {
                Helpers.Log("richTextBox1", $"  Processing Asset Registry");
                //Initial Patch Table setup
                Table patchTable = new Table();
                patchTable.Name = Path.GetFileNameWithoutExtension(AssRegPath);
                patchTable.Type = types.AssetRegistry;
                patchTable.DataTable = new AssetRegistry();

                //Load up assetRegistry
                AssetRegistry Table = JsonConvert.DeserializeObject<AssetRegistry>(File.ReadAllText("Tables\\AssetRegistry.json"));
                AssetRegistry Mod = new AssetRegistry();
                Mod.Read(File.ReadAllBytes(AssRegPath));
                Dictionary<string, string> RegisteredAssets = new Dictionary<string, string>();
                IEnumerable<string> RegisteredAssetsList = from fassetdata in Table.fAssetDatas
                                                           select fassetdata.ObjectPath.ToString();
                RegisteredAssets = RegisteredAssetsList.ToDictionary(x => x);

                foreach (var fassetdata in Mod.fAssetDatas)
                {
                    try
                    {
                        _ = RegisteredAssets[fassetdata.ObjectPath.ToString()];
                    }
                    catch (KeyNotFoundException e)
                    {
                        ((AssetRegistry)patchTable.DataTable).fAssetDatas.Add(fassetdata);
                        RegisteredAssets.Add(fassetdata.ObjectPath.ToString(), fassetdata.ObjectPath.ToString());
                    }
                }

                ishtarPatch.tables.Add(patchTable);
            }

            //Finalize Patch
            Directory.Delete(staging, true);
            Helpers.Log("richTextBox1", $"Done.");
            string json = JsonConvert.SerializeObject(ishtarPatch, Formatting.Indented);
            File.WriteAllText(Path.ChangeExtension(infile, ".ishtarPatch"), json);
        }
    }
}
