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
using Newtonsoft.Json;

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
		private Dictionary<string, bool> allowUsers;
		private Dictionary<string, bool> screenOwners;
		private bool debug;
		private bool safeMode1C;
		private bool buttonsShowStart;
		private bool buttonsHideKeyboard;
		private bool buttonsUsePic;
		private int buttonsNumRows;
		private string oscriptPath;
				
		/// <summary>
        /// Получает настройку из ini файла, преобразуя значение к заданному типу        
        /// <PARAM name="iniSettings">Файл ini</PARAM>
        /// <PARAM name="section">Секция</PARAM>
        /// <PARAM name="key">Ключ</PARAM>
        /// <PARAM name="typeOfValue">Тип значения</PARAM>
        /// <PARAM name="exceptIfEmpty">Вызывать исключение, если получено пустое значение параметра</PARAM>
        /// </summary>
        private object IniReadValue(iniFile iniSettings, string section, string key, Type typeOfValue, bool exceptIfEmpty = false)
        {
        	string iniValue = iniSettings.IniReadValue(section, key);
			
        	if (exceptIfEmpty && String.IsNullOrEmpty(iniValue)) {
        		throw new Exception("Не удалось прочитать значения параметра \"" + section + ":" + key + "\"");
        	}
        	        	        	
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
        /// Получает значение параметра из ini-файла.
        /// Если такого параметра нет, то он создается с переданным
        /// значением по умолчанию
        /// </summary>
        private object GetAdditionalParamFromINI(iniFile iniSettings, string section, string key, Type typeOfValue, string defVal)
        {
        	try
			{
        		return IniReadValue(iniSettings, section, key, typeOfValue, true);
        	}
        	catch {
        		iniSettings.IniWriteValue(section, key, defVal);
        		return IniReadValue(iniSettings, section, key, typeOfValue);
        	}
        }
        		
        /// <summary>
        /// Возвращает из строки коллекцию пользователей, 
		/// которым разрешен доступ      
        /// </summary>
        /// <param name="wl_users">Строка с именами пользователей через разделитель</param>
        /// <returns></returns>
        private Dictionary <string, bool> GetWhiteListOfUsers(string wl_users)
        {        	
        	
        	Dictionary <string, bool> result = new Dictionary<string, bool>();
        	
        	string[] users = wl_users.Split(',');
        	
        	foreach (string user in users)
        		if (!String.IsNullOrEmpty(user))
        			result.Add(user, true);
        	
        	return result;
        	
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
					
					this.safeMode1C = (bool)GetAdditionalParamFromINI(iniSettings, "SafeMode1C", "Enabled", typeof(bool), "1");					
					this.buttonsShowStart = (bool)GetAdditionalParamFromINI(iniSettings, "Buttons", "ShowStartButton", typeof(bool), "0");
					this.buttonsHideKeyboard = (bool)GetAdditionalParamFromINI(iniSettings, "Buttons", "HideButtonsAfterMessage", typeof(bool), "1");					
					this.buttonsNumRows = (int)GetAdditionalParamFromINI(iniSettings, "Buttons", "NumRowsOfButtons", typeof(int), "2");
					this.buttonsUsePic = (bool)GetAdditionalParamFromINI(iniSettings, "Buttons", "UsePictures", typeof(bool), "1");													
					this.allowUsers = GetWhiteListOfUsers((string)GetAdditionalParamFromINI(iniSettings, "WhiteList", "Users", typeof(string), ""));
					this.screenOwners = GetWhiteListOfUsers((string)GetAdditionalParamFromINI(iniSettings, "WhiteList", "ScreenOwners", typeof(string), ""));					
					
					this.oscriptPath = (string)GetAdditionalParamFromINI(iniSettings, "Environment", "OneScriptPath", typeof(string), "");
					
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
					iniSettings.IniWriteValue("SafeMode1C", "Enabled", "1");
					iniSettings.IniWriteValue("Buttons", "ShowStartButton", "0");
					iniSettings.IniWriteValue("Buttons", "HideButtonsAfterMessage", "1");
					iniSettings.IniWriteValue("Buttons", "NumRowsOfButtons", "2");
					iniSettings.IniWriteValue("WhiteList", "Users", "");
					iniSettings.IniWriteValue("OneScriptPath", "Environment", "");
					
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
        	List<DBCommand> baseCommands = new List<DBCommand>();
        	
        	string fileExtention = "";
        	string[] cmdFiles = Directory.GetFiles(commandDir, "*.tcm*");
        	
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
        					fileExtention = Path.GetExtension(cmdFile); 
        					if ( fileExtention.ToLower() == ".tcm_b" )
        						newCommand.KeyboardCommand = true;
        					else
        						newCommand.KeyboardCommand = false;
        					baseCommands.Add(newCommand);
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
        	
        	return baseCommands;
		}
		
		/// <summary>
        /// Загружает настройки для каждой базы данных из каталога databases        
        /// <PARAM name="databases">Массив с именами каталогов</PARAM>
        /// </summary>
		private bool GetDbSettings(string[] databases)
		{
			
			this.bases = new List<DBStruct>();			
			
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
					string wl_users = "";
					int dbVersion = 0;
					
					try
					{						
						conString = (string)IniReadValue(iniSettings, "Base", "ConnectionString", typeof(string));
						dbVersion = (int)IniReadValue(iniSettings, "Base", "Version", typeof(int));
						wl_users = (string)GetAdditionalParamFromINI(iniSettings, "WhiteList", "Users", typeof(string), "");						
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
																												
							List<DBCommand> baseCommands = GetDbCommands(dirName);
							
							if (baseCommands.Count > 0) {
								
								DBStruct newBase = new DBStruct();																										
								newBase.Name = dbName;
								newBase.ConnectionString = conString;
								newBase.Version = dbVersion;
								newBase.Commands = baseCommands;
								newBase.AllowUsers = GetWhiteListOfUsers(wl_users);
								this.bases.Add(newBase);
								
								foreach (DBCommand cmd in baseCommands) {									
									string commandID = "/" + dbName + "_" + cmd.Name;									
									Command baseCmd = new Command();
									baseCmd.Type = commandTypes.command1C;
									baseCmd.ID = commandID;
									baseCmd.Description = cmd.Description;
									baseCmd.Code = cmd.Code;
									baseCmd.Version = newBase.Version;
									baseCmd.ConnectionString = newBase.ConnectionString;
									baseCmd.KeyboardCommand = cmd.KeyboardCommand;									
									baseCmd.AllowUsers = newBase.AllowUsers;
									if (cmd.KeyboardCommand)
										commandID += "_keyb";
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
        /// Проверяет существование скриптов        
        /// <PARAM name="runPath">Путь исполняемого файла</PARAM>
        /// </summary>
		private bool CheckScriptsSettings(string runPath)
		{						
			bool scrOK = true;
			
			string dirBaseName = runPath + "scripts\\";			
			
			if (Directory.Exists(dirBaseName)) {
			
				string[] cmdFiles = Directory.GetFiles(dirBaseName, "*.os*");
				
				string iniFileName = dirBaseName + "scripts.ini";
				
				iniFile iniSettings = new iniFile(iniFileName);
				
				string wl_users = (string)GetAdditionalParamFromINI(iniSettings, "WhiteList", "Users", typeof(string), "");
				
				foreach (string cmdFile in cmdFiles) {
					
					string commandName = Path.GetFileNameWithoutExtension(cmdFile);
	        		commandName = commandName.Replace(" ", "");	        		
	        		
	        		try
	        		{
	        			StreamReader reader = File.OpenText(cmdFile);
	        			string commandDescr = reader.ReadLine();
	        			commandDescr.Trim();
	        			if (commandDescr.StartsWith(@"//"))
	        				commandDescr = commandDescr.Substring(3);
	        			
	        			if (commandDescr != null)
	        			{
	        				string commandCode = reader.ReadToEnd();
	        				reader.Close();
	        				
	        				if (!String.IsNullOrEmpty(commandCode)) {
	        					
	        					Command scriptCmd = new Command();
	        					commandName = "/" + commandName;
	        					scriptCmd.Type = commandTypes.commandOScript;
								scriptCmd.ID = commandName;
								scriptCmd.Description = commandDescr;
								scriptCmd.Code = commandCode;
								scriptCmd.ConnectionString = cmdFile;
								scriptCmd.AllowUsers = GetWhiteListOfUsers(wl_users);
								this.commands.Add(commandName.ToLower(), scriptCmd);	        						        					
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
				
			}
			else {
						
				try				
				{
					Directory.CreateDirectory(dirBaseName);
					Logger.Write("Создан каталог \"scripts\"");
				}
				catch
				{
					Logger.Write("Не удалось создать каталог \"scripts\"");
					scrOK = false;
				}				
				
			}
				
			
			return scrOK;			
			
		}
		
		/// <summary>
        /// Конструктор класса        
        /// </summary>
		public Settings()
		{		
			string runPath = Service.CheckPath(System.Windows.Forms.Application.StartupPath);
			this.commands = new Dictionary<string, Command>();
						
			bool iniOK = CheckMainINI(runPath);
			bool dbOK = CheckDbSettings(runPath);
			bool scrOK = CheckScriptsSettings(runPath);
			
			this.settingsExists = (iniOK && (dbOK || scrOK));
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
        /// Признак запуска кода в безопасном режиме 1С
        /// </summary>
		public bool SafeMode1C
		{
			get 
			{
				return this.safeMode1C;
			}
		}
		
		/// <summary>
        /// Признак показа кнопки доп.клавиатуры для запроса списка команд
        /// </summary>
		public bool ShowStartButton
		{
			get 
			{
				return this.buttonsShowStart;
			}
		}
		
		/// <summary>
        /// Скрывать или нет доп.клавиатуру после получения сообщения
        /// </summary>
		public bool HideButtonsAfterMessage
		{
			get 
			{
				return this.buttonsHideKeyboard;
			}
		}
		
		/// <summary>
        /// Скрывать или нет доп.клавиатуру после получения сообщения
        /// </summary>
		public int NumRowsOfButtons
		{
			get 
			{
				return this.buttonsNumRows;
			}
		}
		
		/// <summary>
        /// Скрывать или нет доп.клавиатуру после получения сообщения
        /// </summary>
		public bool UsePicturesAtButtons
		{
			get 
			{
				return this.buttonsUsePic;
			}
		}
		
		/// <summary>
        /// Путь к файлу oscript.exe
        /// </summary>
		public string OScriptPath
		{
			get 
			{
				return this.oscriptPath;
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
        /// <PARAM name="username">Имя пользователя Telegram</PARAM>
        /// </summary>
        public string GetCommands(string username)
		{			
			string strCommands = "";
			
			if (AllowToGetScreenshot(username))
				strCommands = "/screen - сделать скриншот";			
        	
        	foreach (KeyValuePair<string, Command> element in this.commands) {
			
        		if (AllowableUserForCommand(username, element.Value)) {
				
	        		if (!element.Value.KeyboardCommand) {
					
						if (!String.IsNullOrEmpty(strCommands))
							strCommands += "\r\n";
						
						strCommands += element.Value.ID + " - " + element.Value.Description;
					}
        			
        		}
				
			}
								
			return strCommands;			
		}
        
        /// <summary>
        /// Возвращает список команд (кнопок) с описаниями
        /// кнопки располагаются максимум по 2 в ряд
        /// <PARAM name="username">Имя пользователя Telegram</PARAM>
        /// </summary>
        public string GetKeyboardCommands(string username)
		{
			string strCommand = "";
			List<List<string>>  buttons = new List<List<string>>();
        	
			if (ShowStartButton) {
				List<string> array_of_but = new List<string>();
				if (UsePicturesAtButtons)
					array_of_but.Add(Const.PIC_BUTTON_START + "/start");
				else
					array_of_but.Add("/start");
				buttons.Add(array_of_but);
			}
						
			foreach (KeyValuePair<string, Command> element in this.commands) {

				if ( element.Value.KeyboardCommand && AllowableUserForCommand(username, element.Value) )  {
				
					strCommand = element.Value.ID;					
					if (UsePicturesAtButtons)
						strCommand = Const.PIC_BUTTON_OTHER + strCommand;
					
					if (buttons.Count == 0) {
						List<string> array_of_but = new List<string>();
						array_of_but.Add(strCommand);
						buttons.Add(array_of_but);
					}
					else
					{
						if (buttons[buttons.Count - 1].Count == NumRowsOfButtons) {
							List<string> array_of_but = new List<string>();
							array_of_but.Add(strCommand);
							buttons.Add(array_of_but);
						}
						else {
							buttons[buttons.Count - 1].Add(strCommand);
						}
							
					}
				}
				
			}
			
			TelegramReplyKeyboardMarkup reply = new TelegramReplyKeyboardMarkup();
			reply.keyboard = buttons;
			reply.resize_keyboard = (NumRowsOfButtons == 0);
				
			string result = "";
			
			if (buttons.Count > 0)
			{
				result = JsonConvert.SerializeObject(reply);
			}
			
			return result;
		}
		
        /// <summary>
        /// Определяет, есть ли у пользователя доступ к команде /screen
        /// </summary>
        /// <param name="username">Имя пользователя (username)</param>
        /// <returns></returns>
        public bool AllowToGetScreenshot(string username)
        {
        	if (String.IsNullOrEmpty(username))
        		username = "";
        	
        	if (screenOwners.Count == 0) {
        		return true;
        	}
        	else {        		
        		bool res = false;
        		return screenOwners.TryGetValue(username, out res);
        	}
        }
        
        /// <summary>
        /// Определяет, есть ли у пользователя доступ к команде
        /// </summary>
        /// <param name="username">Имя пользователя (username)</param>
        /// <returns></returns>
        public bool AllowableUserForCommand(string username, Command command)
        {
        	if (String.IsNullOrEmpty(username))
        		username = "";
        	
        	if (command.AllowUsers.Count == 0) {
        		return true;
        	}
        	else {        		
        		bool res = false;
        		return command.AllowUsers.TryGetValue(username, out res);
        	}
        }
        
        /// <summary>
        /// Определяет, есть ли у пользователя доступ к боту
        /// </summary>
        /// <param name="username">Имя пользователя (username)</param>
        /// <returns></returns>
        public bool AllowableUser(string username)
        {
        	if (String.IsNullOrEmpty(username))
        		username = "";
        	
        	if (allowUsers.Count == 0) {
        		return true;
        	}
        	else {        		
        		bool res = false;
        		return allowUsers.TryGetValue(username, out res);
        	}
        }
        
	}
}
