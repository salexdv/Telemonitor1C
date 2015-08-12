/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 12:12
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс Telegram Answer Message
	/// API возвращает его при отправке Message
	/// </summary>
	public class TelegramAnswerMessage
	{
		public TelegramAnswerMessage()
		{
		}
		
		/// <summary>
	    /// Статус ответа
	    /// </summary>
	    [JsonProperty("ok")]
	    public bool ok { get; set; }
	    
	    /// <summary>
	    /// Описание ответа
	    /// </summary>
	    [JsonProperty("description")]
	    public string description { get; set; }	

		/// <summary>
	    /// Код ошибки
	    /// </summary>
	    [JsonProperty("error_code")]
	    public int error_code { get; set; }		    
	    
	    /// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("result")]
	    public TelegramMessage message { get; set; }	
	}
}
