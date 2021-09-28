using System;
using System.Diagnostics;
using System.IO;

namespace eachdir {
	static class Program {
		static void Main(string[] args) {
			string cmd = string.Join(' ', args);
			string dir = Directory.GetCurrentDirectory();
			string[] dirs = Directory.GetDirectories(dir);
			if (args[0] == "--nospace") {
				foreach (var d in dirs) {
					if (d == d.Replace(" ", "")) { continue; }
					MoveMerge(d, d.Replace(" ", ""));
				}

			} else if (args[0] == "--toUnderscore") {
				foreach (var d in dirs) {
					if (d == d.UpToFirst("_")) { continue; }
					MoveMerge(d, d.UpToFirst("_"));
					
				}
			} else {
				foreach (var d in dirs) {
					string path = d.Replace(dir, ".");
					Console.WriteLine($"\n{path}\n$> {cmd}");
					StartProcess(cmd, d).WaitForExit();
				}
			}
		}

		public static PlatformID platformId { get; private set; }
		public static string platform { get; private set; } = InitPlatform();
		public static string shell { get; private set; }
		public static string prefix { get; private set; }
		public static string ForwardSlashPath(this string path) { return path.Replace('\\', '/'); }
		public static string UpToFirst(this string str, string search) {
			if (str.Contains(search)) {
				int ind = str.IndexOf(search);
				return str.Substring(0, ind);
			}
			return str;
		}
		public static string wdir { get { return ForwardSlashPath(Directory.GetCurrentDirectory()); } }

		static string InitPlatform() {
			platformId = Environment.OSVersion.Platform;
			if (platformId == PlatformID.Win32NT) {
				shell = @"C:\Windows\System32\cmd.exe";
				prefix = "/C";
			} else if (platformId == PlatformID.Unix) {
				shell = "/bin/bash";
				prefix = "-c";
			}
			return platformId.ToString();
		}
		public static Process StartProcess(string cmd, string folder = null) {
			ProcessStartInfo info = new ProcessStartInfo(shell, $"{prefix} \"{cmd}\"") {
				UseShellExecute = false
			};
			if (folder != null) {
				info.WorkingDirectory = folder;
			}

			Process p = new Process() { StartInfo = info, };
			p.Start();

			return p;
		}

		public static void MoveMerge(string src, string dest) {
			if (Directory.Exists(src)) {
				try {
					DirectoryInfo srcI = new DirectoryInfo(src);
					DirectoryInfo destI = new DirectoryInfo(dest);
					CopyAll(srcI, destI);
					Directory.Delete(src, true);
				} catch (Exception e) {
					Console.WriteLine($"Failed to merge \"{src}\" with \"{dest}\".\nError:{e}");
				}

			} else {
				Directory.Move(src, dest);
			}
		}

		// From: https://stackoverflow.com/questions/9053564/c-sharp-merge-one-directory-with-another
		public static void CopyAll(DirectoryInfo source, DirectoryInfo target) {
			if (source.FullName.ToLower() == target.FullName.ToLower()) { return; }

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false) { Directory.CreateDirectory(target.FullName); }

			// Copy each file into it's new directory.
			foreach (FileInfo fi in source.GetFiles()) {
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				if (File.Exists(fi.Name)) { Console.WriteLine($"\t\t!!! REPLACING {fi.Name} !!!"); }
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);

			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories()) {
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}

	}
}
