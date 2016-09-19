/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 11:36
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Telemonitor
{
	/// <summary>
	/// Description of Logger.
	/// </summary>
	public static class Logger
	{
		/// <summary>
		/// Возвращает имя файла лога
		/// </summary>
		/// <returns>Имя файла лога</returns>
		private static string GetLogFileName()
		{
			string runPath = Service.CheckPath(System.Windows.Forms.Application.StartupPath);			
			return runPath + "telemonitor.log";					
		}
		
		/// <summary>
        /// Записывает событие в лог        
        /// <PARAM name="description">Описание события</PARAM>
        /// <PARAM name="isError">Признак того, что событие является ошибкой</PARAM>
        /// </summary>
		public static void Write(string description, bool isError = false, Mutex mutLogger = null)
		{										
			bool useMutex =  (mutLogger != null);
			
			if (useMutex)
				mutLogger.WaitOne();
			
			string fileName = GetLogFileName();
			
			StreamWriter LogFile = new StreamWriter(fileName, true, Encoding.Unicode);
            LogFile.AutoFlush = true;			
			
            string message = "";
            
            if (isError)
            	message = "! ";
            
            message += DateTime.Now.ToString();
			message += " - " + description;            
            
			LogFile.WriteLine(message);
			LogFile.Close();
			LogFile.Dispose();
			
			if (useMutex)
				mutLogger.ReleaseMutex();
			
		}
		
		/// <summary>
        /// Записывает событие отладки в лог        
        /// <PARAM name="settings">Настройки</PARAM>
        /// <PARAM name="description">Описание события</PARAM>
        /// <PARAM name="isError">Признак того, что событие является ошибкой</PARAM>
        /// </summary>
		public static void Debug(Settings settings, string description, bool isError = false, Mutex mutLogger = null)
		{			
			if (settings.Debug)
				Write("debug (" + Thread.CurrentThread.ManagedThreadId.ToString() + "): " + description, isError, mutLogger);
		}
				
	}
}
