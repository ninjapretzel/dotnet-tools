using System;
using System.IO;
using System.Text.RegularExpressions;

namespace strip {
	class Program {
		static void Main(string[] args) {
			if (args.Length < 2) {
				Console.WriteLine("Must provide at least two arguments so the program knows what files/lines to strip.");
				Console.WriteLine("Usage: strip <file regex> <search regex>");
				return;
			}

			string fileRegex = args[0];
			string searchRegex = args[1];
			string dir = Directory.GetCurrentDirectory() + "\\";
			var files = Directory.GetFiles(dir);
			foreach (var file in files) {
				string name = file.Replace(dir, "");
				if (Regex.IsMatch(name, fileRegex)) {
					string text = File.ReadAllText(file);
					string result = Regex.Replace(text, searchRegex, "");
					if (text != result) {
						try {
							File.WriteAllText(file, result);
						} catch (Exception e) {
							Console.WriteLine($"Failed to strip file {name}:\n{e}");
							continue;
						}
						Console.WriteLine($"File {name} stripped successfully");
					} else {
						Console.WriteLine($"Nothing stripped from file {name}");
					}
					
				}
			}


		}
	}
}
