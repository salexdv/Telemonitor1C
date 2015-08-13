/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 9:50
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс для Telegram Message.
	/// </summary>
	public class TelegramMessage
	{
		public TelegramMessage()
		{
		}
		
		/// <summary>
	    /// Уникальный идентификатор сообщения
	    /// </summary>
	    [JsonProperty("message_id")]
	    public int message_id { get; set; }
	    
	    /// <summary>
	    /// Отправитель сообщения
	    /// </summary>
	    [JsonProperty("from")]
	    public TelegramUser from { get; set; }
	    
	    /// <summary>
	    /// Отправитель (чат или пользователь)
	    /// </summary>
	    [JsonProperty("chat")]
	    public TelegramUser chat { get; set; }
	    
	    /// <summary>
	    /// Дата сообщения (unix time)
	    /// </summary>
	    [JsonProperty("date")]
	    public int date { get; set; }
	    
	    /// <summary>
	    /// Текст сообщения
	    /// </summary>
	    [JsonProperty("text")]
	    public string text { get; set; }
	    
	    public override string ToString()
        {
	    	return text.Trim();
	    }
	}
}
