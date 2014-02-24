attr
====

A simple utility to view and modify file attributes.. There are three main benefits of this utility over Windows' built-in attrib.exe.

* The order in which you alter the attributes does not matter. For instance, you don't have to explicity remove -r before -s, etc.
* You can combine attributes as in: `attr -rsh file`.
* You can modify multiple files via the same command-line as in: `attr -sa file1 file2 +hs file3`.

Here are the details:

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
