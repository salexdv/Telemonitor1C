﻿/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 9:32
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Telemonitor
{
	/// <summary>
	/// Класс для Telegram Answer.
	/// </summary>
	public class TelegramAnswer
	{
		public TelegramAnswer()
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
	    public List<TelegramUpdate> updates { get; set; }	
	}
}
