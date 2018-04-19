/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 07.08.2015
 * Time: 10:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.IO;
using System.Net.Mime;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Windows.Forms;
using System.Web;
using System.Data.SQLite;
using Newtonsoft.Json;
using System.Timers;
using System.Diagnostics;
using xNet;

namespace Telemonitor
{	
	/// <summary>
	/// Класс для работы с API Telegram (получение сообщений и отправка ответов на них)
	/// </summary>
	public class TelegramWorker
	{
		/// <summary>
        /// Настройки программы                       
        /// </summary>
		private Settings tmSettings;
		
		/// <summary>
        /// Token бота                       
        /// </summary>
		private string botToken;
		
		/// <summary>
        /// Смещение для опроса getUpdates                       
        /// </summary>
		private int tmOffset;
		
		/// <summary>
        /// Максимальное смещение для опроса getUpdates                       
        /// </summary>
		private int maxOffset;
		
		/// <summary>
        /// Mutex для записи лога из разных потоков                       
        /// </summary>
		private Mutex mutLogger;
		
		/// <summary>
        /// Mutex для запросов к API                       
        /// </summary>
		private Mutex mutAPI;
		
		/// <summary>
        /// Последнее время запуска обработчика запросов                       
        /// </summary>
		private DateTime LastStartTime;		
		
		/// <summary>
		/// Основной обработчик для опроса API на предмет
		/// новый сообщений
		/// </summary>
		private BackgroundWorker BackgroundListener;
		
		/// <summary>
		/// Обработчик для проверки работоспособности
		/// </summary>
		private BackgroundWorker BackgroundChecker;
		
		/// <summary>
        /// Очередь необработанных сообщений                       
        /// </summary>
		private Dictionary<int, TelegramCommand> messageOrder;		
		
		/// <summary>
		/// Соединение с базой sqlite
		/// </summary>
		private SQLiteConnection sqlConnection;

		/// <summary>
		/// Возвращает соединение с базой данных sqlite
		/// </summary>
		private SQLiteConnection GetSQLConnection()
		{
			SQLiteConnection conn = new SQLiteConnection("Data Source=Telemonitor.db; Version=3;");
			try
			{
			    conn.Open();
			}
			catch (SQLiteException ex)
			{
				Logger.Debug(tmSettings, "sql connection error: " + ex.Message.ToString(), true, mutLogger);
				return null;
			}
			
			SQLiteCommand cmd = conn.CreateCommand();
			string sql_command = "CREATE TABLE IF NOT EXISTS messages("
			  + "direct TEXT, "
			  + "message_id INTEGER, "
			  + "parent_id INTEGER, "
			  + "user_id INTEGER, "
			  + "chat_id INTEGER, "
			  + "username TEXT, "
			  + "first_name TEXT, "
			  + "last_name TEXT, "
			  + "text TEXT, " 
			  + "date INTEGER); "
    		  + "CREATE INDEX IF NOT EXISTS message_id on messages (message_id); "
			  + "CREATE INDEX IF NOT EXISTS parent_idx on messages (parent_id); "
			  + "CREATE INDEX IF NOT EXISTS from_idx on messages (user_id, chat_id); ";
			cmd.CommandText = sql_command;
			
			try
			{
			    cmd.ExecuteNonQuery();
			}
			catch (SQLiteException ex)
			{
			    Logger.Debug(tmSettings, "sql db error: " + ex.Message.ToString(), true, mutLogger);
				return null;			    
			}
						
			return conn;
		}
		
		/// <summary>
		/// Сохраняет входящие и исходящие сообщения в базу данных
		/// </summary>
		/// <param name="message">Сообщение</param>
		/// <param name="direct">Направление</param>
		private void SaveMessageToDB(TelegramMessage message, string direct)
		{
			if (sqlConnection != null) {
				SQLiteCommand cmd = sqlConnection.CreateCommand();
				cmd.CommandText = "INSERT INTO messages (direct, message_id, parent_id, user_id, chat_id, username, first_name, last_name, text, date) "
				  + "VALUES (@direct, @message_id, @parent_id, @user_id, @chat_id, @username, @first_name, @last_name, @text, @date);";				
				cmd.Parameters.AddWithValue("@direct", direct);
				cmd.Parameters.AddWithValue("@message_id", message.message_id);
				cmd.Parameters.AddWithValue("@parent_id", (message.reply_to_message != null) ? message.reply_to_message.message_id : 0);
				cmd.Parameters.AddWithValue("@user_id", message.from.id);
				cmd.Parameters.AddWithValue("@chat_id", message.chat.id);
				cmd.Parameters.AddWithValue("@username", message.from.username);
				cmd.Parameters.AddWithValue("@first_name", message.from.first_name);
				cmd.Parameters.AddWithValue("@last_name", message.from.last_name);
				cmd.Parameters.AddWithValue("@text", (message.text != null) ? message.text : "");
				cmd.Parameters.AddWithValue("@date", message.date);				
				try {
					Logger.Debug(tmSettings, "sql start insert", false, mutLogger);	
					cmd.ExecuteNonQuery();
					Logger.Debug(tmSettings, "sql insert ok", false, mutLogger);	
				}
				catch (SQLiteException ex) {
					Logger.Debug(tmSettings, "sql ins error: " + ex.Message.ToString(), true, mutLogger);	
				}
			}
			
		}
		
		/// <summary>
		/// Возвращает сообщение из базы данных
		/// </summary>
		/// <param name="message_id">Идентификатор сообщения</param>
		/// <param name="user_id">Идентификатор пользователя</param>
		/// <param name="chat_id">Идентификатор чата</param>
		/// <returns></returns>
		private MessageTDB GetMessageFromDB(int message_id, int user_id, int chat_id)
		{
			if (sqlConnection != null) {
				SQLiteCommand cmd = sqlConnection.CreateCommand();
				cmd.CommandText  = "SELECT direct, message_id, parent_id, text "
				  + "FROM messages WHERE message_id = @message_id AND chat_id = @chat_id";
				cmd.Parameters.AddWithValue("@message_id", message_id);							
				cmd.Parameters.AddWithValue("@chat_id", chat_id);
				try
				{
				    SQLiteDataReader r = cmd.ExecuteReader();
				    string line = String.Empty;
				    MessageTDB message = null;
				    while (r.Read()) {
				    	message = new MessageTDB();
				    	message.Direct = r["direct"].ToString();
				    	message.MessageID = Convert.ToInt32(r["message_id"]);
				    	message.ParentID = Convert.ToInt32(r["parent_id"]);
				    	message.Text = r["text"].ToString();
				    }			    
				    r.Close();
				    return message;
				}
				catch (SQLiteException ex)
				{
				    Logger.Debug(tmSettings, "sql rd error: " + ex.Message.ToString(), true, mutLogger);	
				}
			}
			
			return null;
		}
		
		/// <summary>
		/// Возвращает текст команды
		/// </summary>
		/// <param name="message">Сообщение, для которого надо получить вышестоящую команду</param>
		private string GetTextOfRootCommand(TelegramMessage message)
		{
			string commandText = "";			
			List<string> commandParams = new List<string>();
			commandParams.Add(message.text);
			
			MessageTDB msg = null;
			int parent_id = message.reply_to_message.message_id;
			
			while (parent_id > 0) {
				msg = GetMessageFromDB(parent_id, message.from.id, message.chat.id);
				if (msg == null)
					parent_id = 0;
				else if (msg.ParentID > 0) {
					parent_id = msg.ParentID;
					if (msg.Direct == "in")
						commandParams.Add(msg.Text);						
				}
				else {
					parent_id = 0;
					commandText = msg.Text;
				}
			}	

			string param = "";
			int i = commandParams.Count - 1;
			while (0 <= i) {
				param += commandParams[i];
				if (0 < i)
					param += ",";
				i--;
			}
			
			
			if (!String.IsNullOrEmpty(param))
				commandText += " " + param;
			
			return commandText;
		}
		
		/// <summary>
        /// Конструктор        
        /// <PARAM name="settings">Настройки</PARAM>        
        /// </summary>
		public TelegramWorker(Settings settings)
		{			
			this.tmOffset = 0;
			this.maxOffset = 0;
			this.tmSettings = settings;			
			this.botToken = settings.BotToken;			
			this.mutLogger = new Mutex();
			this.mutAPI = new Mutex();
			this.messageOrder = new Dictionary<int, TelegramCommand>();						
			this.sqlConnection = GetSQLConnection();
		}
		
		void BackgroundListener_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
    		if (e.Cancelled == true) {        		
        		Logger.Debug(tmSettings, "worker canceled", false, mutLogger);	
    		}
    		else if (e.Error != null) {        		
        		Logger.Debug(tmSettings, "worker error: " + e.Error.Message, true, mutLogger);
    		}
			else if (e.Result == null) {
				Logger.Debug(tmSettings, "worker stopped", true, mutLogger);
			}
    		else {        		
        		Logger.Debug(tmSettings, "worker stopped: " + e.Result.ToString(), true, mutLogger);
    		}
		}
		
		/// <summary>
        /// Основной обработчик BackgroundCheker                      
        /// </summary>
		void BackgroundChecker_DoWork(object sender, DoWorkEventArgs e)
		{
													
			while (!BackgroundChecker.CancellationPending) {
											
				if (LastStartTime != new DateTime(1, 1, 1)) {
				
					DateTime CurrentTime = DateTime.Now;
					TimeSpan ts = CurrentTime - LastStartTime;
					int interval = tmSettings.Interval * 1000;				
					
					Logger.Debug(tmSettings, "checker: cur.time - " + CurrentTime.ToString() + ", ls.time - " + LastStartTime.ToString(), false, mutLogger);
					
					if (interval * 100 < ts.TotalMilliseconds) {
						Logger.Debug(tmSettings, "restart BackgroundListener " + (ts.Milliseconds / 1000).ToString(), false, mutLogger);
						LastStartTime = new DateTime(1, 1, 1);
						BackgroundListener.CancelAsync();
						BackgroundListener.Dispose();
						this.tmOffset = Math.Max(this.tmOffset, this.maxOffset);
						StartWork(false);
						Logger.Debug(tmSettings, "BackgroundListener was restarted", false, mutLogger);
						System.Threading.Thread.Sleep(interval * 100);
					}
					
					System.Threading.Thread.Sleep(interval * 50);					
					
				}
								
			}
				
		}
		
		/// <summary>
        /// Запускает работу с api.telegram.org                      
        /// </summary>
		public void StartWork(bool restartChecker = true)
		{
			BackgroundListener = new BackgroundWorker();
			BackgroundListener.WorkerSupportsCancellation = true;
			BackgroundListener.DoWork += new DoWorkEventHandler(BackgroundListener_DoWork);
			BackgroundListener.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundListener_RunWorkerCompleted);			
			BackgroundListener.RunWorkerAsync();

			if (restartChecker) {
				BackgroundChecker = new BackgroundWorker();
				BackgroundChecker.WorkerSupportsCancellation = true;
				BackgroundChecker.DoWork += new DoWorkEventHandler(BackgroundChecker_DoWork);
				BackgroundChecker.RunWorkerAsync();
			}
		}
		
		/// <summary>
        /// Основной обработчик BackgroundListener                      
        /// </summary>
		void BackgroundListener_DoWork(object sender, DoWorkEventArgs e)
		{
										
			DateTime StartTime = new DateTime(2000, 1, 1);
			DateTime CurrentTime = DateTime.Now;
			TimeSpan ts = CurrentTime - StartTime;
			int interval = tmSettings.Interval * 1000;				
						
			while ( !((BackgroundWorker)sender).CancellationPending ) {
								
				CurrentTime = DateTime.Now;
				ts = CurrentTime - StartTime;
										
				if (ts.TotalMilliseconds < interval) {
					Logger.Debug(tmSettings, "wt " + (interval - ts.Milliseconds).ToString(), false, mutLogger);
					System.Threading.Thread.Sleep(interval - ts.Milliseconds);
				}				
				
				StartTime = DateTime.Now;
				LastStartTime = StartTime;
				
				this.tmOffset = Math.Max(this.tmOffset, this.maxOffset);
				
				// Получение updates с заданной периодичностью													
				string url = "https://api.telegram.org/bot{0}/getUpdates?offset=" + this.tmOffset.ToString();
				Logger.Debug(tmSettings, "url: " + url, false, mutLogger);
	
				Logger.Debug(tmSettings, "mt wait", false, mutLogger);
				mutAPI.WaitOne();
												
				HttpRequest request = CreateRequest();
				
				Logger.Debug(tmSettings, "request created", false, mutLogger);
				
				TelegramAnswer answer = null;
				
				try {                    
                    
					HttpResponse response = request.Get(String.Format(url, botToken));
                    string jsonText = response.ToString();
                    
                    response.None();
                    response = null;
                    
                    // Убираем пикрограммы
					jsonText = jsonText.Replace(Const.PIC_BUTTON_START_REP, "");
					jsonText = jsonText.Replace(Const.PIC_BUTTON_OTHER_REP, "");					
		            Logger.Debug(tmSettings, "request:" + jsonText, false, mutLogger);
		            // Получение updates из JSON
					answer = JsonConvert.DeserializeObject<TelegramAnswer>(jsonText);
		            Logger.Debug(tmSettings, answer.ok.ToString(), false, mutLogger);
                                                           
				}
				catch (Exception respExc) {
					Logger.Debug(tmSettings, "response err: " + respExc.Message, true, mutLogger);
				}
				
				request.Close();
				request = null;
				
				// Обработка, полученных updates
				if (answer != null)
					this.tmOffset = Math.Max(this.tmOffset, listener_CheckTelegramAnswer(answer));

				this.maxOffset = Math.Max(this.tmOffset, this.maxOffset);
				
				Logger.Debug(tmSettings, "mt release", false, mutLogger);
				mutAPI.ReleaseMutex();
								
			}
				
		}
			
		/// <summary>
        /// Оправляет multipart/form-data на заданный url       
        /// <PARAM name="url">Адрес url</PARAM>
		/// <PARAM name="pData">PostData</PARAM>        
        /// </summary>
		private void SendMultipartFormdata(string url, PostData pData)
		{
			Logger.Debug(tmSettings, "smfd mt wait", false, mutLogger);
			mutAPI.WaitOne();
			Logger.Debug(tmSettings, "smfd mt success", false, mutLogger);
			
			HttpRequest request = CreateRequest();
			request.KeepAlive = true;
			
			foreach (PostDataParam param in pData.Params) {
				if (param.Type == PostDataParamType.Field)
					request.AddField(param.Name, param.Value);
				else
					request.AddFile(param.Name, param.Value);
			}
			
			Logger.Debug(tmSettings, "smfd post request", false, mutLogger);
			
			HttpResponse response = request.Post(String.Format(url, botToken));
			string jsonText = response.ToString();
			
			response.None();
			response = null;
			
			Logger.Debug(tmSettings, "smfd answer to response: " + jsonText, false, mutLogger);
			TelegramAnswerMessage answer = JsonConvert.DeserializeObject<TelegramAnswerMessage>(jsonText);					
            Logger.Debug(tmSettings, "smfd " + answer.ok.ToString(), false, mutLogger);		            
            
            if (!answer.ok)
            	Logger.Write(answer.description, true, mutLogger);
            else
            	SaveMessageToDB(answer.message, "out");		
                        
            request.Close();            
            request = null;	            

			mutAPI.ReleaseMutex();
			Logger.Debug(tmSettings, "smfd mt release", false, mutLogger);
			GC.WaitForPendingFinalizers();
			GC.Collect();
						
		}
				
		
		/// <summary>
        /// Отправляет фото image/png в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="fileName">Имя отправляемого файла</PARAM>        
        /// </summary>
		private void SendPhoto(int chat_id, string fileName, int reply_to_message_id = 0)
		{						
			string url = "https://api.telegram.org/bot{0}/sendPhoto";
			
			Logger.Debug(tmSettings, "url: " + url, false, mutLogger);				
			Logger.Debug(tmSettings, "response file: " + fileName, false, mutLogger);
			            										
			PostData pData = new PostData();
			pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));						
			pData.Params.Add(new PostDataParam("caption", "Скриншот " + DateTime.Now.ToString(), PostDataParamType.Field));
			pData.Params.Add(new PostDataParam("photo", fileName, PostDataParamType.File));
						
			SendMultipartFormdata(url, pData);
			
			pData.Dispose();
			
			GC.WaitForPendingFinalizers();
			GC.Collect();
								
		}
		
		/// <summary>
		/// Разбивает сообщение на блоки по 4096 символов
		/// и возвращает массив этих блоков (4096 - максимальная длина сообщения)
		/// </summary>
		/// <param name="message"></param>
		/// <returns></returns>
		private List<string> GetArrayOfMessages(string message)
		{
			int msgLength = 0;
			string tmpMsg = (string)message.Clone();
			List<string> messages = new List<string>();
			
			while (0 < tmpMsg.Length)
			{
				msgLength = Math.Min(tmpMsg.Length, 4096);
				messages.Add(tmpMsg.Substring(0, msgLength));
				tmpMsg = tmpMsg.Substring(msgLength);
			}
			
			return messages;
		}
		
		/// <summary>
        /// Отправляет сообщение в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="message">Текст сообщения</PARAM>        
        /// </summary>
		private void SendMessage(int chat_id, string message, string keyboard = "", int reply_to_message_id = 0)
		{			
			string url = "https://api.telegram.org/bot{0}/sendMessage";			
			
			Logger.Debug(tmSettings, "url: " + url, false, mutLogger);
			Logger.Debug(tmSettings, "response: " + message, false, mutLogger);
			   
			List<string> messages = GetArrayOfMessages(message);
			
			foreach (string curMessage in messages) {			
				PostData pData = new PostData();
				pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));						
				pData.Params.Add(new PostDataParam("text", curMessage, PostDataParamType.Field));
				pData.Params.Add(new PostDataParam("reply_to_message_id", reply_to_message_id.ToString(), PostDataParamType.Field));
				if (reply_to_message_id > 0) {
					TelegramForceReply forceReply = new TelegramForceReply();
					forceReply.force_reply = true;
					pData.Params.Add(new PostDataParam("reply_markup", JsonConvert.SerializeObject(forceReply), PostDataParamType.Field));
				}
				else if (!String.IsNullOrEmpty(keyboard))
					pData.Params.Add(new PostDataParam("reply_markup", keyboard, PostDataParamType.Field));
				else
				{
					if (tmSettings.HideButtonsAfterMessage)
						pData.Params.Add(new PostDataParam("reply_markup", JsonConvert.SerializeObject(new TelegramReplyKeyboardHide()), PostDataParamType.Field));
				}
				
				SendMultipartFormdata(url, pData);
				
				pData.Dispose();
			}
			
			GC.WaitForPendingFinalizers();
			GC.Collect();
			
		}
		
		/// <summary>
        /// Отправляет документ (файл) в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="fName">Имя файла</PARAM>        
        /// </summary>
		private void SendDocument(int chat_id, string fileName, int reply_to_message_id = 0)
		{						
			if (File.Exists(fileName)) {
			
				string url = "https://api.telegram.org/bot{0}/sendDocument";
			
				Logger.Debug(tmSettings, "url: " + url, false, mutLogger);				
				Logger.Debug(tmSettings, "response file: " + fileName, false, mutLogger);
				            										
				PostData pData = new PostData();
				pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));
				pData.Params.Add(new PostDataParam("document", fileName, PostDataParamType.File));
				
				SendMultipartFormdata(url, pData);
				
				pData.Dispose();
				
				GC.WaitForPendingFinalizers();
				GC.Collect();
				
			}
			else
				Logger.Write("Файл для отправки " + fileName + " не обнаружен", true, mutLogger);			
			
		}
		
		/// <summary>
        /// Создает HttpRequest с заданным Url       
        /// <PARAM name="url">Адрес url</PARAM>				       
        /// </summary>
        /// <returns>HttpRequest</returns>
		private HttpRequest CreateRequest()
		{
			
			var request = new HttpRequest();
			request.KeepAlive = true;
			request.ConnectTimeout = 30000;
			if (tmSettings.UseProxy) {
				if (tmSettings.ProxyType == 0)
					request.Proxy = HttpProxyClient.Parse(tmSettings.ProxyServer.ToString() + ':' + tmSettings.ProxyPort.ToString());
				else
					request.Proxy = Socks5ProxyClient.Parse(tmSettings.ProxyServer.ToString() + ':' + tmSettings.ProxyPort.ToString());
				request.Proxy.Username = tmSettings.ProxyUser;
				request.Proxy.Password = tmSettings.ProxyPass;
			}
						
			return request;
		}
		
		/// <summary>
        /// Делает скриншот всей области экрана и возващает имя файла PNG                     
        /// </summary>
        /// <returns>string</returns>
		private string getScreenShot() {
			
			// Получение скриншота
			string tmpFileName = Path.GetTempFileName();
			string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(tmpFileName) + ".jpg";
			
			Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var gr = Graphics.FromImage(bmp)) {
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size);
            }			
						                                            
            // Сохранение в файл
			bmp.Save(fileName, ImageFormat.Jpeg);
			bmp.Dispose();	
			
			File.Delete(tmpFileName);
			
			return fileName;
		}
		
        /// <summary>
        /// Обработчик для BackgroundWorker, который обрабатывает команды                    
        /// </summary>        
		private void ExecuteCommand(object sender, DoWorkEventArgs e)
		{
			TelegramCommand tCommand = (TelegramCommand)e.Argument;
			
			if (tCommand.Command.Type == commandTypes.command1C) {
			
				// Запуск команды в базе 1С
				V8Connector connector = new V8Connector(tCommand.Command, tCommand.Parameters, tmSettings);
				connector.TelegramUserName = tCommand.Message.from.username;
				connector.TelegramFirstName = tCommand.Message.from.first_name;
				connector.TelegramLastName = tCommand.Message.from.last_name;
							
				Logger.Debug(tmSettings, "Запуск команды " + tCommand.Command.ID + " на выполнение", false, mutLogger);			
				// Создание ComConnector и выполнение кода команды
				V8Answer result = connector.Execute(mutLogger);
				Logger.Debug(tmSettings, "Команда " + tCommand.Command.ID + " выполнена", false, mutLogger);
				
				if (connector.Success) {
					
					int reply_to_message_id = (result.Dialog) ? tCommand.Message.message_id : 0;
					
					if (!String.IsNullOrEmpty(result.Text))
						SendMessage(tCommand.Message.chat.id, result.Text, "", reply_to_message_id);
					
					if (!String.IsNullOrEmpty(result.FileName))
						SendDocument(tCommand.Message.chat.id, result.FileName);
					
					if (String.IsNullOrEmpty(result.Text) && String.IsNullOrEmpty(result.FileName)) 
						SendMessage(tCommand.Message.chat.id, "Команда выполнена");
				}
				else
					SendMessage(tCommand.Message.chat.id, "Ошибка при выполнении команды");
				
				result = null;
				connector.Dispose();								
			}
			else {				
				
				// Запуск команды OneScript
				Logger.Debug(tmSettings, "Запуск команды oscript " + tCommand.Command.ID + " на выполнение", false, mutLogger);			
				
				StringBuilder output = new StringBuilder();
				System.Diagnostics.Process oscript = new Process();
				
				if (String.IsNullOrEmpty(tmSettings.OScriptPath))
					oscript.StartInfo.FileName = "oscript";
				else {
					if (File.Exists(tmSettings.OScriptPath))
						oscript.StartInfo.FileName = tmSettings.OScriptPath;
					else {						
						SendMessage(tCommand.Message.chat.id, "oscript не найден по указанному пути");
						Logger.Debug(tmSettings, "oscript не найден по указанному пути: " + tmSettings.OScriptPath, true, mutLogger);
					}
				}
				
				if (!String.IsNullOrEmpty(oscript.StartInfo.FileName)) {
				
					oscript.StartInfo.Arguments = "\"" + tCommand.Command.ConnectionString + "\"";
					
					if (!String.IsNullOrEmpty(tCommand.Parameters)) {
						string oscParam = tCommand.Parameters.Trim().Replace(' ', '_').Replace(',', ' ');
						oscript.StartInfo.Arguments += " " + oscParam;
					}
									
					oscript.StartInfo.UseShellExecute = false;
					oscript.StartInfo.RedirectStandardOutput = true;
					oscript.StartInfo.StandardOutputEncoding = Encoding.GetEncoding(866);
					oscript.StartInfo.CreateNoWindow = true;				
					
					try {
						oscript.Start();	
					}
					catch (Exception ex) {
						SendMessage(tCommand.Message.chat.id, "Ошибка при выполнении команды");
						Logger.Debug(tmSettings, "Ошибка при выполнении команды oscript: " + ex.Message, true, mutLogger);
					}
					
					string curString = "";
					bool dialog = false;
					string resFile = "";
					
					while (!oscript.StandardOutput.EndOfStream) 
	 				{
						curString = oscript.StandardOutput.ReadLine();
						
						if (curString.StartsWith("Результат_Файл") && String.IsNullOrEmpty(resFile)) {
							resFile = curString.Substring(15).Trim();
							if (resFile.StartsWith("=")) {
								resFile = resFile.Substring(2).Trim();
								continue;
							}
							else
								resFile = "";
						}
						
						if (curString == "ДиалогСПараметрами")
							dialog = true;
						else
							output.AppendLine(curString);
	 				}
					
					int reply_to_message_id = (dialog) ? tCommand.Message.message_id : 0;
	
					if (0 < output.Length || !String.IsNullOrEmpty(resFile)) {
						if (0 < output.Length)
							SendMessage(tCommand.Message.chat.id, output.ToString(), "", reply_to_message_id);
						if (!String.IsNullOrEmpty(resFile))
							SendDocument(tCommand.Message.chat.id, resFile);
					}
					else
						SendMessage(tCommand.Message.chat.id, "Команда выполнена");
					
					Logger.Debug(tmSettings, "Команда oscript" + tCommand.Command.ID + " выполнена", false, mutLogger);
				}
			}
									
			messageOrder.Remove(tCommand.Message.message_id);
			
			tCommand = null;
						
			((BackgroundWorker)sender).CancelAsync();			
			((BackgroundWorker)sender).Dispose();
			
			GC.WaitForPendingFinalizers();
			GC.Collect();
			
		}
		
		/// <summary>
		/// Возвращает параметры из команды (все что отделено пробелом).
		/// Текст команды при этом приводится к чистому виду.
		/// Необработанная строка имеет вид /Команда Параметр1,Параметр2,и т.д.
		/// </summary>
		/// <param name="cmd">Текст поступившей команды</param>
		/// <returns>Параметры команды, разделенные запятыми</returns>
		private string extractParamForCommand(string cmd, out string newCmd)
		{
			string param = "";				
			int splitter = cmd.IndexOf(' ');
			
			if (0 < splitter) {
				newCmd = cmd.Substring(0, splitter);
				string[] subStr = cmd.Substring(splitter).Split(',');
				for (int i = 0; i < subStr.Length; i++) {
					if (!String.IsNullOrEmpty(subStr[i])) {
						param += subStr[i];
						param += ',';
					}
				}
			}			
			else
				newCmd = cmd;	
									
			if (!String.IsNullOrEmpty(param))
				param = param.Remove(param.Length - 1);
			
			return param;
			
		}
		
		/// <summary>
        /// Разбирает полученное сообщение и определяет, что с ним делать       
        /// <PARAM name="message">Сообщение TelegramMessage</PARAM>				       
        /// </summary>        
		private void listener_CheckMessage(TelegramMessage message)			
		{
			
			SaveMessageToDB(message, "in");
						
			if (message.text == null) {
				SendMessage(message.chat.id, "Неизвестная команда");
			}
			else {
				
				string username = message.from.username;
				string text = "";
				string param = "";		
				
				if (message.reply_to_message != null ) {
					// Это ответ на начальное сообщение, значит команда содержится в предыдущем, а в текущем идут параметры
					text = GetTextOfRootCommand(message);								
				}
				else {
					text = message.ToString();
				}
								
				text = text.ToLower();
				param = extractParamForCommand(text, out text);		
				
				// Фильтрация пользователей
				if (tmSettings.AllowableUser(username)) {
				
					if (text == "/start" || text == "/help" || text == "/settings") {
						// Запрошен список команд
						SendMessage(message.chat.id, tmSettings.GetCommands(username), tmSettings.GetKeyboardCommands(username));
					}
					else if (text == "/screen") {
						// Запрошен скриншот всей области экрана
						if (tmSettings.AllowToGetScreenshot(username)) {
							string fileName = getScreenShot();
							SendPhoto(message.chat.id, fileName);
							File.Delete(fileName);
						}
						else {
							SendMessage(message.chat.id, "У вас нет доступа к данной команде");	
						}
					}
					else {				
						object cmd = tmSettings.GetCommandByName(text);
						if (cmd != null) {
							// Запрошена команда из списка
							if (!messageOrder.ContainsKey(message.message_id)) {
								Command cur_command = (Command)cmd;							
								if (tmSettings.AllowableUserForCommand(username, cur_command)) {
									TelegramCommand tCommand = new TelegramCommand();
									tCommand.Message = message;
									tCommand.Command = cur_command;
									tCommand.Parameters = param;
									messageOrder.Add(message.message_id, tCommand);
									BackgroundWorker executer = new BackgroundWorker();
									executer.WorkerSupportsCancellation = true;
									executer.DoWork += new DoWorkEventHandler(ExecuteCommand);
									executer.RunWorkerAsync(tCommand);
								}
								else {
									SendMessage(message.chat.id, "У вас нет доступа к данной команде");
								}
							}
						}
						else {
							// unknow command
							SendMessage(message.chat.id, "Неизвестная команда");
						}
					}
				}
				else {
					SendMessage(message.chat.id, "У вас нет доступа к данному боту");
				}
			}						
		}
		
		/// <summary>
        /// Разбирает полученные updates c сообщениями       
        /// <PARAM name="updates">List<TelegramUpdate></PARAM>				       
        /// </summary>
        /// <returns>int - номер последнего обработанного TelegramUpdate</returns>
		private int listener_CheckUpdates(List<TelegramUpdate> updates)			
		{
			int newOffset = this.tmOffset;
			
			foreach (TelegramUpdate update in updates) {
				// Обработка каждого сообщения
				listener_CheckMessage(update.message);
				newOffset = update.update_id + 1;
			}
			
			return newOffset;
		}
		
		/// <summary>
        /// Запускает обработку полученных updates       
        /// <PARAM name="answer">List<TelegramAnswer></PARAM>				       
        /// </summary>
        /// <returns>int - номер последнего обработанного TelegramUpdate</returns>
		private int listener_CheckTelegramAnswer(TelegramAnswer answer)
		{

			if (answer.ok) {
				// Ошибок нет, можно обработать updates
				return listener_CheckUpdates(answer.updates);
			}
			else
			{
				// API вернул ошибку
				Logger.Write(answer.description, true, mutLogger);
				return this.tmOffset;
			}
			
		}
		
		/// <summary>
        /// Останавливае работу с api.telegram.org                      
        /// </summary>
		public void StopWork()
		{	
			
			BackgroundChecker.CancelAsync();
			BackgroundChecker.Dispose();
			
			BackgroundListener.CancelAsync();
			BackgroundListener.Dispose();			
		}
		
		
	}
}
