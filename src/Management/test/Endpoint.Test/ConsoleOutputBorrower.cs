// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the Apache 2.0 License.
// See the LICENSE file in the project root for more information.

namespace Steeltoe.Management.Endpoint.Test;

internal sealed class ConsoleOutputBorrower : IDisposable
{
    private readonly TextWriter _originalOutput;
    private StringWriter _borrowedOutput;

    public ConsoleOutputBorrower()
    {
        _originalOutput = Console.Out;
        _borrowedOutput = new StringWriter();
        Console.SetOut(_borrowedOutput);
    }

    public override string ToString()
    {
        return _borrowedOutput.ToString();
    }

    public void Dispose()
    {
        if (_borrowedOutput != null)
        {
            Console.SetOut(_originalOutput);
            _borrowedOutput.Dispose();
            _borrowedOutput = null;
        }
    }
}