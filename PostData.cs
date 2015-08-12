/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 11.08.2015
 * Time: 12:38
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Telemonitor
{
	
	/// <summary>
	/// Перечисление - тип параметра PostData
	/// </summary>
	public enum PostDataParamType
	{
	    Field,
	    File
	}
	
	/// <summary>
	/// Класс для хранения параметра PostData
	/// </summary>
	public class PostDataParam
	{	
		/// <summary>
		/// Имя параметра
		/// </summary>
		public string Name {get; set;}
		
		/// <summary>
		/// Полный путь к файлу
		/// </summary>
	    public string FileName {get; set;}
	    
	    /// <summary>
	    /// Значение параметра
	    /// </summary>
	    public string Value {get; set;}
	    
	    /// <summary>
	    /// Content-Type для параметра типа File
	    /// </summary>
	    public string FileContentType {get; set;}
	    	    
	    /// <summary>
	    /// Тип параметра
	    /// </summary>
	    public PostDataParamType Type {get; set;}
		
		/// <summary>
		/// Конструктор произвольного параметра
		/// <PARAM name="name">Имя параметра</PARAM>
		/// <PARAM name="value">Значение параметра</PARAM>
		/// <PARAM name="type">Тип параметра</PARAM>
		/// </summary>		
	    public PostDataParam(string name, string value, PostDataParamType type)
	    {
	    	Name = name;
	    	Value = value;
	    	Type = type;
	    	FileName = "";
	    	FileContentType = "";
	    }
	    
	    /// <summary>
		/// Конструктор параметра типа File
		/// <PARAM name="name">Имя параметра</PARAM>
		/// <PARAM name="fileName">Полный путь к файлу</PARAM>
		/// <PARAM name="contentType">Сontent-Type</PARAM>
		/// </summary>		
	    public PostDataParam(string name, string fileName, string contentType)
	    {
	    	Name = name;
			FileName = fileName;
	    	FileContentType	= contentType;
	    	Type = PostDataParamType.File;
			Value = "";	    	
	    }
			    
	}
	
	/// <summary>
	/// Класс PostData для отправки multipart/form-data;
	/// </summary>
	public class PostData
	{
		/// <summary>
		/// Коллекция параметров
		/// </summary>
		private List<PostDataParam> p_Params;
		
		/// <summary>
		/// Разделитель Boundary для тела запроса
		/// </summary>
		private string p_Boundary;
		
		/// <summary>
		/// Поток, через который осуществляется запись параметров в тело запроса
		/// </summary>
		private StreamWriter p_DataWriter;		
		
		/// <summary>
		/// Разделитель Boundary для тела запроса 
		/// </summary>
		public string Boundary
	    {
	    	get {
	    		return p_Boundary; 
	    	}
	    	set { 
	    		p_Boundary = value; 
	    	}
	    }
		
		/// <summary>
		/// Коллекция параметров
		/// </summary>
	    public List<PostDataParam> Params
	    {
	    	get {
	    		return p_Params; 
	    	}
	    	set { 
	    		p_Params = value; 
	    	}
	    }
	
	    /// <summary>
	    /// Конструктор класса
	    /// </summary>
	    public PostData()
	    {
	    	p_Params = new List<PostDataParam>();				
			p_Boundary = String.Format("----------{0:N}", Guid.NewGuid());
			p_DataWriter = null;
	    }
	    
	    /// <summary>
	    /// Удаление потока p_DataWriter
	    /// </summary>
	    private void DestroyDataWriter()
	    {
	    	if (p_DataWriter != null) {
	    		p_DataWriter.Close();
	    		p_DataWriter.Dispose();
	    		p_DataWriter = null;
	    	}
	    }
	    
	    /// <summary>
	    /// Возвращает MemoryStream с multi-part/formdata
	    /// </summary>
	    /// <returns>MemoryStream</returns>
	    public MemoryStream GetPostData()
	    {
	    	DestroyDataWriter();
	    	
	    	MemoryStream postDataStream = new MemoryStream();
			p_DataWriter = new StreamWriter(postDataStream);
	    		
	    	StringBuilder sb = new StringBuilder();
	    	foreach (PostDataParam p in p_Params)
	    	{	    
	    		if (p.Type == PostDataParamType.File)
	    		{
	    			p_DataWriter.Write("\r\n--" + p_Boundary + "\r\n");
					p_DataWriter.Write("Content-Disposition: form-data;"
			                        + "name=\"{0}\";"
			                        + "filename=\"{1}\""
			                        + "\r\nContent-Type: {2}\r\n\r\n",
			                        p.Name,												                        
			                        Path.GetFileName(p.FileName),
			                        p.FileContentType);
					p_DataWriter.Flush();
					FileStream fileStream = new FileStream(p.FileName, FileMode.Open, FileAccess.Read);
					byte[] buffer = new byte[1024];
					int bytesRead = 0;
					while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
					{
					    postDataStream.Write(buffer, 0, bytesRead);
					}
					fileStream.Close();
					fileStream.Dispose();	    			
	    		}
	    		else
	    		{
	    			p_DataWriter.Write("\r\n--" + p_Boundary + "\r\n");
					p_DataWriter.Write("Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}",
			                        p.Name,
			                        p.Value);	    			
	    		}
	    	}
	    	
	    	p_DataWriter.Write("\r\n--" + p_Boundary + "--\r\n");
			p_DataWriter.Flush();			
		   
	    	return postDataStream;			
	    }
	    
	    /// <summary>
        /// Деструктор       
        /// </summary>		
		public void Dispose()
        {            
			p_Params.Clear();
			p_Params = null;
			p_Boundary = "";
			DestroyDataWriter();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForFullGCComplete();
        }
	}
}
