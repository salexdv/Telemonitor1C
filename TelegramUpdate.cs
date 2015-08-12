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
	/// Description of TelegramUpdate.
	/// </summary>
	public class TelegramUpdate
	{
		public TelegramUpdate()
		{
		}
		
		/// <summary>
	    /// Уникальный идентификатор группового чата
	    /// </summary>
	    [JsonProperty("update_id")]
	    public int update_id { get; set; }
	    
	    /// <summary>
	    /// Заголовок чата
	    /// </summary>
	    [JsonProperty("message")]
	    public TelegramMessage message { get; set; }
	}
}
