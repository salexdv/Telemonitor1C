/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 12.08.2015
 * Time: 14:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Telemonitor
{
	/// <summary>
	/// Класс для ответа от V8Connector
	/// </summary>
	public class V8Answer
	{
		/// <summary>
		/// Текст ответа
		/// </summary>
		public string Text {get; set;}
		
		/// <summary>
		/// Имя возвращаемого файла
		/// </summary>
		public string FileName {get; set;}
				
		/// <summary>
		/// Создает объект V8Answer 
		/// </summary>
		/// <param name="txt">Текст ответа</param>
		/// <param name="fName">Возвращаемый файл</param>
		public V8Answer(string txt, string fName)
		{
			Text = txt;
			FileName = fName;
		}
	}
}
