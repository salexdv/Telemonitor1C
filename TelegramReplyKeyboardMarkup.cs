/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.11.2015
 * Time: 12:11
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс для доп.клавиатуры
	/// </summary>
	public class TelegramReplyKeyboardMarkup
	{
		public TelegramReplyKeyboardMarkup()
		{
		}
				 	  		
		/// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("resize_keyboard")]
	    public bool resize_keyboard { get; set; }
		
	    /// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("keyboard")]
	    public List<List<string>> keyboard { get; set; }
	}
}
