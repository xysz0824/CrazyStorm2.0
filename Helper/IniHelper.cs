using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

public class IniHelper
{
    private string fileName;
    public string FileName
    {
        get { return fileName; }
        set 
        {
            if (value != fileName)
            {
                fileName = value;
            }
        }
    }
    [DllImport("kernel32")]
    private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

    [DllImport("kernel32")]
    private static extern int GetPrivateProfileString(string section, string key, string defVal, System.Text.StringBuilder retVal, int size, string filePath);

    public IniHelper(string FileName)
    {
        fileName = Path.GetFullPath(FileName);
    }
    public void WriteValue(string Section, string Key, string Value)
    {
        WritePrivateProfileString(Section, Key, Value, this.fileName);
    }
    public string ReadValue(string Section, string Key, string Default)
    {
        StringBuilder temp = new StringBuilder(256);
        int i = GetPrivateProfileString(Section, Key, Default, temp, 256, this.fileName);
        return temp.ToString();
    }
    public bool ExistINIFile()
    {
        return File.Exists(fileName);
    }
    public void WriteValue(string Section, string Key, int Value)
    {
        WriteValue(Section, Key, Value.ToString());
    }
    public void WriteValue(string Section, string Key, Boolean Value)
    {
        WriteValue(Section, Key, Value.ToString());
    }
    public void WriteValue(string Section, string Key, DateTime Value)
    {
        WriteValue(Section, Key, Value.ToString());
    }
    public void WriteValue(string Section, string Key, object Value)
    {
        WriteValue(Section, Key, Value.ToString());
    }
    public int ReadValue(string Section, string Key, int Default)
    {
        return Convert.ToInt32(ReadValue(Section, Key, Default.ToString()));
    }
    public bool ReadValue(string Section, string Key, bool Default)
    {
        return Convert.ToBoolean(ReadValue(Section, Key, Default.ToString()));
    }
    public DateTime ReadValue(string Section, string Key, DateTime Default)
    {
        return Convert.ToDateTime(ReadValue(Section, Key, Default.ToString()));
    }
    public string ReadValue(string Section, string Key)
    {
        return ReadValue(Section, Key, string.Empty);
    }
}
