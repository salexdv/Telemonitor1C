/*
 * Created by SharpDevelop.
 * User: Alex
 * Date: 06.08.2015
 * Time: 12:29
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace Telemonitor
{
	
	/// <summary>
	/// Структура для хранения команды
	/// </summary>
	public struct Command : IEquatable<Command>
	{		
		int dbVersion;
		string dbConString;
		string commandID;
		string commandDescr;
		string commandCode;
		bool keyboardCommand;
			
		/// <summary>
        /// Версия 8.x
        /// </summary>
		public int Version
		{
			get 
			{
				return dbVersion;
			}
			set
			{
				dbVersion = value;	
			}
		}
		
		/// <summary>
        /// Строка соединения
        /// </summary>
		public string ConnectionString
		{
			get 
			{
				return dbConString;
			}
			set
			{
				dbConString = value;	
			}
		}
		
		/// <summary>
        /// Идентификатор команды
        /// </summary>
		public string ID
		{
			get 
			{
				return commandID;
			}
			set
			{
				commandID = value;	
			}
		}
		
		/// <summary>
        /// Описание команды
        /// </summary>
		public string Description
		{
			get 
			{
				return commandDescr;
			}
			set
			{
				commandDescr = value;	
			}
		}
		
		/// <summary>
        /// Код команды
        /// </summary>
		public string Code
		{
			get 
			{
				return commandCode;
			}
			set
			{
				commandCode = value;	
			}
		}
		
		/// <summary>
        /// Признак того, что команда является клавиатурной
		/// (показывается в виде кнопки доп.клавиатуры)
        /// </summary>
		public bool KeyboardCommand
		{
			get 
			{
				return keyboardCommand;
			}
			set
			{
				keyboardCommand = value;	
			}
		}
		
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<Structures>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is Command)
				return Equals((Command)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(Command other)
		{
			// add comparisions for all members here
			return (this.dbVersion == other.dbVersion 
			        && this.dbConString == other.dbConString
			        && this.commandID == other.commandID
			        && this.commandDescr == other.commandDescr
			        && this.commandCode == other.commandCode);
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return dbVersion.GetHashCode() ^ dbConString.GetHashCode() ^ commandID.GetHashCode() ^ commandDescr.GetHashCode() ^ commandCode.GetHashCode();
		}
		
		public static bool operator ==(Command left, Command right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(Command left, Command right)
		{
			return !left.Equals(right);
		}
		#endregion
	}
	
	/// <summary>
	/// Структура для хранения списка команд базы данных
	/// </summary>
	public struct DBCommand : IEquatable<DBCommand>
	{
		string commandName;
		string commandDescr;
		string commandCode;
		bool keyboardCommand; 
			
		/// <summary>
        /// Имя команды
        /// </summary>
		public string Name
		{
			get 
			{
				return commandName;
			}
			set
			{
				commandName = value;	
			}
		}
		
		/// <summary>
        /// Описание команды
        /// </summary>
		public string Description
		{
			get 
			{
				return commandDescr;
			}
			set
			{
				commandDescr = value;	
			}
		}
		
		/// <summary>
        /// Код команды
        /// </summary>
		public string Code
		{
			get 
			{
				return commandCode;
			}
			set
			{
				commandCode = value;	
			}
		}
		
		/// <summary>
        /// Признак того, что команда является клавиатурной
		/// (показывается в виде кнопки доп.клавиатуры)
        /// </summary>
		public bool KeyboardCommand
		{
			get 
			{
				return keyboardCommand;
			}
			set
			{
				keyboardCommand = value;	
			}
		}
		
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<Structures>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is DBCommand)
				return Equals((DBCommand)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(DBCommand other)
		{
			// add comparisions for all members here
			return (this.commandName == other.commandName 
			        && this.commandDescr == other.commandDescr
			        && this.commandCode == other.commandCode);
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return commandName.GetHashCode() ^ commandDescr.GetHashCode() ^ commandCode.GetHashCode();
		}
		
		public static bool operator ==(DBCommand left, DBCommand right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(DBCommand left, DBCommand right)
		{
			return !left.Equals(right);
		}
		#endregion
	}
	
	/// <summary>
	/// Структура для хранения настроек базы данных
	/// </summary>
	public struct DBStruct : IEquatable<DBStruct>
	{
		string dbName;
		int dbVersion;
		string dbConString;
		List<DBCommand> commands;
		
		/// <summary>
        /// Имя базы данных
        /// </summary>
		public string Name
		{
			get 
			{
				return dbName;
			}
			set
			{
				dbName = value;	
			}
		}
		
		/// <summary>
        /// Версия базы данных (81, 82, 83)
        /// </summary>
		public int Version
		{
			get 
			{
				return dbVersion;
			}
			set
			{
				dbVersion = value;	
			}
		}
		
		/// <summary>
        /// Строка соединения
        /// </summary>
		public string ConnectionString
		{
			get 
			{
				return dbConString;
			}
			set
			{
				dbConString = value;	
			}
		}
		
		/// <summary>
        /// Строка соединения
        /// </summary>
		public List<DBCommand> Commands
		{
			get 
			{
				return commands;
			}
			set
			{
				commands = value;	
			}
		}
		
		#region Equals and GetHashCode implementation
		// The code in this region is useful if you want to use this structure in collections.
		// If you don't need it, you can just remove the region and the ": IEquatable<Structures>" declaration.
		
		public override bool Equals(object obj)
		{
			if (obj is DBStruct)
				return Equals((DBStruct)obj); // use Equals method below
			else
				return false;
		}
		
		public bool Equals(DBStruct other)
		{
			// add comparisions for all members here
			return (this.dbName == other.dbName
			       && this.dbVersion == other.dbVersion
			       && this.dbConString == other.dbConString
			      );
		}
		
		public override int GetHashCode()
		{
			// combine the hash codes of all members here (e.g. with XOR operator ^)
			return dbName.GetHashCode() ^ dbVersion.GetHashCode() ^ dbConString.GetHashCode();
		}
		
		public static bool operator ==(DBStruct left, DBStruct right)
		{
			return left.Equals(right);
		}
		
		public static bool operator !=(DBStruct left, DBStruct right)
		{
			return !left.Equals(right);
		}
		#endregion
	}
}
