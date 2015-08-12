/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 9:44
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс для Telegram User
	/// </summary>
	public class TelegramUser
	{
		public TelegramUser()
		{
		}
		
		/// <summary>
	    /// Уникальный идентификатор пользователя или бота
	    /// </summary>
	    [JsonProperty("id")]
	    public int id { get; set; }
	    
	    /// <summary>
	    /// Имя пользователя
	    /// </summary>
	    [JsonProperty("first_name")]
	    public string first_name { get; set; }
	    
	    /// <summary>
	    /// Фамилия пользователя
	    /// </summary>
	    [JsonProperty("last_name")]
	    public string last_name { get; set; }
	    
	    /// <summary>
	    /// username пользователя
	    /// </summary>
	    [JsonProperty("username")]
	    public string username { get; set; }
	}
}
