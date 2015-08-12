/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 16:33
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Telemonitor
{
	/// <summary>
	/// Класс для хранения команды и сообщения, запросившего выполнение команды
	/// </summary>
	public class TelegramCommand
	{
		/// <summary>
		/// Конструктор класса
		/// </summary>
		public TelegramCommand()
		{
		}
		
		/// <summary>
	    /// Сообщение с командой
	    /// </summary>	    
	    public TelegramMessage message { get; set; }
	    
	    /// <summary>
	    /// Команда для выполнения
	    /// </summary>	    
	    public Command command { get; set; }
	}
}
