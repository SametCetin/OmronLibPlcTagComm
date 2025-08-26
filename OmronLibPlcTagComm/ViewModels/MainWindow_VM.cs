using CommunityToolkit.Mvvm.Input;
using libplctag;
using libplctag.DataTypes.Simple;
using libplctag.NativeImport;
using OmronLibPlcTagComm.Interfaces;
using PropertyChanged;

namespace OmronLibPlcTagComm.ViewModels;

[AddINotifyPropertyChangedInterface]
public class MainWindow_VM
{
	private readonly IMessageService _messageService;

	public IRelayCommand ReadPlcVarCommand { get; }
	public IRelayCommand WritePlcVarCommand { get; }

	private TagReal testTag;
	private TagReal testTagWr;

	public MainWindow_VM() { }

	public MainWindow_VM(IMessageService messageService)
	{
		_messageService = messageService;
		ReadPlcVarCommand = new RelayCommand(ReadVar);
		WritePlcVarCommand = new RelayCommand(WriteVar);

		_ = CounterAsync();
	}

	public string StatusText { get; set; } = "Not connected";
	public string PlcIpAddress { get; set; } = "192.168.250.1";
	public int CounterValue { get; set; } = 0;

	public string PlcVarName { get; set; } = "DB.HMI.AxisX_SetPosition";
	public string PlcVariableType { get; set; } = "INT";
	public List<string> PlcVariableTypes { get; } =
	[
		"BOOL", "INT", "UINT", "REAL", "LREAL", "STRING"
	];

	public string ValueReadFromPlc { get; set; } = "Empty";
	public string ValueWriteToPlc { get; set; } = "Empty";



	private void ReadVar()
	{
		if (string.IsNullOrEmpty(PlcVarName) || string.IsNullOrEmpty(PlcIpAddress) || string.IsNullOrEmpty(PlcVariableType))
		{
			return;
		}

		var res = ReadPlcVar(PlcVarName, PlcVariableType);
		if (res.success)
		{
			ValueReadFromPlc = res.value.ToString() ?? "Null"; // Ensure null safety by providing a fallback value
		}
		else
			ValueReadFromPlc = "Read Error";
	}

	// plctag wrapper
	private static dynamic ReadAny(string plcVarType, Tag tag)
	{
		object value = plcVarType switch
		{
			var t when plcVarType == "BOOL" => tag.GetBit(0),
			var t when plcVarType == "INT" => tag.GetInt16(0),
			var t when plcVarType == "UINT" => tag.GetUInt16(0),
			var t when plcVarType == "REAL" => tag.GetFloat32(0),
			var t when plcVarType == "LREAL" => tag.GetFloat64(0),
			var t when plcVarType == "STRING" => tag.GetString(0),
			_ => throw new NotSupportedException($"Type {plcVarType} not supported")
		};

		return value;
	}


	// Generic read method
	private readonly object _lockRead = new();
	private (bool success, dynamic value) ReadPlcVar(string varName, string varType)
	{
		lock (_lockRead)
		{
			Tag? plcTag = null;
			try
			{
				plcTag = new Tag
				{
					Name = varName,
					Gateway = PlcIpAddress,
					Path = "1,0",
					PlcType = PlcType.Omron,
					Protocol = Protocol.ab_eip,
					Timeout = TimeSpan.FromMilliseconds(500)
				};

				if (plcTag == null)
					throw new Exception("Error occured when creating Tag");
				if (!plcTag.IsInitialized)
					plcTag.Initialize();
				var value = ReadAny(varType, plcTag);
				return (true, value);
			}
			catch (Exception ex)
			{
				StatusText = ex.Message;
				return (false, null);
			}
			finally
			{
				plcTag?.Dispose();
				plcTag = null;
			}
		}
	}


	private static void WriteAny(string plcVarType, Tag tag, dynamic value)
	{

		switch (plcVarType)
		{
			case "BOOL":
				tag.SetBit(0, Convert.ToBoolean(value));
				break;
			case "INT":
				tag.SetInt16(0, Convert.ToInt16(value));
				break;
			case "UINT":
				tag.SetUInt16(0, Convert.ToUInt16(value));
				break;
			case "REAL":
				tag.SetFloat32(0, Convert.ToSingle(value));
				break;
			case "LREAL":
				tag.SetFloat64(0, Convert.ToDouble(value));
				break;
			case "STRING":
				tag.SetString(0, value?.ToString() ?? "");
				break;
			default:
				throw new NotSupportedException($"Type {plcVarType} not supported");
		}
	}


	private readonly object _lockWrite = new();
	private void WriteVar()
	{
		if (string.IsNullOrEmpty(PlcVarName) || string.IsNullOrEmpty(PlcIpAddress))
		{
			return;
		}

		lock (_lockWrite)
		{
			Tag? plcTag = null;

			try
			{
				plcTag = new Tag
				{
					Name = PlcVarName,
					Gateway = PlcIpAddress,
					Path = "1,0",
					PlcType = PlcType.Omron,
					Protocol = Protocol.ab_eip,
					Timeout = TimeSpan.FromMilliseconds(500)
				};

				if (plcTag == null)
					throw new Exception("Error occured when creating Tag");
				if (!plcTag.IsInitialized)
					plcTag.Initialize();

				WriteAny(PlcVariableType, plcTag, ValueWriteToPlc);
				plcTag.Write();
			}
			catch (Exception ex)
			{
				ValueWriteToPlc = ex.Message;
			}
			finally
			{
				plcTag?.Dispose();
				plcTag = null;
			}
		}


	}

	public async Task CounterAsync()
	{
		await Task.Delay(1000);

		while (true)
		{
			CounterValue++;
			await Task.Delay(1000);
		}
	}
}
