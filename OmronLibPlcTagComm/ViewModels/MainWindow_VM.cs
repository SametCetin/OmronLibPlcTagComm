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

	public IRelayCommand ConnectCommand { get; }
	public IRelayCommand ReadPlcVarCommand { get; }
	public IRelayCommand WritePlcVarCommand { get; }

	private TagReal testTag;
	private TagReal testTagWr;

	public MainWindow_VM() { }

	public MainWindow_VM(IMessageService messageService)
	{
		_messageService = messageService;
		ConnectCommand = new RelayCommand(Connect);
		ReadPlcVarCommand = new RelayCommand(ReadVar);
		WritePlcVarCommand = new RelayCommand(WriteVar);

		_ = CounterAsync();
	}

	public string StatusText { get; set; } = "Not connected";
	public string PlcIpAddress { get; set; } = "192.168.250.1";
	public int CounterValue { get; set; } = 0;
	public string PlcVarName { get; set; } = "DB.HMI.AxisX_SetPosition";
	public string ValueReadFromPlc { get; set; } = "Empty";
	public string ValueWriteToPlc { get; set; } = "Empty";

	private void Connect()
	{
		PlcIpAddress = "192.168.250.3";
		_messageService.ShowMessage(PlcIpAddress);
	}


	private readonly object _lockRead = new();
	private void ReadVar()
	{
		if (string.IsNullOrEmpty(PlcVarName) || string.IsNullOrEmpty(PlcIpAddress))
		{
			return;
		}

		lock (_lockRead)
		{
			var res = ReadPlcVar<float>(PlcVarName);
			ValueReadFromPlc = res.value.ToString() ?? "Null"; // Ensure null safety by providing a fallback value
		}
	}

	private (bool success, T? value) ReadPlcVar<T>(string varName) where T : struct
	{
		Tag? plcTag = null;
		try
		{
			plcTag = new Tag
			{
				Name = PlcVarName,
				Gateway = PlcIpAddress,
				Path = "1,0",
				PlcType = libplctag.PlcType.Omron,
				Protocol = libplctag.Protocol.ab_eip,
				Timeout = TimeSpan.FromMilliseconds(500)
			};

			plcTag.Read();

			if (typeof(T) == typeof(float))
			{
				var floatValue = plcTag.GetFloat32(0);
				return (true, (T?)(object)floatValue);
			}
			else
			{
				return (false, null);
			}
		}
		catch (Exception ex)
		{
			return (false, null);
		}
		finally
		{
			plcTag?.Dispose();
		}
	}

	private readonly object _lockWrite = new();
	private void WriteVar()
	{
		if (string.IsNullOrEmpty(PlcVarName) || string.IsNullOrEmpty(PlcIpAddress))
		{
			return;
		}

		lock (_lockRead)
		{
			try
			{
				testTagWr = new TagReal()
				{
					Name = PlcVarName,
					Gateway = PlcIpAddress,
					Path = "1,0",
					PlcType = libplctag.PlcType.Omron,
					Protocol = libplctag.Protocol.ab_eip,
					Timeout = TimeSpan.FromMilliseconds(500)
				};

				if (float.TryParse(ValueWriteToPlc, out var valueToWrite))
				{
					testTagWr.Write(valueToWrite);
				}
			}
			catch (Exception ex)
			{
				ValueWriteToPlc = ex.Message;
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
