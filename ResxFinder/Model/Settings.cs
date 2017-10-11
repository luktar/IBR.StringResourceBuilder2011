using ResxFinder.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Serialization;



namespace ResxFinder.Model
{
  [Serializable]
  public class Settings : ISettings
    {
    private static XmlSerializer ms_SettingsSerializer = new XmlSerializer(typeof(Settings));

    private static Regex ms_RegexNumber = new Regex(@"^\s*\d+\.?\d*\s*$");

    private bool m_IsIgnoreWhiteSpaceStrings = true;
    public bool IsIgnoreWhiteSpaceStrings
    {
      get { return (m_IsIgnoreWhiteSpaceStrings); }
      set { m_IsIgnoreWhiteSpaceStrings = value; }
    }

    private bool m_IsIgnoreNumberStrings = true;
    public bool IsIgnoreNumberStrings
    {
      get { return (m_IsIgnoreNumberStrings); }
      set { m_IsIgnoreNumberStrings = value; }
    }

    private bool m_IsIgnoreVerbatimStrings;
    public bool IsIgnoreVerbatimStrings
    {
      get { return (m_IsIgnoreVerbatimStrings); }
      set { m_IsIgnoreVerbatimStrings = value; }
    }

    private bool m_IsIgnoreStringLength = true;
    public bool IsIgnoreStringLength
    {
      get { return (m_IsIgnoreStringLength); }
      set { m_IsIgnoreStringLength = value; }
    }

    private int m_IgnoreStringLength = 2;
    public int IgnoreStringLength
    {
      get { return (m_IgnoreStringLength); }
      set { m_IgnoreStringLength = value; }
    }

    private bool m_IsUseGlobalResourceFile;
    public bool IsUseGlobalResourceFile
    {
      get { return (m_IsUseGlobalResourceFile); }
      set { m_IsUseGlobalResourceFile = value; }
    }

    private string m_GlobalResourceFileName = "SRB_Strings";
    public string GlobalResourceFileName
    {
      get { return (m_GlobalResourceFileName); }
      set { m_GlobalResourceFileName = value; }
    }

    private bool m_IsDontUseResourceAlias;
    public bool IsDontUseResourceAlias
    {
      get { return (m_IsDontUseResourceAlias); }
      set { m_IsDontUseResourceAlias  = value; }
    }

    private List<string> m_IgnoreStrings = new List<string>();
    public List<string> IgnoreStrings
    {
      get { return (m_IgnoreStrings); }
      set { m_IgnoreStrings = value; }
    }

    private List<string> m_IgnoreSubStrings = new List<string>();
    public List<string> IgnoreSubStrings
    {
      get { return (m_IgnoreSubStrings); }
      set { m_IgnoreSubStrings = value; }
    }

    private List<string> m_IgnoreMethods = new List<string>();
    public List<string> IgnoreMethods
    {
      get { return (m_IgnoreMethods); }
      set { m_IgnoreMethods = value; }
    }

    private List<string> m_IgnoreMethodsArguments = new List<string>();
    public List<string> IgnoreMethodsArguments
    {
      get { return (m_IgnoreMethodsArguments); }
      set { m_IgnoreMethodsArguments = value; }
    }

    public string Serialize()
    {
      string xmlString = null;

      try
      {
        StringBuilder xml = new StringBuilder();
        using (TextWriter writer = new StringWriter(xml))
        {
          ms_SettingsSerializer.Serialize(writer, this);
          writer.Flush();
        }

        xmlString = xml.ToString().Replace("utf-16", "utf-8");
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to serialize the settings:\n" + ex.ToString(),
                        "Serialize settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return (xmlString);
    }

    public static ISettings DeSerialize(string xml)
    {
      if (string.IsNullOrEmpty(xml))
        return (null);

      try
      {
        using (StringReader reader = new StringReader(xml))
        {
          return (ms_SettingsSerializer.Deserialize(reader) as Settings);
        }
      }
      catch (Exception ex)
      {
        MessageBox.Show("Unable to deserialize the settings:\n" + ex.ToString(),
                        "Deserialize settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }

      return (null);
    }

    public bool IgnoreString(string text)
    {
      if (m_IsIgnoreStringLength && (text.Length <= m_IgnoreStringLength))
        return (true);

      if (m_IsIgnoreWhiteSpaceStrings && string.IsNullOrEmpty(text.Trim(' ', '\t')))
        return (true);

      if (m_IsIgnoreNumberStrings && ms_RegexNumber.IsMatch(text))
        return (true);

      if (m_IgnoreStrings.Contains(text))
        return (true);

      if (m_IgnoreSubStrings.Exists(s => text.Contains(s)))
        return (true);

      return (false);
    }

    public bool IgnoreMethod(string name)
    {
      return (m_IgnoreMethods.Contains(name));
    }

    public bool IgnoreMethodArguments(string name)
    {
      return (m_IgnoreMethodsArguments.Contains(name));
    }

  }
} 
