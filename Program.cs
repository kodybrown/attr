using System;
using System.IO;

namespace attr
{
	class Program
	{
		// OPTIONS
		static bool processed_file = false;
		static bool pause = false;
		static bool recursive = false;
		static bool incl_dirs = false;
		static bool showpath = false;

		static int Main( string[] arguments )
		{
			string f = null;
			FileAttributes attrs;
			bool has_attrs;
			bool? arc, sys, ro, hid;

			// Clear/reset the flags for the next file.
			f = null;
			attrs = FileAttributes.Normal;
			has_attrs = false;
			arc = null;
			sys = null;
			ro = null;
			hid = null;

			foreach (string arg in arguments) {
				string a = arg;

				if (a.StartsWith("/") || a.StartsWith("--")) {
					// OPTIONS..
					while (a.StartsWith("/") || a.StartsWith("-")) {
						a = a.Substring(1);
					}

					string al = a.ToLowerInvariant();

					if (a == "?" || al.StartsWith("h")) {
						usage();
						return 0;

					} else if (al.StartsWith("p") || al.StartsWith("!p") || al.StartsWith("no-p")) {
						pause = al.StartsWith("p");
					} else if (al.Equals("s") || al.Equals("!s") || al.StartsWith("r") || al.StartsWith("!r")) {
						recursive = al.Equals("s") || al.StartsWith("r");

					} else if (al.StartsWith("d") || al.StartsWith("!d") || al.StartsWith("no-d")) {
						incl_dirs = al.StartsWith("d");

					} else if (al.StartsWith("showp") || al.StartsWith("show-p") || al.StartsWith("!showp") || al.StartsWith("nop") || al.StartsWith("no-p")) {
						showpath = al.StartsWith("showp") || al.StartsWith("show-p");

					} else {
						Console.WriteLine("unknown option: " + a);
					}
				} else if (a.StartsWith("+") || a.StartsWith("-")) {
					bool op = (a[0] == '+');

					for (int i = 0; i < a.Length; i++) {
						char c = a[i];

						if (c == '+') {
							op = true;
						} else if (c == '-') {
							op = false;
						} else if (c == 'a' || c == 'A') {
							has_attrs = true;
							arc = op;
						} else if (c == 's' || c == 'S') {
							has_attrs = true;
							sys = op;
						} else if (c == 'r' || c == 'R') {
							has_attrs = true;
							ro = op;
						} else if (c == 'h' || c == 'H') {
							has_attrs = true;
							hid = op;
						} else {
							Console.WriteLine("unknown option: " + a + " [" + a.Substring(0, i - 1) + "*" + a.Substring(i - 1, 1) + "*" + a.Substring(i) + "]");
						}
					}
				} else {
					processed_file = true;

					f = a;

					if (!Path.IsPathRooted(f)) {
						f = Path.Combine(Environment.CurrentDirectory, f);
					}

					if (!Directory.Exists(f) && !File.Exists(f) && !ValidWildcards(f)) {
						Console.WriteLine("File (or directory) was not found: " + f);
						continue;
					}

					if (!has_attrs) {
						// Display the item's attributes.
						DisplayFileAttributes(f);
					} else {
						// Get the file's current attributes.
						attrs = File.GetAttributes(f);

						// Set the file's attributes.
						if (arc.HasValue) {
							if (arc.Value) {
								attrs |= FileAttributes.Archive;
							} else {
								attrs &= ~FileAttributes.Archive;
							}
						}
						if (sys.HasValue) {
							if (sys.Value) {
								attrs |= FileAttributes.System;
							} else {
								attrs &= ~FileAttributes.System;
							}
						}
						if (ro.HasValue) {
							if (ro.Value) {
								attrs |= FileAttributes.ReadOnly;
							} else {
								attrs &= ~FileAttributes.ReadOnly;
							}
						}
						if (hid.HasValue) {
							if (hid.Value) {
								attrs |= FileAttributes.Hidden;
							} else {
								attrs &= ~FileAttributes.Hidden;
							}
						}

						File.SetAttributes(f, attrs);

						// Display the new attributes.
						DisplayFileAttributes(f);
					}

					// Clear/reset the flags for the next file.
					f = null;
					attrs = FileAttributes.Normal;
					has_attrs = false;
					arc = null;
					sys = null;
					ro = null;
					hid = null;
				}
			}

			if (!processed_file) {
				// show files with attributes (mimic DOS)..
				Console.WriteLine(Environment.CurrentDirectory);
				DisplayFileAttributes(Environment.CurrentDirectory);
			}

			if (pause) {
				Console.WriteLine("Press any key to exit...");
				Console.ReadKey(true);
			}

			return 0;
		}

		static bool ValidWildcards( string f )
		{
			int i = f.LastIndexOf('\\');
			string d = f.Substring(0, i + 1),
				n = f.Substring(i + 1);

			if (d.IndexOf("*") > -1) {
				return false;
			}

			return n.IndexOf("*") > -1;
		}

		static void DisplayFileAttributes( string f )
		{
			FileSystemInfo[] fileInfos;

			if (null == f || 0 == (f = f.Trim()).Length) {
				Console.WriteLine("Missing file name");
				return;
			}

			if (Directory.Exists(f) && (recursive || !processed_file)) {
				fileInfos = new DirectoryInfo(f).GetFileSystemInfos("*.*");
			} else if (Directory.Exists(f) || File.Exists(f) || ValidWildcards(f)) {
				int i = f.LastIndexOf('\\');
				string d = f.Substring(0, i + 1),
					n = f.Substring(i + 1);
				fileInfos = new DirectoryInfo(d).GetFileSystemInfos(n);
			} else {
				Console.WriteLine("File (or directory) was not found: " + f);
				return;
			}

			foreach (FileSystemInfo fi in fileInfos) {
				FileAttributes fa = fi.Attributes;

				//if ((fa & FileAttributes.Directory) != FileAttributes.Directory || incl_dirs) {
				Console.Out.WriteLine("{1} {3}{4}{5}{6}  {2}{0}",
					showpath ? fi.FullName : fi.Name,
					(fa & FileAttributes.Directory) == FileAttributes.Directory ? "D" : " ",
					(fa & FileAttributes.ReparsePoint) == FileAttributes.ReparsePoint ? "*" : " ",
					(fa & FileAttributes.Archive) == FileAttributes.Archive ? "A" : " ",
					(fa & FileAttributes.System) == FileAttributes.System ? "S" : " ",
					(fa & FileAttributes.Hidden) == FileAttributes.Hidden ? "H" : " ",
					(fa & FileAttributes.ReadOnly) == FileAttributes.ReadOnly ? "R" : " "
				);
				//}

				if (recursive && (fa & FileAttributes.Directory) == FileAttributes.Directory) {
					// todo 
					Console.WriteLine("**** TODO: recursive... (sorry)");
				}
			}
		}

		static void header()
		{
			Console.WriteLine(@"attr.exe | Copyright (C) 2013-2014 @wasatchwizard
");
		}

		static void usage()
		{
			header();
			Console.WriteLine(@"SYNOPSIS:

  A simple utility to view and modify file attributes.. There are three 
  main benefits of this utility over Windows' built-in attrib.exe.

  1) The order in which you alter the attributes does not matter. For
     instance, you don't have to explicity remove -r before -s, etc.
  2) You can combine attributes as in: `attr -rsh file`.
  3) You can modify multiple files via the same command-line as in:
     `attr -sa file1 file2 +hs file3`.

USAGE: attr.exe [options] [file] [file..n] [options file..n] ...

  no-arguments    Displays attributes for all files (and directories) 
                  in the current directory.

  file            Specifies a file (or file wildcard) for attr to process.

OPTIONS:

    +     Sets an attribute.
    -     Clears an attribute.

    r     Read-only file attribute.
    a     Archive file attribute.
    s     System file attribute.
    h     Hidden file attribute.

  /p --pause      Pause when finished.
  /r --recursive  Processes matching files in the current directory 
                  and all sub-directories. (also /s)
  /d              Processes directories as well. (not implemented!)

     --showpath   Output each file's full path and name.
                  Default is file name only (without the path).
EXAMPLES:

  > attr -h -s desktop.ini
    Removes the hidden and system attributes from desktop.ini.

  > attr -hs +a desktop.ini
    Removes the hidden and system attributes from 
    and adds the archive attribute to desktop.ini.

  > attr -r +hs -a desktop.ini +hs folder.jpg
    Removes the readonly and archive attributes from and adds the 
    hidden and system attributes to desktop.ini.
    Removes the archive attribute and adds the hidden and system 
    attributes to folder.jpg.
    
  > attr /s -h -s desktop.ini
    Removes the hidden and system attributes from desktop.ini in the
    current and all sub-directories.

".Trim());
		}
	}
}
