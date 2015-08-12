/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 10:20
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace Telemonitor
{
	/// <summary>
	/// Класс для хранения всех настроек
	/// </summary>
	public class Settings
	{
		private bool settingsExists;
		private string botToken;
		private int interval;
		private bool useProxy;
		private string proxyServer;
		private int proxyPort;
		private string proxyUser;
		private string proxyPass;
		private List<DBStruct> bases;
		private Dictionary<string, Command> commands;
		private bool debug;
		
		/// <summary>
        /// Получает настройку из ini файла, преобразуя значение к заданному типу        
        /// <PARAM name="iniSettings">Файл ini</PARAM>
        /// <PARAM name="section">Секция</PARAM>
        /// <PARAM name="key">Ключ</PARAM>
        /// <PARAM name="typeOfValue">Тип значения</PARAM>
        /// </summary>
        private object IniReadValue(iniFile iniSettings, string section, string key, Type typeOfValue)
        {
        	string iniValue = iniSettings.IniReadValue(section, key);
			
        	if (typeOfValue == typeof(int)) {
        		if (String.IsNullOrEmpty(iniValue)) {
        			iniValue = "0";
        		}
        	}
        	else if (typeOfValue == typeof(bool)) {
        		if (iniValue == "0") {
        			iniValue = "false";
        		}
        		else if (iniValue == "1") {
        			iniValue = "true";
        		}
        	}
        	
        	try
			{
				return Convert.ChangeType(iniValue, typeOfValue);
			}
			catch
			{
				throw new Exception("Не удалось прочитать значения параметра \"" + section + ":" + key + "\"");				
			}
    			
        }
		
		/// <summary>
        /// Проверяет существование файла ini с настройками.
        /// При необходимости создает его.
        /// <PARAM name="runPath">Путь исполняемого файла</PARAM>
        /// </summary>
		private bool CheckMainINI(string runPath)
		{						
			bool iniOK = false;
			
			string fileName = runPath + "settings.ini";
			iniFile iniSettings = new iniFile(fileName);
			
			if (File.Exists(fileName)) {
				
				try
				{
					this.botToken = (string)IniReadValue(iniSettings, "Main", "BotToken", typeof(string));
					this.interval = (int)IniReadValue(iniSettings, "Main", "Interval", typeof(int));
					this.useProxy = (bool)IniReadValue(iniSettings, "Proxy", "UseProxy", typeof(bool));
					this.proxyServer = (string)IniReadValue(iniSettings, "Proxy", "Server", typeof(string));
					this.proxyPort = (int)IniReadValue(iniSettings, "Proxy", "Port", typeof(int));
					this.proxyUser = (string)IniReadValue(iniSettings, "Proxy", "Username", typeof(string));
					this.proxyPass = (string)IniReadValue(iniSettings, "Proxy", "Password", typeof(string));
					this.debug = (bool)IniReadValue(iniSettings, "Debug", "Enabled", typeof(bool));
					
					if (!String.IsNullOrEmpty(botToken)) {
						if (this.interval == 0)
							this.interval = 1;
						iniOK = true;
					}
					else
						Logger.Write("Settings.ini: не заполнен токен бота", true);
										
				}
				catch(Exception e)
				{
					Logger.Write("Ошибка Settings.ini: " + e.Message, true);
				}
			}
			else {				
				
				try
				{
					iniSettings.IniWriteValue("Main", "BotToken", "");
					iniSettings.IniWriteValue("Main", "Interval", "1");				
					iniSettings.IniWriteValue("Proxy", "UseProxy", "0");
					iniSettings.IniWriteValue("Proxy", "Server", "");
					iniSettings.IniWriteValue("Proxy", "Port", "");
					iniSettings.IniWriteValue("Proxy", "Username", "");
					iniSettings.IniWriteValue("Proxy", "Password", "");
					iniSettings.IniWriteValue("Debug", "Enabled", "0");
					
					Logger.Write("Создан файл настроек \"settings.ini\"");
				}
				catch
				{
					Logger.Write("Не удалось создать файл \"settings.ini\"", true);	
				}				
				
			}
				
			
			return iniOK;			
			
		}
		
		/// <summary>
        /// Возвращает список команд для базы данных        
        /// <PARAM name="commandDir">Каталог команд для базы данных</PARAM>
        /// </summary>
        private List<DBCommand> GetDbCommands(string commandDir)
		{
        	List<DBCommand> commands = new List<DBCommand>();
        	
        	string[] cmdFiles = Directory.GetFiles(commandDir, "*.tcm");
        	
        	foreach (string cmdFile in cmdFiles) {
        		
        		string commandName = Path.GetFileNameWithoutExtension(cmdFile);
        		commandName = commandName.Replace(" ", "");
        		commandName = commandName.Replace("_", "");
        		
        		try
        		{
        			StreamReader reader = File.OpenText(cmdFile);
        			string commandDescr = reader.ReadLine();
        			
        			if (commandDescr != null)
        			{
        				string commandCode = reader.ReadToEnd();
        				reader.Close();
        				
        				if (!String.IsNullOrEmpty(commandCode)) {        				
        					DBCommand newCommand = new DBCommand();
        					newCommand.Name = commandName;
        					newCommand.Description = commandDescr;
        					newCommand.Code = commandCode;
        					commands.Add(newCommand);
        				}
        				else {
        					Logger.Write(cmdFile + ": файл не содержит кода команды", true);
        				}
        				
        			}
        			else
        			{
        				Logger.Write(cmdFile + ": файл не содержит записей", true);
        			}
        			
        		}
        		catch
        		{
        			Logger.Write(cmdFile + ": не удалось прочитать файл", true);
        		}        		
        	}
        	
        	return commands;
		}
		
		/// <summary>
        /// Загружает настройки для каждой базы данных из каталога databases        
        /// <PARAM name="databases">Массив с именами каталогов</PARAM>
        /// </summary>
		private bool GetDbSettings(string[] databases)
		{
			
			this.bases = new List<DBStruct>();
			this.commands = new Dictionary<string, Command>();
			
			foreach (string dir in databases) {
								
				string dbName = new DirectoryInfo(dir).Name;
				dbName = dbName.Replace(" ", "");
				dbName = dbName.Replace("_", "");
				
				string dirName = Service.CheckPath(dir);
				
				string fileName = dirName + "database.ini";

				if (File.Exists(fileName)) {
					
					iniFile iniSettings = new iniFile(fileName);	
					
					bool baseOK = true;					
					string conString = "";
					int dbVersion = 0;
					
					try
					{						
						conString = (string)IniReadValue(iniSettings, "Base", "ConnectionString", typeof(string));
						dbVersion = (int)IniReadValue(iniSettings, "Base", "Version", typeof(int));
					}
					catch (Exception e)
					{
						Logger.Write(fileName + ": " + e.Message, true);
						baseOK = false;
					}
					
					if (baseOK) {
												
						if (String.IsNullOrEmpty(conString)) {
							Logger.Write(fileName + ": не указана строка соединения с базой данных", true);
							baseOK = false;
						}
						if (dbVersion == 0) {
							Logger.Write(fileName + ": не указана версия 1С для базы данных", true);
							baseOK = false;
						}
						else if (!(82 <= dbVersion && dbVersion <= 83)) {
							Logger.Write(fileName + ": указана неизвестная версия 1С", true);
							baseOK = false;
						}
						
						if (baseOK) {
																												
							List<DBCommand> commands = GetDbCommands(dirName);
							
							if (commands.Count > 0) {
								
								DBStruct newBase = new DBStruct();																										
								newBase.Name = dbName;
								newBase.ConnectionString = conString;
								newBase.Version = dbVersion;
								newBase.Commands = commands;
								this.bases.Add(newBase);
								
								foreach (DBCommand cmd in commands) {
									string commandID = "/" + dbName + "_" + cmd.Name;									
									Command baseCmd = new Command();
									baseCmd.ID = commandID; 
									baseCmd.Description = cmd.Description;
									baseCmd.Code = cmd.Code;
									baseCmd.Version = newBase.Version;
									baseCmd.ConnectionString = newBase.ConnectionString;
									this.commands.Add(commandID.ToLower(), baseCmd);
								}
							}
							else {
								Logger.Write(dirName + ": нет ни одной команды для выполнения", true);
							}																						
							
						}
						
					}
				}
				else {
					Logger.Write(dirName + ": не найден файл \"database.ini\"", true);
				}
				
			}
			
			return (bases.Count > 0);
		}
		
		/// <summary>
        /// Проверяет существование настроек для баз данных        
        /// <PARAM name="runPath">Путь исполняемого файла</PARAM>
        /// </summary>
		private bool CheckDbSettings(string runPath)
		{						
			bool dbOK = false;
			
			string dirBaseName = runPath + "databases\\";			
			
			if (Directory.Exists(dirBaseName)) {
				
				string[] databases = Directory.GetDirectories(dirBaseName);
				
				if (databases.Length > 0)
					dbOK = GetDbSettings(databases);
				else
					Logger.Write("Нет ни одной настройки для баз данных в каталоге \"databases\"", true);	
				
			}
			else {				
						
				try				
				{
					Directory.CreateDirectory(dirBaseName);
					Logger.Write("Создан каталог \"databases\"");
				}
				catch
				{
					Logger.Write("Не удалось создать каталог \"databases\"");
				}				
				
			}
				
			
			return dbOK;			
			
		}
		
		/// <summary>
        /// Конструктор класса        
        /// </summary>
		public Settings()
		{		
			string runPath = Service.CheckPath(System.Windows.Forms.Application.StartupPath);
						
			bool iniOK = CheckMainINI(runPath);
			bool dbOK = CheckDbSettings(runPath);
			
			this.settingsExists = (iniOK && dbOK);
		}
		
		/// <summary>
        /// Указывает на существование настроек
        /// </summary>
		public bool SettingsExists
		{
			get 
			{
				return this.settingsExists;
			}
		}
		
		/// <summary>
        /// Токен бота Telegram
        /// </summary>
		public string BotToken
		{
			get 
			{
				return this.botToken;
			}
		}

		/// <summary>
        /// Время опроса бота в секундах
        /// </summary>
		public int Interval
		{
			get 
			{
				return this.interval;
			}
		}
		
		/// <summary>
        /// Использовать/не использовать прокси-сервер
        /// </summary>
		public bool UseProxy
		{
			get 
			{
				return this.useProxy;
			}
		}
		
		/// <summary>
        /// Имя прокси-сервера
        /// </summary>
		public string ProxyServer
		{
			get 
			{
				return this.proxyServer;
			}
		}
		
		/// <summary>
        /// Порт прокси-сервера
        /// </summary>
		public int ProxyPort
		{
			get 
			{
				return this.proxyPort;
			}
		}
		
		/// <summary>
        /// Имя пользователя прокси-сервера
        /// </summary>
		public string ProxyUser
		{
			get 
			{
				return this.proxyUser;
			}
		}
		
		/// <summary>
        /// Пароль пользователя прокси-сервера
        /// </summary>
		public string ProxyPass
		{
			get 
			{
				return this.proxyPass;
			}
		}
		
		/// <summary>
        /// Признак отладки
        /// </summary>
		public bool Debug
		{
			get 
			{
				return this.debug;
			}
		}
		
		/// <summary>
        /// Возвращает команду по имени. Если такой команды нет, возвращается null
        /// <PARAM name="commandName">Имя команды</PARAM>
        /// </summary>
        public object GetCommandByName(string commandName)
		{
        	Command result = new Command();
        	if (this.commands.TryGetValue(commandName, out result))
        		return result;        	
        	else
        		return null;
						
		}
		
		/// <summary>
        /// Возвращает список команд с описаниями
        /// </summary>
        public string GetCommands()
		{
			string strCommands = "/screen - сделать скриншот";
        	
			foreach (KeyValuePair<string, Command> element in this.commands) {

				if (!String.IsNullOrEmpty(strCommands))
					strCommands += "\r\n";
				
				strCommands += element.Value.ID + " - " + element.Value.Description;
				
			}
			
			return strCommands;			
		}
		
	}
}
