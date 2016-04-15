/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 14.04.2016
 * Time: 12:54
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Description of TelegramForceReply.
	/// </summary>
	public class TelegramForceReply
	{
		public TelegramForceReply()
		{
		}
		
		/// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("force_reply")]
	    public bool force_reply { get; set; }
		
	    /// <summary>
	    /// Результаты
	    /// </summary>
	    [JsonProperty("selective")]
	    public bool selective { get; set; }
	}
}
