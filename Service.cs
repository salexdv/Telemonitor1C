/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 10:34
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Telemonitor
{
	/// <summary>
	/// Служебные функции
	/// </summary>
	public static class Service
	{
		/// <summary>
		/// При необходимости добавляет "\" в конец переданного пути
		/// </summary>
		public static string CheckPath(string path)
		{
			if (path.Length > 0 && !path.EndsWith("\\")){
				return path += "\\";
			}
			
			return path;
		}
	}
}
