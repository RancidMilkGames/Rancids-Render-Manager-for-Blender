//This file is part of Rancid's Render Manager.

//Rancid's Render Manager is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.

//Rancid's Render Manager is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

//You should have received a copy of the GNU General Public License along with Rancid's Render Manager. If not, see <https://www.gnu.org/licenses/>. 

//This program uses small bits of Blender's API. Blender is 

using System;
using Godot;
using System.Diagnostics;
using System.Threading;
using Godot.Collections;
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
	
	public int StartFrame = 0;
	public int EndFrame = 0;
	private int _currentFrame = 0;
	private int MaxLogLines = 1000;

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
		_mainPanel.Visible = !value;
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
	

	public /*async*/ void _on_start_pressed_can_thread()
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
				if (control.GetChildCount() > 0 && control.GetChild(0) is LineEdit lineEdit && lineEdit.Text.EndsWith(".blend") && FileAccess.FileExists(lineEdit.Text))
				{
					var fVAnim = "";
					switch ((control.GetNode("OptionButton") as OptionButton).Text)
					{
						case "Image":
							fVAnim = "-f " + (control.GetNode("SpinBox") as SpinBox).Value;
							break;
						case "Animation":

							if (betterProgress)
							{
								RunningLabel.Text = "Getting file info";
								_process.StartInfo.Arguments =
									"-b" + " " + lineEdit.Text + " " + "--python-expr" + " " + scriptPath;
								SetRunningProp(true);
								_process.StartInfo.UseShellExecute = false;
								_process.StartInfo.CreateNoWindow = true;
								_process.StartInfo.WorkingDirectory = path;
								_process.StartInfo.RedirectStandardOutput = true;
								_process.StartInfo.FileName = _blenderInstallPath.Text; //"blender.exe";
								_process.Start();
								StartFrame = 0;
								while (_process.StandardOutput.EndOfStream == false)
								{
									_output = _process.StandardOutput.ReadLine();
									
									if (_output.Contains("frame start"))
									{
										var sf = ""; 
										for (int i = 0; i < _output.Length; i++)
										{
											if (int.TryParse(_output[i].ToString(), out _))
												sf += _output[i].ToString();
										}
										StartFrame = int.Parse(sf);
									}
									else if (_output.Contains("frame end"))
									{
										var ef = ""; //_output[_output.Length - 1].ToString();
										for (int i = 0; i < _output.Length; i++)
										{
											if (int.TryParse(_output[i].ToString(), out _))
												ef += _output[i].ToString();
										}
										EndFrame = int.Parse(ef);
										_animFrames = EndFrame - StartFrame;
										break;
								 	}
								}
							}
							
							
							fVAnim = "-a";
							break;
						default:
							GD.Print("Error, not image or animation");
							return;
					}

					
					Render(lineEdit.Text, fVAnim);
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
	
	private void Render(string blendPath, string fVAnim)
	{
		RunningLabel.Text = "Rendering";
		_process.StartInfo.UseShellExecute = false;
		_process.StartInfo.CreateNoWindow = true;
		_process.StartInfo.WorkingDirectory = path;
		_process.StartInfo.RedirectStandardOutput = true;
		_process.StartInfo.FileName = _blenderInstallPath.Text; //"blender.exe";
		_process.StartInfo.Arguments = "-b" + " " + blendPath + " " + "--python-expr" + " " + scriptPath + " " + fVAnim;
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
	
}
