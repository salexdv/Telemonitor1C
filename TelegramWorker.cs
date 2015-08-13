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
using Newtonsoft.Json;

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
        /// Mutex для записи лога из разных потоков                       
        /// </summary>
		private Mutex mutLogger;
		
		/// <summary>
        /// Основной BackgroundWorker для работы с api.telegram.org                       
        /// </summary>
		private BackgroundWorker listener;		
		
		/// <summary>
        /// Очередь необработанных сообщений                       
        /// </summary>
		private Dictionary<int, TelegramCommand> messageOrder;		
					
		/// <summary>
        /// Конструктор        
        /// <PARAM name="settings">Настройки</PARAM>        
        /// </summary>
		public TelegramWorker(Settings settings)
		{
			this.tmSettings = settings;
			this.botToken = settings.BotToken;			
			this.mutLogger = new Mutex();
			this.messageOrder = new Dictionary<int, TelegramCommand>();						
		}
		
		/// <summary>
        /// Запускает работу с api.telegram.org                      
        /// </summary>
		public void StartWork()
		{
			listener = new BackgroundWorker();
			listener.WorkerSupportsCancellation = true;
			listener.DoWork += new DoWorkEventHandler(listener_DoWork);
			listener.RunWorkerAsync();									
		}
				
		/// <summary>
        /// Оправляет multipart/form-data на заданный url       
        /// <PARAM name="url">Адрес url</PARAM>
		/// <PARAM name="pData">PostData</PARAM>        
        /// </summary>
		private void SendMultipartFormdata(string url, PostData pData)
		{
			HttpWebRequest request = CreateRequest(String.Format(url, botToken));
			request.Method = WebRequestMethods.Http.Post;			
			request.KeepAlive = true;
			request.Credentials = System.Net.CredentialCache.DefaultCredentials;
			request.ContentType = "multipart/form-data; boundary=" + pData.Boundary;
						
			MemoryStream postDataStream = pData.GetPostData();
			
			request.ContentLength = postDataStream.Length;
									  		
			using (Stream s = request.GetRequestStream())
			{
			    postDataStream.WriteTo(s);			    
			    postDataStream.Flush();						    			    
			}
			
			using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
				
				using (StreamReader reader = new StreamReader(response.GetResponseStream())) {			            			            
		            string jsonText = reader.ReadToEnd();
		            Logger.Debug(tmSettings, "answer to response: " + jsonText, false, mutLogger);
					TelegramAnswerMessage answer = JsonConvert.DeserializeObject<TelegramAnswerMessage>(jsonText);
		            Logger.Debug(tmSettings, answer.ok.ToString());
		            if (!answer.ok)
		            	Logger.Write(answer.description, true, mutLogger);
		        }
				
			}
        				
			request = null;				
			postDataStream.Close();			
			postDataStream.Dispose();						
		}
		
		/// <summary>
        /// Отправляет фото image/png в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="fileName">Имя отправляемого файла</PARAM>        
        /// </summary>
		private void SendPhoto(int chat_id, string fileName)
		{						
			string url = "https://api.telegram.org/bot{0}/sendPhoto";
			
			Logger.Debug(tmSettings, "url: " + url, false, mutLogger);				
			Logger.Debug(tmSettings, "response file: " + fileName, false, mutLogger);
			            										
			PostData pData = new PostData();
			pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));						
			pData.Params.Add(new PostDataParam("caption", "Скриншот " + DateTime.Now.ToString(), PostDataParamType.Field));
			pData.Params.Add(new PostDataParam("photo", fileName, "image/png"));
			
			SendMultipartFormdata(url, pData);
			
			pData.Dispose();
								
		}
		
		/// <summary>
        /// Отправляет сообщение в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="message">Текст сообщения</PARAM>        
        /// </summary>
		private void SendMessage(int chat_id, string message)
		{			
			string url = "https://api.telegram.org/bot{0}/sendMessage";			
			
			Logger.Debug(tmSettings, "url: " + url, false, mutLogger);
			Logger.Debug(tmSettings, "response: " + message, false, mutLogger);
			            
			PostData pData = new PostData();
			pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));						
			pData.Params.Add(new PostDataParam("text", message, PostDataParamType.Field));
			
			SendMultipartFormdata(url, pData);
			
			pData.Dispose();
			
		}
		
		/// <summary>
        /// Отправляет документ (файл) в заданный чат       
        /// <PARAM name="chat_id">Идентификатор чата</PARAM>
		/// <PARAM name="fName">Имя файла</PARAM>        
        /// </summary>
		private void SendDocument(int chat_id, string fileName)
		{						
			if (File.Exists(fileName)) {
			
				string url = "https://api.telegram.org/bot{0}/sendDocument";
			
				Logger.Debug(tmSettings, "url: " + url, false, mutLogger);				
				Logger.Debug(tmSettings, "response file: " + fileName, false, mutLogger);
				            										
				PostData pData = new PostData();
				pData.Params.Add(new PostDataParam("chat_id", chat_id.ToString(), PostDataParamType.Field));
				pData.Params.Add(new PostDataParam("document", fileName, ""));
				
				SendMultipartFormdata(url, pData);
				
				pData.Dispose();
				
			}
			else
				Logger.Write("Файл для отправки " + fileName + " не обнаружен", true, mutLogger);
			
		}
		
		/// <summary>
        /// Создает HttpWebRequest с заданным Url       
        /// <PARAM name="url">Адрес url</PARAM>				       
        /// </summary>
        /// <returns>HttpWebRequest</returns>
		private HttpWebRequest CreateRequest(string url)
		{
			HttpWebRequest request = HttpWebRequest.Create(url) as HttpWebRequest;
			request.Method = WebRequestMethods.Http.Post;
			request.Timeout	= 10000;
			if (tmSettings.UseProxy) {
				request.Proxy = new WebProxy(tmSettings.ProxyServer, tmSettings.ProxyPort);
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
			string fileName = Path.GetTempPath() + Path.GetFileNameWithoutExtension(tmpFileName) + ".png";
			
			Bitmap bmp = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            using (var gr = Graphics.FromImage(bmp)) {
                gr.CopyFromScreen(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y,
                    0, 0, Screen.PrimaryScreen.Bounds.Size);
            }			
						                                            
            // Сохранение в файл
			bmp.Save(fileName, ImageFormat.Png);
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
			V8Connector connector = new V8Connector(tCommand.Command, tCommand.Parameters);
						
			Logger.Debug(tmSettings, "Запуск команды " + tCommand.Command.ID + " на выполнение", false, mutLogger);			
			// Создание ComConnector и выполнение кода команды
			V8Answer result = connector.Execute(mutLogger);
			Logger.Debug(tmSettings, "Команда " + tCommand.Command.ID + " выполнена", false, mutLogger);
			
			if (connector.Success) {
				if (!String.IsNullOrEmpty(result.Text))
					SendMessage(tCommand.Message.chat.id, result.Text);
				
				if (!String.IsNullOrEmpty(result.FileName))
					SendDocument(tCommand.Message.chat.id, result.FileName);
				
				if (String.IsNullOrEmpty(result.Text) && String.IsNullOrEmpty(result.FileName)) 
					SendMessage(tCommand.Message.chat.id, "Команда выполнена");
			}
			else
				SendMessage(tCommand.Message.chat.id, "Ошибка при выполнении команды");
			
			connector.Dispose();
			
			messageOrder.Remove(tCommand.Message.message_id);
			
			((BackgroundWorker)sender).CancelAsync();			
			
		}
		
		/// <summary>
		/// Возвращает параметры из команды (все что отделено пробелом).
		/// Текст команды при этом приводится к чистому виду.
		/// </summary>
		/// <param name="cmd">Текст поступившей команды</param>
		/// <returns>Параметры команды, разделенные запятыми</returns>
		private string extractParamForCommand(string cmd, out string newCmd)
		{
			string param = "";
					
			string[] subStr = cmd.Split(' ');
			newCmd = subStr[0];
			
			for (int i = 1; i < subStr.Length; i++) {
				param += subStr[i];
				param += ',';
			}
			
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
			string text = message.ToString();
			string param = extractParamForCommand(text, out text);
			text = text.ToLower();
			
			if (text == "/start") {
				// Запрошен список команд
				SendMessage(message.chat.id, tmSettings.GetCommands());
			}
			else if (text == "/screen") {
				// Запрошен скриншот всей области экрана
				string fileName = getScreenShot();
				SendPhoto(message.chat.id, fileName);
				File.Delete(fileName);
			}
			else {
				object cmd = tmSettings.GetCommandByName(text);
				if (cmd != null) {
					// Запрошена команда из списка
					if (!messageOrder.ContainsKey(message.message_id)) {
						TelegramCommand tCommand = new TelegramCommand();
						tCommand.Message = message;
						tCommand.Command = (Command)cmd;
						tCommand.Parameters = param;
						messageOrder.Add(message.message_id, tCommand);
						BackgroundWorker executer = new BackgroundWorker();
						executer.WorkerSupportsCancellation = true;
						executer.DoWork += new DoWorkEventHandler(ExecuteCommand);
						executer.RunWorkerAsync(tCommand);
					}
				}
				else {
					// unknow command
					SendMessage(message.chat.id, "Неизвестная команда");
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
			int newOffset = tmOffset;
			
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
				return tmOffset;
			}
			
		}

		/// <summary>
        /// Обработчик для BackgroundWorker, который опрашивает api.telegram.org                    
        /// </summary>
		void listener_DoWork(object sender, DoWorkEventArgs e)
		{
			// Получение updates с заданной периодичностью
			int interval = this.tmSettings.Interval;			
			tmOffset = 0;			
			                   		
			HttpWebRequest request = null;			
			
			while (!listener.CancellationPending) {
												
				string url = "https://api.telegram.org/bot{0}/getUpdates?offset=" + this.tmOffset.ToString();
				Logger.Debug(tmSettings, "url: " + url, false, mutLogger);				
				             
				request = CreateRequest(String.Format(url, botToken));
				
				using (HttpWebResponse response = (HttpWebResponse)request.GetResponse()) {
					
					using (StreamReader reader = new StreamReader(response.GetResponseStream())) {			            			            
			            string jsonText = reader.ReadToEnd();
						Logger.Debug(tmSettings, "request:" + jsonText, false, mutLogger);
			            // Получение updates из JSON
						TelegramAnswer answer = JsonConvert.DeserializeObject<TelegramAnswer>(jsonText);
			            Logger.Debug(tmSettings, answer.ok.ToString(), false, mutLogger);
			            // Обработка, полученных updates
			            tmOffset = listener_CheckTelegramAnswer(answer);
			        }
					
				}
				
				request = null;
								
				System.Threading.Thread.Sleep(interval * 1000);				
			}
		}
		
		/// <summary>
        /// Останавливае работу с api.telegram.org                      
        /// </summary>
		public void StopWork()
		{
			listener.CancelAsync();
			listener.Dispose();			
		}
		
		
	}
}
