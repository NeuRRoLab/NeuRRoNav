using System.Collections.Generic;
using System.IO;
using System;

class FileIO
{
    string[] loggingString;
    int linesWritten;
    int lastLineSaved;
    int columns;
    string filePath;
    string fileName;
    int linesPerCommit;
    StreamWriter writer;
    bool dirty;
    bool logging;
    string[] headers;

    public FileIO(int columns, string filepath, string filename, int linesPerCommit)
    {
        construct(columns, filepath, filename, linesPerCommit);
        headers = null;
    }
    public FileIO(int columns, string filepath, string filename, int linesPerCommit, string[] headers)
    {
        construct(columns, filepath, filename, linesPerCommit);
        this.headers = headers;
        writeHeaders();
    }
    private void construct(int columns, string filepath, string filename, int linesPerCommit)
    {
        loggingString = new string[columns];
        linesWritten = 0;
        this.columns = columns;
        this.fileName = filename;
        this.filePath = filepath;

        writer = new StreamWriter(filePath + fileName);
        this.linesPerCommit = linesPerCommit;
        dirty = false;
        logging = false;

        for (int i = 0; i < columns; i++)
        {
            loggingString[i] = string.Empty;
        }
    }
    public bool Log()
    {
        if (logging)
        {
            if (dirty)
            {
                string psring = string.Empty;
                for (int i = 0; i < columns; i++)
                {
                    psring += loggingString[i] + "\t";
                }
                writer.WriteLine(psring);
                linesWritten++;
                if (linesWritten % linesPerCommit == 0)
                {
                    writer.Flush();
                }
                flush();
            }
            return true;
        }
        else
        {
            return false;
        }
    }
    public void setColumn(int column, string data)
    {
        if (!dirty)
        {
            dirty = true;
        }
        else if (!loggingString[column].Equals(string.Empty) && !loggingString[column].Equals("\t"))
        {
            Log();
        }
        loggingString[column] = data;
    }

    private void flush()
    {
        for (int i = 0; i < columns; i++)
        {
            loggingString[i] = string.Empty;
        }
        dirty = false;
    }
    public void Close()
    {
        ForceLog();
        writer.Close();
    }
    public bool toggleLogging()
    {
        logging = !logging;
        if (!logging)
        {
            ForceLog();
        }
        else
        {
            flush();
        }
        return logging;
    }
    public void toggleLogging(bool b)
    {
        if (logging && !b)
        {
            ForceLog();
        }
        else if(!logging && b)
        {
            flush();
        }
        logging = b;
    }
    public void SetFilePath(string path)
    {
        Close();
        filePath = path;
        construct(columns, filePath, fileName, linesPerCommit);
        if (headers != null)
        {
            writeHeaders();
        }
    }
    public void SetFileName(string name)
    {
        Close();
        fileName = name;
        construct(columns, filePath, fileName, linesPerCommit);
        if (headers != null)
        {
            writeHeaders();
        }
    }
    public int getLine()
    {
        return linesWritten;
    }
    public string getData(int column)
    {
        return loggingString[column];
    }
    private void writeHeaders()
    {
        int i = 0;
        foreach (string head in headers)
        {
            setColumn(i, head);
            i++;
        }
        ForceLog();
    }
    private void ForceLog()
    {
        if (dirty)
        {
            string psring = string.Empty;
            for (int i = 0; i < columns; i++)
            {
                psring += loggingString[i] + "\t";
            }
            writer.WriteLine(psring);
            linesWritten++;
        }
            writer.Flush();
            flush();
    }
}
