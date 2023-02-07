Easily batch render blender files without having to mess with command line. Drag and drop files in, or select them with the program's file explorer. UI made in Godot 4.

*Note: When exporting, make sure to add res://Scripts/frame_range.py to the list of scripts to be exported. This is so the program can get the frame range of the file.

TODO list:

	[] Optional render output path override
	[] get samples for progress from current image
	[] allow default render of start frame for single image
	[] output log to file
	[] remember queue
	[] Let the user know when using main thread that the program will be unresponsive
	[] Make it so on adding a file to the queue, it gets the frame start/end info then
