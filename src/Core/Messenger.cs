// +---------------------------------------------------------------------------+
// |                                                                           |
// |       MadChess is developed by Erik Madsen.  Copyright 2012 - 2024.       |
// |       MadChess is free software.  It is distributed under the MIT         |
// |       license.  See LICENSE.md file for details.                          |
// |       See https://www.madchess.net/ for user and developer guides.        |
// |                                                                           |
// +---------------------------------------------------------------------------+


using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;


namespace ErikTheCoder.MadChess.Core;

public sealed class Messenger(Stream inputStream, Stream outputStream) : IAsyncDisposable
{
    public bool Debug;
    // Create stream readers.
    private readonly StreamReader _inputStreamReader = new(inputStream, leaveOpen: true);
    private readonly StreamWriter _outputStreamWriter = new(outputStream, leaveOpen: true) { AutoFlush = true };
    // Create diagnostic and synchronization objects.
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private readonly object _inputStreamLock = new();
    private readonly object _outputStreamLock = new();
    private readonly object _logLock = new();
    private StreamWriter _logWriter;


    public bool Log
    {
        get => _logWriter != null;
        set
        {
            lock (_logLock)
            {
                if (value)
                {
                    // Start logging.
                    if (_logWriter == null)
                    {
                        // Create or append to log file.
                        // Include GUID in log filename to avoid multiple engine instances interleaving lines in a single log file.
                        var file = $"MadChess-{Guid.NewGuid()}.log";
                        var fileStream = File.Open(file, FileMode.Append, FileAccess.Write, FileShare.Read);

                        _logWriter = new StreamWriter(fileStream, leaveOpen: false) { AutoFlush = true };
                    }
                }
                else
                {
                    // Stop logging.
                    _logWriter?.Close();
                    _logWriter?.Dispose();
                    _logWriter = null;
                }
            }
        }
    }


    public async ValueTask DisposeAsync()
    {
        // ReSharper disable InconsistentlySynchronizedField
        await (_outputStreamWriter?.DisposeAsync() ?? ValueTask.CompletedTask);
        await (_logWriter?.DisposeAsync() ?? ValueTask.CompletedTask);
        _inputStreamReader?.Dispose();
        // ReSharper restore InconsistentlySynchronizedField
    }


    public string ReadLine()
    {
        lock (_inputStreamLock)
        {
            var line = _inputStreamReader.ReadLine();
            if (Log) WriteLogLine(line, CommandDirection.In);
            return line;
        }
    }


    public void WriteLine()
    {
        lock (_outputStreamLock)
        {
            _outputStreamWriter.WriteLine();
            if (Log) WriteLogLine(null, CommandDirection.Out);
        }
    }


    public void WriteLine(string message)
    {
        lock (_outputStreamLock)
        {
            _outputStreamWriter.WriteLine(message);
            if (Log) WriteLogLine(message, CommandDirection.Out);
        }
    }


    private void WriteLogLine(string message, CommandDirection direction)
    {
        lock (_logLock)
        {
            var elapsed = _stopwatch.Elapsed;

            _logWriter.Write($"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}.{elapsed.Milliseconds:000}  ");
            _logWriter.Write(direction == CommandDirection.In ? " In   " : " Out  ");
            _logWriter.WriteLine(message);
        }
    }
}
