using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Telemonitor
{
    class iniFile
    {
        public string path;

        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section,
            string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section,
                 string key, string def, StringBuilder retVal,
            int size, string filePath);

        /// <summary>
        /// Конструктор класса
        /// </summary>
        /// <PARAM name="INIPath">Путь к INI-файлу</PARAM>
        public iniFile(string INIPath)
        {
            path = INIPath;
        }
        /// <summary>
        /// Запись данных в INI-файл
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// Название секции
        /// <PARAM name="Key"></PARAM>
        /// Имя ключа
        /// <PARAM name="Value"></PARAM>
        /// Значение
        public void IniWriteValue(string Section, string Key, string Value)
        {
            WritePrivateProfileString(Section, Key, Value, this.path);
        }

        /// <summary>
        /// Чтение данных из INI-файла
        /// </summary>
        /// <PARAM name="Section"></PARAM>
        /// <PARAM name="Key"></PARAM>
        /// <returns>Значение заданного ключа</returns>
        public string IniReadValue(string Section, string Key)
        {
            StringBuilder temp = new StringBuilder(255);
            int i = GetPrivateProfileString(Section, Key, "", temp,
                                            255, this.path);
            return temp.ToString();
        }
    }
}
