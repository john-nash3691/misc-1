using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLine.TestConsole
{
    [Flags]
    public enum OutputType
    {
        JSON = 1,
        XML = 2
    }
}
