using System;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Text;

public class XMLSerialization
{

    public string SerializeObject(System.Object obj)
    {
        try
        {
            string _XmlizedString = null;
            MemoryStream _memoryStream = new MemoryStream();
            XmlSerializer _xs = new XmlSerializer(obj.GetType());
            XmlTextWriter _xmlTextWriter = new XmlTextWriter(_memoryStream, Encoding.GetEncoding("ISO-8859-1"));

            _xs.Serialize(_xmlTextWriter, obj);
            _memoryStream = (MemoryStream)_xmlTextWriter.BaseStream;
            _XmlizedString = ByteArrayToString(_memoryStream.ToArray());

            Debug.Log("Wrote Scalp");

            return _XmlizedString;
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            Debug.Log("Error Saving Scalp");
            return null;
        }
    }

    public T DeserializeObject<T>(string xml)
    {
        try
        {
            XmlSerializer _xs = new XmlSerializer(typeof(T));
            MemoryStream _memoryStream = new MemoryStream(StringToByteArray(xml));
            //XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.GetEncoding("ISO-8859-1") );
            return (T)_xs.Deserialize(_memoryStream);
        }
        catch (Exception e)
        {
            System.Console.WriteLine(e);
            return default(T);
        }
    }

    public static byte[] StringToByteArray(string s)
    {
        byte[] b = new byte[s.Length];

        for (int i = 0; i < s.Length; i++)
            b[i] = (byte)s[i];

        return b;
    }

    public static string ByteArrayToString(byte[] b)
    {
        string s = "";

        for (int i = 0; i < b.Length; i++)
            s += (char)b[i];

        return s;
    }
}