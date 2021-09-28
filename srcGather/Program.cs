using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace srcGather {
	static class Program {
		public static string SourceFileDirectory([CallerFilePath] string callerPath = "[NO PATH]") {
			return callerPath.Substring(0, callerPath.Replace('\\', '/').LastIndexOf('/'));
		}

		// FUCKING UGH.
		// Why are there so many different filetypes?
		// I've seen students use all sorts of them!
		static readonly string[] cppExts = {
			".h",".H",".hpp",
			".c",".C",".cpp", ".cc",".c++",".cxx",
			".cppm", ".c++m", ".cxxm",
			".ii", ".ixx", ".ipp", ".inl", 
			".txx", ".tpp", ".tpl",
		};

		static readonly string[] javaExts = { ".java" };

		// public static string[] javaExts = { ".java" };

		static void Main(string[] args) {
			Console.WriteLine($"{args.Length} args: {string.Join(", ", args)}");
			var exts = cppExts;
			if (args.Length > 0 && (args[0] == "--java" || args[0] == "-j")) {
				exts = javaExts;
			}
			Console.WriteLine($"Searching for files with extensions: {string.Join(", ", exts) }");

			string dir = Directory.GetCurrentDirectory();
			var files = GetAllFiles(dir)
				.Where(it => !it.Filename().StartsWith("._"))

				.FilterExtensions(exts);


			int copied = 0;
			foreach (var file in files) {
				var f = file.Replace(dir, ".");
				try {
					string dest = $"./{file.Filename()}";
					if (!File.Exists(dest)) { 
						Console.WriteLine($"Copying [{f}] to [{dest}]");
						File.Copy(file, dest, true); 
					} else {
						Console.WriteLine($"Skipping Copy of [{f}] to [{dest}]");
					}
					copied++;
				} catch (Exception e) {
					Console.WriteLine($"Could not write file [{f}] to [./{file.Filename()}]\n{e}");
				}
			}

			var dirs = Directory.GetDirectories(dir);
			foreach (var d in dirs) { Directory.Delete(d, true); }

			Console.WriteLine($"Copied {copied} items.");

		}


		public static void CopySourceFiles(string fromDirectory, string toDirectory) {
			var files = GetAllFiles(fromDirectory.ForwardSlashPath()).Select(s => s.ForwardSlashPath());
			Console.WriteLine($"Copying {files.Count()} files in tree \n\tFrom: {fromDirectory}\n\tTo  : {toDirectory}");
			if (Directory.Exists(toDirectory)) {
				Directory.Delete(toDirectory, true);
			}
			if (!Directory.Exists(toDirectory)) {
				Directory.CreateDirectory(toDirectory);
			}
			foreach (var file in files) {
				string filename = file.Filename();
				string relpath = file.RelPath(fromDirectory);
				string destination = $"{toDirectory}{relpath}{filename}".ForwardSlashPath();
				// Console.WriteLine($"Copy for [{relpath}] [{filename}]\n\tFrom: {file}\n\tTo  : {destination}");
				string folder = destination.Folder();

				if (!Directory.Exists(folder)) {
					Directory.CreateDirectory(folder);
				}
				string text = File.ReadAllText(file);
				File.WriteAllText(destination, text);

				//File.Copy(file, destination, true);
			}
		}

		public static void FixSourceFiles(string inDirectory) {
			var files = GetAllFiles(inDirectory.ForwardSlashPath()).Select(s => s.ForwardSlashPath())
				.Where(it => it.EndsWith(".cs"));
			Console.WriteLine($"Checking {files.Count()} files in tree:\n{inDirectory}");
			foreach (var file in files) {
				string text = File.ReadAllText(file);
				if (text.Contains("\r\n")) {
					text = text.Replace("\r\n", "\n").Replace("\n\r", "\n").Replace("\r", "\n");
					File.WriteAllText(file, text, Encoding.UTF8);
				}
			}
		}

		private static string Filename(this string filepath) {
			return filepath.ForwardSlashPath().FromLast("/");
		}
		private static string Folder(this string filepath) {
			return filepath.UpToLast("/");
		}

		private static string ForwardSlashPath(this string path) { return path.Replace('\\', '/'); }
		private static string UpToLast(this string str, string search) {
			if (str.Contains(search)) {
				int ind = str.LastIndexOf(search);
				return str.Substring(0, ind);
			}
			return str;
		}
		private static string FromLast(this string str, string search) {
			if (str.Contains(search) && !str.EndsWith(search)) {
				int ind = str.LastIndexOf(search);

				return str.Substring(ind + search.Length);
			}
			return "";
		}

		private static string RelPath(this string filepath, string from) {
			return filepath.Replace(from, "").Replace(filepath.Filename(), "");
		}
		private static IEnumerable<string> FilterExtensions(this IEnumerable<string> src, params string[] exts) {
			return src.Where(it => {
				foreach (string ext in exts) {
					string xt = !ext.StartsWith('.') ? "." + ext : ext;
					if (it.EndsWith(xt)) { return true; }
				}
				return false;
			});
		}

		private static List<string> GetAllFiles(string dirPath, List<string> collector = null) {
			if (collector == null) { collector = new List<string>(); }

			collector.AddRange(Directory.GetFiles(dirPath));
			foreach (var subdir in Directory.GetDirectories(dirPath)) {
				GetAllFiles(subdir, collector);
			}

			return collector;
		}
	}
}
