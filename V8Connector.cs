/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 15:04
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using System.Threading;
using System.Runtime.InteropServices;

namespace Telemonitor
{
	/// <summary>
	/// Класс для запуска 1С и выполнения команд
	/// </summary>
	public class V8Connector
	{
		/// <summary>
		/// Команда, которая будет выполнятся методом Execute
		/// </summary>
		private Command excCommand;
		
		/// <summary>
		/// Имя пользователя Telegram
		/// </summary>
		private string t_username;
		
		/// <summary>
		/// first_name пользователя Telegram
		/// </summary>
		private string t_first_name;
		
		/// <summary>
		/// last_name пользователя Telegram
		/// </summary>
		private string t_last_name;
		
		/// <summary>
		/// Параметры выполняемой команды
		/// </summary>
		private string excParams;
		
		/// <summary>
		/// Тип для создания ComConnector
		/// </summary>
		private Type v80Type;
		
		/// <summary>
		/// Соединение через ComConnector;
		/// </summary>
        private object Connection;
        
        /// <summary>
        /// Признак успешного выполнения команды
        /// </summary>
        private bool success;
        
        /// <summary>
        /// Признак запуска кода в безопасном режиме 1С
        /// </summary>
        private bool safeMode1C;

        private static BindingFlags FlagsSetProrerty = BindingFlags.Public | BindingFlags.Static | BindingFlags.SetProperty;
        private static BindingFlags FlagsGetProperty = BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty;
        private static BindingFlags FlagsMethod = BindingFlags.Public
        | System.Reflection.BindingFlags.Public
        | System.Reflection.BindingFlags.NonPublic
        | System.Reflection.BindingFlags.GetProperty
        | System.Reflection.BindingFlags.GetField
        | System.Reflection.BindingFlags.InvokeMethod
        | System.Reflection.BindingFlags.IgnoreCase
        | System.Reflection.BindingFlags.Instance
        | System.Reflection.BindingFlags.Static;

        /// <summary>
        /// Возвращает список команд с описаниями
        /// </summary>
        public bool Success
		{
			get
			{
				return this.success;
			}
						
		}
        
        /// <summary>
        /// Имя пользователя Telegram
        /// </summary>
        public string TelegramUserName
		{
			get
			{
				return this.t_username;
			}
			set
			{
				this.t_username = value;
			}
						
		}
        
        /// <summary>
        /// first_name пользователя Telegram
        /// </summary>
        public string TelegramFirstName
		{
			get
			{
				return this.t_first_name;
			}
			set
			{
				this.t_first_name = value;
			}
						
		}
        
        /// <summary>
        /// last_name пользователя Telegram
        /// </summary>
        public string TelegramLastName
		{
			get
			{
				return this.t_last_name;
			}
			set
			{
				this.t_last_name = value;
			}
						
		}
			
		/// <summary>
        /// Конструктор класса		
		/// <PARAM name="cmdObj">Команда</PARAM>		
		/// <PARAM name="parameters">Параметры команды</PARAM>
        /// </summary>		        
		public V8Connector(Command cmdObj, string parameters, bool safeMode1C)
		{			
			this.excCommand = cmdObj;			
			this.excParams = parameters;
			this.safeMode1C = safeMode1C;
		}
		
		/// <summary>
        /// Выполняет команду через COMConnector		       
        /// </summary>		
		public V8Answer Execute(Mutex mutLogger)
		{
			string result = "";
			string fName = "";
			bool isDialog = false; 			
			string v8version = this.excCommand.Version.ToString();
			
			string runPath = Service.CheckPath(System.Windows.Forms.Application.StartupPath);
			runPath = Service.CheckPath(runPath);			
			
			this.success = true;
			
			v80Type = Type.GetTypeFromProgID("V" + v8version + ".COMConnector");
            object v8Connector = Activator.CreateInstance(v80Type);
            try
            {            	
            	Connection = v80Type.InvokeMember("Connect", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.InvokeMethod, null, v8Connector, new object[1] { this.excCommand.ConnectionString });            	
                object externalData = GetProperty(null, "ExternalDataProcessors");                
                object executer = Method(externalData, "Create", new object[2] { runPath + "executer" + v8version + ".tep", this.safeMode1C });               
                SetProperty(executer, "Код", this.excCommand.Code);
				SetProperty(executer, "ПараметрыКоманды", this.excParams);
				SetProperty(executer, "username", this.t_username);
				SetProperty(executer, "first_name", this.t_first_name);
				SetProperty(executer, "last_name", this.t_last_name);
				SetProperty(executer, "command", this.excCommand.ID);
                Method(executer, "ExecuteCode", new object[0]);                
                result = (string)GetProperty(executer, "Результат");
				fName = (string)GetProperty(executer, "Результат_Файл");
				isDialog = (bool)GetProperty(executer, "ДиалогСПараметрами");
                Marshal.Release(Marshal.GetIDispatchForObject(executer));
                Marshal.Release(Marshal.GetIDispatchForObject(externalData));
                Marshal.ReleaseComObject(executer);
                Marshal.ReleaseComObject(externalData);
                executer = null;
                externalData = null;
            }
            catch (Exception e)
            {            	
            	string errorDescr = e.Message;
                if (e.InnerException != null)
                {
                    errorDescr = errorDescr + "\r\n" + e.InnerException.Message;
                }
                
            	Logger.Write(String.Format("Не удалось выполнить команду \"{0}\": {1}", this.excCommand.ID, errorDescr), true, mutLogger);
				this.success = false;
            }  
          
            Marshal.Release(Marshal.GetIDispatchForObject(Connection));
            Marshal.ReleaseComObject(Connection);
            Marshal.Release(Marshal.GetIDispatchForObject(v8Connector));
            Marshal.ReleaseComObject(v8Connector);
            
            Connection = null;
            v80Type = null;
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
            
            return new V8Answer(result, fName, isDialog);
		}
		
		/// <summary>
        /// Деструктор       
        /// </summary>		
		public void Dispose()
        {                                   
            Connection = null;
            v80Type = null;           
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
        
		/// <summary>
        /// Установка реквизита объекта 1С       
        /// </summary>		
        private void SetProperty(object Obj, string PropertyName, string PropertyValue)
        {
            v80Type.InvokeMember(PropertyName, FlagsSetProrerty, null, Obj, new object[1] { PropertyValue });
        }
        
        /// <summary>
        /// Получение реквизита объекта 1С       
        /// </summary>		
        private object GetProperty(object Obj, string PropertyName)
        {
            if (Obj != null)
                return v80Type.InvokeMember(PropertyName, FlagsGetProperty, null, Obj, null);
            else
                return v80Type.InvokeMember(PropertyName, FlagsGetProperty, null, Connection, null);
        }
        
        /// <summary>
        /// Выполнение метода объекта 1С       
        /// </summary>		
        private object Method(object Obj, string MethodName, object[] MethodParams)
        {
            if (Obj != null)                
                return v80Type.InvokeMember(MethodName, FlagsMethod, null, Obj, MethodParams);
            else                
                return v80Type.InvokeMember(MethodName, FlagsMethod, null, Connection, MethodParams);
        }
        
        /// <summary>
        /// Создание объекта 1С       
        /// </summary>		
        private object CreateObject(string ObjectName, object[] param)
        {
            if (param.Length == 0)
                return v80Type.InvokeMember("NewObject", FlagsMethod, null, Connection, new object[1] { ObjectName });
            else
            {
                object[] p = new object[param.Length + 1];
                p[0] = ObjectName;
                int i = 1;
                foreach (object obj in param)
                {
                    p[i] = obj;
                    i++;
                }
                return v80Type.InvokeMember("NewObject", FlagsMethod, null, Connection, p);
            }
        }
		
	}
}
