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
        /// Конструктор класса
		/// <PARAM name="cmdName">Имя команды</PARAM>
		/// <PARAM name="cmdObj">Команда</PARAM>		
        /// </summary>		        
		public V8Connector(Command cmdObj)
		{			
			this.excCommand = cmdObj;			
		}
		
		/// <summary>
        /// Выполняет команду через COMConnector		       
        /// </summary>		
		public V8Answer Execute(Mutex mutLogger)
		{
			string result = "";
			string fName = "";
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
                object executer = Method(externalData, "Create", new object[1] { runPath + "executer" + v8version + ".tep" });                
                SetProperty(executer, "Код", this.excCommand.Code);                
                Method(executer, "ExecuteCode", new object[0]);                
                result = (string)GetProperty(executer, "Результат");
				fName = (string)GetProperty(executer, "Результат_Файл");                
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
          
            return new V8Answer(result, fName);
		}
		
		/// <summary>
        /// Деструктор       
        /// </summary>		
		public void Dispose()
        {            
            Marshal.Release(Marshal.GetIDispatchForObject(Connection));
            Marshal.ReleaseComObject(Connection);            
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
