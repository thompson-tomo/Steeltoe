// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

using Serilog.Core;
using Serilog.Events;

namespace Steeltoe.Extensions.Logging.DynamicSerilog.Test;

public class TestSink : ILogEventSink
{
    private static TestSink _currentSink;

    private readonly List<string> _logs = new();

    internal static TestSink GetCurrentSink(bool createNew = false)
    {
        if (createNew)
        {
            _currentSink = new TestSink();
        }

        return _currentSink ??= new TestSink();
    }

    public void Emit(LogEvent logEvent)
    {
        _logs.Add(logEvent.RenderMessage());
    }

    public List<string> GetLogs()
    {
        return _logs;
    }
}