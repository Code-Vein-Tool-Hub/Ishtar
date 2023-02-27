using Newtonsoft.Json;
using QueenIO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UAssetAPI;

namespace Ishtar.IO
{
    public class AssetRegistryMerger
    {
        private static ConcurrentDictionary<string, string> LoggedPaks = new ConcurrentDictionary<string, string>();

        public static void ListMergeablePaks(string[] infiles)
        {
            if (!File.Exists("Tables\\AssetRegistry.json"))
            {
                Helpers.Log("richTextBox1", $"Could not find AssetRegistry.Json");
                return;
            }

            Parallel.ForEach(infiles, pak =>
            {
                string[] list = Helpers.ListPak(pak);
                if (list.Any(x => x.Contains("AssetRegistry")))
                {
                    bool added = LoggedPaks.TryAdd(Path.GetFileNameWithoutExtension(pak), pak);
                }
            });
            Helpers.Log("richTextBox1", $"Found {LoggedPaks.Count} paks with Asset Registries for merging");
        }

        public static void Merge(string[] infiles)
        {
            if (LoggedPaks.Values.Count <= 0)
            {
                Helpers.Log("richTextBox1", $"no paks found, skipping merge");
                return;
            }

            AssetRegistry assestRegistry = JsonConvert.DeserializeObject<AssetRegistry>(File.ReadAllText("Tables\\AssetRegistry.json"));
            Dictionary<string, string> RegisteredAssets = new Dictionary<string, string>();

            IEnumerable<string> RegisteredAssetsList = from fassetdata in assestRegistry.fAssetDatas
                                                   select fassetdata.ObjectPath.ToString();

            RegisteredAssets = RegisteredAssetsList.ToDictionary(x => x);

            Helpers.Log("richTextBox1", $"{RegisteredAssets.ToArray().Length} Base assets registered...");

            string outpath = $"ZZZZZ-MergePatch\\CodeVein\\AssetRegistry.bin";
            if (!Directory.Exists(Path.GetDirectoryName(outpath)))
                Directory.CreateDirectory(Path.GetDirectoryName(outpath));

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
                string AssRegPath = list.First(x => Path.GetFileName(x) == "AssetRegistry.bin");

                AssetRegistry modRegistry = new AssetRegistry();
                modRegistry.Read(File.ReadAllBytes(AssRegPath));

                Parallel.ForEach(modRegistry.fAssetDatas, fassetdata =>
                {
                    try
                    {
                        _ = RegisteredAssets[fassetdata.ObjectPath.ToString()];
                    }
                    catch (KeyNotFoundException e)
                    {
                        assestRegistry.fAssetDatas.Add(fassetdata);
                        RegisteredAssets.Add(fassetdata.ObjectPath.ToString(), fassetdata.ObjectPath.ToString());
                        Helpers.Log("richTextBox1", $"    Registered asset {fassetdata.AssetName}");
                    }
                });
                Directory.Delete(staging, true);
            }

            File.WriteAllBytes(outpath, assestRegistry.Make());
            return;
        }
    }
}
