using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace race {
	static class Program {
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("Usage: race (time in seconds) (command)");
				return;
			}

			double time;
			if (!double.TryParse(args[0], out time)) {
				Console.WriteLine($"Could not read time from \"{args[0]}\"");
				return;
			}

			string[] restArgs = new string[args.Length - 1];
			for (int i = 0; i < restArgs.Length; i++) {
				restArgs[i] = args[i+1];
			}
			
			int ms = (int)(time * 1000);
			Task delay = Task.Delay(ms);
			Process p = null;
			DateTime start = DateTime.UtcNow;
			DateTime end = start;
			Task process = Task.Run(async () => {
				p = StartProcess(string.Join(' ', restArgs));
				start = DateTime.UtcNow;
				await p.WaitForExitAsync();
				end = DateTime.UtcNow;
			});

			int index = Task.WaitAny(delay, process);
			if (index == 0) {
				p?.Kill();
				Console.WriteLine($"\n\nProcess did not finish before {time}s delay.");
			} else {
				Console.WriteLine($"\n\nProcess exited with code {p.ExitCode} in {(end-start).TotalSeconds}s.");
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
	}

}
