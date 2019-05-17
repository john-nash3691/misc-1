using Microsoft.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CommandLine.TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            new EserCliApplication()
                .AddOutputOptions(OutputType.JSON)
                .AddSampleGeneratorOption()
                .Execute(args);
        }
    }

    public class EserCliApplication : CommandLineApplication
    {
        public Dictionary<string, EserHandler> _handlers;

        public EserCliApplication()
        {
            _handlers = new Dictionary<string, EserHandler>();
        }

        public EserCliApplication AddSampleGeneratorOption()
        {
            _handlers.Add(nameof(EventSampleHandler), new EventSampleHandler(this));
            return this;
        }

        public EserCliApplication AddOutputOptions(OutputType outputType)
        {
            _handlers.Add(nameof(OutputHandler), new OutputHandler(this, outputType));
            return this;
        }

        public override int Execute(string[] args)
        {
            this.OnExecute(OnExecute);
            return base.Execute(args);
        }

        private int OnExecute()
        {
            EserHandlerOutput _output = null;
            if (_handlers.ContainsKey(nameof(EventSampleHandler)) && _handlers[nameof(EventSampleHandler)].CanHandle())
            {
                _output = _handlers[nameof(EventSampleHandler)].Execute();
            }

            if (_handlers.ContainsKey(nameof(OutputHandler)) && _handlers[nameof(OutputHandler)].CanHandle())
            {
                _output = _handlers[nameof(OutputHandler)].Execute();
            }

            return 0;
        }
    }

    public abstract class EserHandler
    {
        internal abstract bool CanHandle();
        internal abstract EserHandlerOutput Execute();
    }

    public class EserHandlerOutput
    {

    }

    public class EserHandlerInput
    {

    }

    public class EventSampleHandler : EserHandler
    {
        private CommandOption _sampleOption;

        public EventSampleHandler(CommandLineApplication app, bool inherit = true)
        {
            _sampleOption = app.Option("-s|--sample <EVENT-TYPE>", "Generates sample JSON for corresponding Event Types", CommandOptionType.MultipleValue);
        }

        internal override EserHandlerOutput Execute()
        {
            return new EserHandlerOutput();
        }

        internal override bool CanHandle()
        {
            return _sampleOption.HasValue();
        }
    }

    public class OutputHandler : EserHandler
    {
        private OutputType _supportedTypes;
        private CommandOption _outputOption;
        public OutputHandler(CommandLineApplication app, OutputType supportedTypes)
        {
            _supportedTypes = supportedTypes;
            _outputOption = app.Option("-f|--file <FILE-PATH>", "Writes Output to file", CommandOptionType.SingleValue);
        }

        internal override bool CanHandle()
        {
            return _outputOption.HasValue();
        }

        internal override EserHandlerOutput Execute()
        {
            var outValue = _outputOption.Value();

            return new EserHandlerOutput();
        }
    }
}
