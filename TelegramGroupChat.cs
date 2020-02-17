/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 9:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс для Telegram GroupChat
	/// </summary>
	public class TelegramGroupChat
	{
		public TelegramGroupChat()
		{
		}
		
		/// <summary>
	    /// Уникальный идентификатор группового чата
	    /// </summary>
	    [JsonProperty("id")]
	    public Int64 id { get; set; }
	    
	    /// <summary>
	    /// Заголовок чата
	    /// </summary>
	    [JsonProperty("title")]
	    public string title { get; set; }
	}
}
