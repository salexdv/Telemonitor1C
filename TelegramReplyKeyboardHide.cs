/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.11.2015
 * Time: 12:35
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Description of ReplyKeyboardHide.
	/// </summary>
	public class TelegramReplyKeyboardHide
	{
		public TelegramReplyKeyboardHide()
		{
			hide_keyboard = true;
		}
		
		/// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("hide_keyboard")]
	    public bool hide_keyboard { get; set; }	
	}	
}
