using System;

namespace Microsoft.Extensions.CommandLineUtils
{
    public class CommandParsingException : Exception
    {
        public CommandParsingException(CommandLineApplication command, string message)
          : base(message)
        {
            this.Command = command;
        }

        public CommandLineApplication Command { get; }
    }
}
