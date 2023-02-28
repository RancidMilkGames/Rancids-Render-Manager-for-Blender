//This file is part of Rancid's Render Manager.

//Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

//Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

//You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 

//This program uses small bits of Blender's API. Blender is 

using System;
using Godot;
using System.Diagnostics;
using System.IO;
using System.Threading;
using FileAccess = Godot.FileAccess;
using Object = Godot.GodotObject;

public partial class Mono : Node
{
	[Export] private LineEdit _blenderInstallPath;
	[Export] private Control _renderItemContainer;
	[Export] private Panel _runningPanel;
	[Export] private Control _mainPanel;
	[Export] private PackedScene _outputItemScene;
	private Control _currentOutputItem;
	[Export] private Control _outputItemContainer;
	[Export] public Node Config;
	[Export] public ProgressBar ProgressBar;
	[Export] public ProgressBar CurrentProgressBar;
	[Export] public Label RunningLabel;
	[Export] public LineEdit OverridePathLineEdit;
	[Export] public Window ProgressWindow;
	
	public bool UseThreads = true;
	public bool betterProgress = true;
	private Process _process;
	private string _output = "";
	private Thread _thread;
	private bool _killProcessWithGodot = false;
	private bool _stopPressed = false;
	private int _animFrames = 0;
	private string scriptPath = "";
	private string path = "";
	public bool OffOnFinish = false;
	public RandomNumberGenerator Rng = new RandomNumberGenerator();
	
	public int StartFrame = 0;
	public int EndFrame = 0;
	private int _currentFrame = 0;
	private int MaxLogLines = 1000;
	private int _currentSearches = 0;
	private int _maxSearches = 3;
	private DirAccess _dirAccess = DirAccess.Open("res://");
	private string _invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());

	public int CurrentFrame
	{
		get => 0;
		set
		{
			_currentFrame = value;
			CurrentProgressBar.Value = ((value - StartFrame) / float.Parse(((EndFrame - StartFrame) + 1).ToString()))  * 100;
		} 
	}

	private bool RunningProp { get; set; } = false;

	public void SetRunningProp(bool value)
	{
		RunningProp = value;
		if (RunningProp) ProgressWindow.PopupCentered();
		else ProgressWindow.Hide();
		//_mainPanel.Visible = !value;
		
	}


	public override void _Ready()
	{
		_currentOutputItem = _outputItemScene.Instantiate() as Control;
		_outputItemContainer.AddChild(_currentOutputItem);
		scriptPath = FileAccess.GetFileAsString("res://Scripts/frame_range.py");
		
		var pathArray = _blenderInstallPath.Text.Split("/");
		
		var fileName = pathArray[pathArray.Length - 1];
		foreach (var t in pathArray)
		{
			if (t != fileName)
			{
				path += t + "/";
			}
		}
	}


	public async void _on_start_pressed()
	{
		if (!_blenderInstallPath.Text.EndsWith("blender.exe"))
		{
			OS.Alert("Please fill in the path to the blender executable at the top of the main window. " +
			         "This is the location you have Blender installed. The file should be called blender.exe.", "Error");
			return;
		}
		
		if (RunningProp)
		{
			return;
		}
		
		_process = new Process();
		RunningLabel.Text = "Starting";
		
		if (UseThreads)
		{
			_thread = new Thread(_on_start_pressed_can_thread);
			_thread.Start();
			Thread.Sleep(0);
			
		}
		else
		{
			_on_start_pressed_can_thread();
		}
	}
	

	public void _on_start_pressed_can_thread()
	{
		var count = 0f;
		foreach (var child in _renderItemContainer.GetChildren())
		{
			if (_stopPressed)
			{
				
				break;
			}
			
			if (child is Control control)
			{
				var outOverride = "";
				var formatOverride = "";
				
				if (control.GetChildCount() > 0 && control.GetChild(0) is LineEdit lineEdit && lineEdit.Text.EndsWith(".blend") && FileAccess.FileExists(lineEdit.Text))
				{
					OptionButton imgOverride = control.GetNode("ImageOverride") as OptionButton;
					OptionButton animOverride = control.GetNode("AnimOverride") as OptionButton;
					
					if (OverridePathLineEdit.Text != "" && DirAccess.DirExistsAbsolute(OverridePathLineEdit.Text)) //_dirAccess.DirExists(OverridePathLineEdit.Text))
					{
						outOverride = " " + "-o" + " " + "\"" + OverridePathLineEdit.Text + "\"";
						if (!outOverride.EndsWith("/")) outOverride += "//";
						{
							outOverride += RemoveBadPathCharacters(lineEdit.Text.Split("/")[^1].Replace(".blend", "")) + "/";
						}
					}
					else if (OverridePathLineEdit.Text != "" && !DirAccess.DirExistsAbsolute(OverridePathLineEdit.Text)) //!_dirAccess.DirExists(OverridePathLineEdit.Text))
					{
						OS.Alert("The output path you have entered is not valid. Please enter a valid path or no path to use the file(s)'s output destination." + OverridePathLineEdit.Text, "Error");
						break;
					}
					var fVAnim = "";
					RunningLabel.Text = "Getting file info";
					SetRunningProp(true);
					switch ((control.GetNode("OptionButton") as OptionButton).Text)
					{
						case "Image":
							if (imgOverride.Text != "NA")
							{
								formatOverride = " " + "-F" + " " + imgOverride.Text.Replace("*", "") + " ";
							}
							
							var frameToRender = (control.GetNode("SpinBox") as SpinBox).Value;
							if (betterProgress && (control.GetNode("SpinBox") as SpinBox).Value == -1)
							{
								while (control.Get("frame_current").AsInt32() == -111)
								{
									Thread.Sleep(500);
								}
								
								frameToRender = control.Get("frame_current").AsInt32();
								
							}
							fVAnim = "-f " + frameToRender;
							break;
						case "Animation":
							if (animOverride.Text != "NA")
							{
								if (animOverride.Text == "MKV")
								{
									formatOverride = "bpy.context.scene.render.image_settings.file_format='FFMPEG'";
								}
								else if (animOverride.Text == "MP4")
								{
									formatOverride = "bpy.context.scene.render.image_settings.file_format='FFMPEG'" +
									                 "\n" + "bpy.context.scene.render.ffmpeg.format='MPEG4'" +
									                 "\n" + "bpy.context.scene.render.ffmpeg.codec='H264'";
								}
								else
								{
									formatOverride = " " + "-F" + " " + animOverride.Text.Replace("*", "") + " ";
								}
							}

							if (betterProgress)
							{
								while (control.Get("frame_start").AsInt32() == -111)
								{
									Thread.Sleep(500);
								}
								
								StartFrame = control.Get("frame_start").AsInt32();
								EndFrame = control.Get("frame_end").AsInt32();
							}

							fVAnim = "-a";
							break;
						default:
							GD.Print("Error, not image or animation");
							return;
					}

					
					Render(lineEdit.Text, outOverride, fVAnim, formatOverride);
					count++;
					
					ProgressBar.Value = count / (_renderItemContainer.GetChildCount() - 1) * 100;
					CurrentProgressBar.Value = 0;
				}
			}

			
		}
		
		ProgressBar.Value = 0;
		if (OffOnFinish && !_stopPressed)
		{
			ProcessStartInfo startinfo = new ProcessStartInfo("shutdown.exe", "-s");
			Process.Start(startinfo);
		}
		_stopPressed = false;
	}
	
	private void Render(string blendPath, string outOverride, string fVAnim, string formatOverride)
	{
		RunningLabel.Text = "Rendering";
		_process.StartInfo.UseShellExecute = false;
		_process.StartInfo.CreateNoWindow = true;
		_process.StartInfo.WorkingDirectory = path;
		_process.StartInfo.RedirectStandardOutput = true;
		_process.StartInfo.FileName = _blenderInstallPath.Text; //"blender.exe";
		_process.StartInfo.Arguments = "-b" + " " + "\"" + blendPath + "\"" + " " + "--python-expr" + " " + scriptPath + formatOverride + outOverride + " " + fVAnim;
		SetRunningProp(true);
		_process.Start();

		while (_process.StandardOutput.EndOfStream == false)
		{
			var output = _process.StandardOutput.ReadLine();
			if (output.Contains("Fra:"))
			{
				var curIndex = 4;
				var tryParse = true;
				var stringDigits = "";
				while (tryParse)
				{
					tryParse = int.TryParse(output[curIndex].ToString(), out int digit);
					if (tryParse)
					{
						stringDigits += output[curIndex].ToString();
						curIndex++;
					}
				}
				CurrentFrame = int.Parse(stringDigits);
			}
			GD.Print(output);
			if (output == String.Empty)
			{
				
				_currentOutputItem = _outputItemScene.Instantiate() as Control;
				_outputItemContainer.AddChild(_currentOutputItem);
				if (MaxLogLines != -1 && _outputItemContainer.GetChildCount() > MaxLogLines)
				{
					_outputItemContainer.GetChild(0).QueueFree();
				}
			}
			(_currentOutputItem as RichTextLabel).Text += "\n" + output;
			Thread.Sleep(100);
		}
		_process.WaitForExit();
		SetRunningProp(false);
	}
	
	public void _on_stop_process_pressed()
	{
		if (RunningProp)
		{
			_stopPressed = true;
			_process.Kill();
			SetRunningProp(false);
		}
	}

	public override void _ExitTree()
	{
		base._ExitTree();
		if (RunningProp && _killProcessWithGodot)
		{
			_process.Kill();
		}
	}

	public void UpdateConfig()
	{
		_killProcessWithGodot = Config.Get("persist_on_close").AsBool();
		UseThreads = Config.Get("separate_thread").AsBool();
		OffOnFinish = Config.Get("shutdown_finish").AsBool();
		MaxLogLines = Config.Get("max_log_lines").AsInt32();
	}
	
	public void _on_clear_logs_pressed()
	{
		_currentOutputItem = _outputItemScene.Instantiate() as Control;
		_outputItemContainer.AddChild(_currentOutputItem);
		foreach (var child in _outputItemContainer.GetChildren())
		{
			if (child != _currentOutputItem) child.QueueFree();
		}
	}

	public void FileAdded() //(string path, string nodeName)
	{
		if (_currentSearches >= _maxSearches)
		{
			return;
		}
		_currentSearches += 1;
		var newThread = new Thread(GetInfo);
		newThread.Start();
	}

	public void GetInfo()
	{

		Thread.Sleep(Rng.RandiRange(100, 500));
		var pathN = "";
		Node cN = null;
		foreach (var c in _renderItemContainer.GetChildren())
		{
			if (c is Button) continue;
			if (c.Get("locked").AsBool() || !(c.Get("searching_panel").AsGodotObject() as Control).Visible) continue;
			pathN = (c.Get("path").AsGodotObject() as LineEdit).Text;
			cN = c;
			break;
		}

		if (cN == null)
		{
			return;
		}

		
		var _newProcess = new Process();
		_newProcess.StartInfo.Arguments =
			"-b" + " " + "\"" + pathN + "\"" + " " + "--python-expr" + " " + scriptPath;
		_newProcess.StartInfo.UseShellExecute = false;
		_newProcess.StartInfo.CreateNoWindow = true;
		_newProcess.StartInfo.WorkingDirectory = path;
		_newProcess.StartInfo.RedirectStandardOutput = true;
		_newProcess.StartInfo.FileName = _blenderInstallPath.Text;
		_newProcess.Start();
		
		var child = cN as Control;
		child.Set("locked", true);
		while (_newProcess.StandardOutput.EndOfStream == false)
		{
			var infoOutput = _newProcess.StandardOutput.ReadLine();

			if (infoOutput.Contains("frame current"))
			{
				var cf = ""; 
				for (int i = 0; i < infoOutput.Length; i++)
				{
					if (int.TryParse(infoOutput[i].ToString(), out _))
						cf += infoOutput[i].ToString();
				}
				child.Set("frame_current", int.Parse(cf));
				//break;
			}
			else if (infoOutput.Contains("frame start"))
			{
				var sf = ""; 
				for (int i = 0; i < infoOutput.Length; i++)
				{
					if (int.TryParse(infoOutput[i].ToString(), out _))
						sf += infoOutput[i].ToString();
				}
				child.Set("frame_start", int.Parse(sf));
			}
			if (infoOutput.Contains("frame end"))
			{
				var ef = "";
				for (int i = 0; i < infoOutput.Length; i++)
				{
					if (int.TryParse(infoOutput[i].ToString(), out _))
						ef += infoOutput[i].ToString();
				}
				child.Set("frame_end", int.Parse(ef));
				break;
			}
			Thread.Sleep(100);
			
		}
		
		child.Set("locked", false);
		_newProcess.WaitForExit();
		_currentSearches -= 1;
		FileAdded();
	}
	

	public string RemoveBadPathCharacters(string bad)
	{
		bad = bad.Replace(" ", "");
		bad = bad.Replace("-", "");
		
		foreach (char c in Path.GetInvalidPathChars())
		{
			bad = bad.Replace(c.ToString(), "");
		}

		foreach (var c in Path.GetInvalidFileNameChars())
		{
			bad = bad.Replace(c.ToString(), "");
		}

		return bad;
	}
}
