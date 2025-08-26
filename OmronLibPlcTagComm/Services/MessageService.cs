using OmronLibPlcTagComm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OmronLibPlcTagComm.Services;

public class MessageService:IMessageService
{
	public void ShowMessage(string message)
	{
		MessageBox.Show(message);
	}
}
