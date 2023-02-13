using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.WebRequestMethods;

namespace Ishtar.IO
{
    public class Helpers
    {
        public static string[] ListPak(string infile)
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
                list = output.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }
            List<string> result = new List<string>();
            foreach (string line in list)
            {
                if (!line.Contains("LogPakFile: Display:"))
                    continue;
                string file = line.Split(' ')[2];
                if (!file.Contains("\""))
                    continue;
                result.Add(file.Replace("\"", "").Replace("/", "\\"));
            }
            return result.ToArray();
        }

        public static void ExtractPak(string infile, string outpath = "Temp")
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "ExtractPak.bat";
                process.StartInfo.Arguments = $"\"{infile}\" \"{outpath}\"";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                //process.StartInfo.RedirectStandardOutput = true;
                process.Start();

                //richTextBox1.AppendText(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            }
        }

        public static void Log(string richTextBox, string line)
        {
            RichTextBox RTB = Application.OpenForms["Form1"].Controls[richTextBox] as RichTextBox;

            if (RTB.InvokeRequired)
                RTB.Invoke(new Action(() => RTB.AppendText(line + Environment.NewLine)));
            else
                RTB.AppendText(line + Environment.NewLine);
        }

        public static void LogClear(string richTextBox)
        {
            RichTextBox RTB = Application.OpenForms["Form1"].Controls[richTextBox] as RichTextBox;

            if (RTB.InvokeRequired)
                RTB.Invoke(new Action(() => RTB.Clear()));
            else
                RTB.Clear();
        }

        public static string HashFile(byte[] infile)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var hash = md5.ComputeHash(infile);
                string s = BitConverter.ToString(hash).Replace("-", "");
                return s;
            }
        }

        public static string HashString(string s)
        {
            return HashFile(Encoding.UTF8.GetBytes(s));
        }
    }
}
