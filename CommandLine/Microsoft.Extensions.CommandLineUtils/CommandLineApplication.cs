using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Extensions.CommandLineUtils
{
    public class CommandLineApplication
    {
        private readonly bool _throwOnUnexpectedArg;
        public readonly List<CommandOption> Options;
        public readonly List<CommandArgument> Arguments;
        public readonly List<string> RemainingArguments;
        public readonly List<CommandLineApplication> Commands;

        public CommandLineApplication(bool throwOnUnexpectedArg = true)
        {
            this._throwOnUnexpectedArg = throwOnUnexpectedArg;
            this.Options = new List<CommandOption>();
            this.Arguments = new List<CommandArgument>();
            this.Commands = new List<CommandLineApplication>();
            this.RemainingArguments = new List<string>();
            this.Invoke = (Func<int>)(() => 0);
        }

        public CommandLineApplication Parent { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public string Syntax { get; set; }

        public string Description { get; set; }

        public bool ShowInHelpText { get; set; } = true;

        public string ExtendedHelpText { get; set; }

        public CommandOption OptionHelp { get; private set; }

        public CommandOption OptionVersion { get; private set; }

        public bool IsShowingInformation { get; protected set; }

        public Func<int> Invoke { get; set; }

        public Func<string> LongVersionGetter { get; set; }

        public Func<string> ShortVersionGetter { get; set; }

        public bool AllowArgumentSeparator { get; set; }

        public TextWriter Out { get; set; } = Console.Out;

        public TextWriter Error { get; set; } = Console.Error;

        public IEnumerable<CommandOption> GetOptions()
        {
            IEnumerable<CommandOption> first = this.Options.AsEnumerable<CommandOption>();
            CommandLineApplication commandLineApplication = this;
            while (commandLineApplication.Parent != null)
            {
                commandLineApplication = commandLineApplication.Parent;
                first = first.Concat<CommandOption>(commandLineApplication.Options.Where<CommandOption>((Func<CommandOption, bool>)(o => o.Inherited)));
            }
            return first;
        }

        public CommandLineApplication Command(
          string name,
          Action<CommandLineApplication> configuration,
          bool throwOnUnexpectedArg = true)
        {
            CommandLineApplication commandLineApplication = new CommandLineApplication(throwOnUnexpectedArg)
            {
                Name = name,
                Parent = this
            };
            this.Commands.Add(commandLineApplication);
            configuration(commandLineApplication);
            return commandLineApplication;
        }

        public CommandOption Option(
          string template,
          string description,
          CommandOptionType optionType)
        {
            return this.Option(template, description, optionType, (Action<CommandOption>)(_ => { }), false);
        }

        public CommandOption Option(
          string template,
          string description,
          CommandOptionType optionType,
          bool inherited)
        {
            return this.Option(template, description, optionType, (Action<CommandOption>)(_ => { }), inherited);
        }

        public CommandOption Option(
          string template,
          string description,
          CommandOptionType optionType,
          Action<CommandOption> configuration)
        {
            return this.Option(template, description, optionType, configuration, false);
        }

        public CommandOption Option(
          string template,
          string description,
          CommandOptionType optionType,
          Action<CommandOption> configuration,
          bool inherited)
        {
            CommandOption commandOption = new CommandOption(template, optionType)
            {
                Description = description,
                Inherited = inherited
            };
            this.Options.Add(commandOption);
            configuration(commandOption);
            return commandOption;
        }

        public CommandArgument Argument(
          string name,
          string description,
          bool multipleValues = false)
        {
            return this.Argument(name, description, (Action<CommandArgument>)(_ => { }), multipleValues);
        }

        public CommandArgument Argument(
          string name,
          string description,
          Action<CommandArgument> configuration,
          bool multipleValues = false)
        {
            CommandArgument commandArgument1 = this.Arguments.LastOrDefault<CommandArgument>();
            if (commandArgument1 != null && commandArgument1.MultipleValues)
                throw new InvalidOperationException(string.Format("The last argument '{0}' accepts multiple values. No more argument can be added.", (object)commandArgument1.Name));
            CommandArgument commandArgument2 = new CommandArgument()
            {
                Name = name,
                Description = description,
                MultipleValues = multipleValues
            };
            this.Arguments.Add(commandArgument2);
            configuration(commandArgument2);
            return commandArgument2;
        }

        protected void OnExecute(Func<int> invoke)
        {
            this.Invoke = invoke;
        }

        //public void OnExecute(Func<Task<int>> invoke)
        //{
        //    this.Invoke = (Func<int>)(() => invoke().Result);
        //}

        public virtual int Execute(params string[] args)
        {
            CommandLineApplication command1 = this;
            CommandOption commandOption = (CommandOption)null;
            IEnumerator<CommandArgument> enumerator = (IEnumerator<CommandArgument>)null;
            for (int index = 0; index < args.Length; ++index)
            {
                string b = args[index];
                bool flag = false;
                if (!flag && commandOption == null)
                {
                    string[] strArray = (string[])null;
                    string[] shortOption = (string[])null;
                    if (b.StartsWith("--"))
                        strArray = b.Substring(2).Split(new char[2]
                        {
              ':',
              '='
                        }, 2);
                    else if (b.StartsWith("-"))
                        shortOption = b.Substring(1).Split(new char[2]
                        {
              ':',
              '='
                        }, 2);
                    if (strArray != null)
                    {
                        flag = true;
                        string longOptionName = strArray[0];
                        commandOption = command1.GetOptions().SingleOrDefault<CommandOption>((Func<CommandOption, bool>)(opt => string.Equals(opt.LongName, longOptionName, StringComparison.Ordinal)));
                        if (commandOption == null)
                        {
                            if (string.IsNullOrEmpty(longOptionName) && !command1._throwOnUnexpectedArg && this.AllowArgumentSeparator)
                                ++index;
                            this.HandleUnexpectedArg(command1, args, index, "option");
                            break;
                        }
                        if (command1.OptionHelp == commandOption)
                        {
                            command1.ShowHelp((string)null);
                            return 0;
                        }
                        if (command1.OptionVersion == commandOption)
                        {
                            command1.ShowVersion();
                            return 0;
                        }
                        if (strArray.Length == 2)
                        {
                            if (!commandOption.TryParse(strArray[1]))
                            {
                                command1.ShowHint();
                                throw new CommandParsingException(command1, string.Format("Unexpected value '{0}' for option '{1}'", (object)strArray[1], (object)commandOption.LongName));
                            }
                            commandOption = (CommandOption)null;
                        }
                        else if (commandOption.OptionType == CommandOptionType.NoValue)
                        {
                            commandOption.TryParse((string)null);
                            commandOption = (CommandOption)null;
                        }
                    }
                    if (shortOption != null)
                    {
                        flag = true;
                        commandOption = command1.GetOptions().SingleOrDefault<CommandOption>((Func<CommandOption, bool>)(opt => string.Equals(opt.ShortName, shortOption[0], StringComparison.Ordinal))) ?? command1.GetOptions().SingleOrDefault<CommandOption>((Func<CommandOption, bool>)(opt => string.Equals(opt.SymbolName, shortOption[0], StringComparison.Ordinal)));
                        if (commandOption == null)
                        {
                            this.HandleUnexpectedArg(command1, args, index, "option");
                            break;
                        }
                        if (command1.OptionHelp == commandOption)
                        {
                            command1.ShowHelp((string)null);
                            return 0;
                        }
                        if (command1.OptionVersion == commandOption)
                        {
                            command1.ShowVersion();
                            return 0;
                        }
                        if (shortOption.Length == 2)
                        {
                            if (!commandOption.TryParse(shortOption[1]))
                            {
                                command1.ShowHint();
                                throw new CommandParsingException(command1, string.Format("Unexpected value '{0}' for option '{1}'", (object)shortOption[1], (object)commandOption.LongName));
                            }
                            commandOption = (CommandOption)null;
                        }
                        else if (commandOption.OptionType == CommandOptionType.NoValue)
                        {
                            commandOption.TryParse((string)null);
                            commandOption = (CommandOption)null;
                        }
                    }
                }
                if (!flag && commandOption != null)
                {
                    flag = true;
                    if (!commandOption.TryParse(b))
                    {
                        command1.ShowHint();
                        throw new CommandParsingException(command1, string.Format("Unexpected value '{0}' for option '{1}'", (object)b, (object)commandOption.LongName));
                    }
                    commandOption = (CommandOption)null;
                }
                if (!flag && enumerator == null)
                {
                    CommandLineApplication commandLineApplication = command1;
                    foreach (CommandLineApplication command2 in command1.Commands)
                    {
                        if (string.Equals(command2.Name, b, StringComparison.OrdinalIgnoreCase))
                        {
                            flag = true;
                            command1 = command2;
                            break;
                        }
                    }
                    if (command1 != commandLineApplication)
                        flag = true;
                }
                if (!flag)
                {
                    if (enumerator == null)
                        enumerator = (IEnumerator<CommandArgument>)new CommandLineApplication.CommandArgumentEnumerator((IEnumerator<CommandArgument>)command1.Arguments.GetEnumerator());
                    if (enumerator.MoveNext())
                    {
                        flag = true;
                        enumerator.Current.Values.Add(b);
                    }
                }
                if (!flag)
                {
                    this.HandleUnexpectedArg(command1, args, index, "command or argument");
                    break;
                }
            }
            if (commandOption != null)
            {
                command1.ShowHint();
                throw new CommandParsingException(command1, string.Format("Missing value for option '{0}'", (object)commandOption.LongName));
            }
            return command1.Invoke();
        }

        public CommandOption HelpOption(string template)
        {
            this.OptionHelp = this.Option(template, "Show help information", CommandOptionType.NoValue);
            return this.OptionHelp;
        }

        public CommandOption VersionOption(
          string template,
          string shortFormVersion,
          string longFormVersion = null)
        {
            if (longFormVersion == null)
                return this.VersionOption(template, (Func<string>)(() => shortFormVersion), (Func<string>)null);
            return this.VersionOption(template, (Func<string>)(() => shortFormVersion), (Func<string>)(() => longFormVersion));
        }

        public CommandOption VersionOption(
          string template,
          Func<string> shortFormVersionGetter,
          Func<string> longFormVersionGetter = null)
        {
            this.OptionVersion = this.Option(template, "Show version information", CommandOptionType.NoValue);
            this.ShortVersionGetter = shortFormVersionGetter;
            this.LongVersionGetter = longFormVersionGetter ?? shortFormVersionGetter;
            return this.OptionVersion;
        }

        public void ShowHint()
        {
            if (this.OptionHelp == null)
                return;
            this.Out.WriteLine(string.Format("Specify --{0} for a list of available options and commands.", (object)this.OptionHelp.LongName));
        }

        public void ShowHelp(string commandName = null)
        {
            for (CommandLineApplication commandLineApplication = this; commandLineApplication != null; commandLineApplication = commandLineApplication.Parent)
                commandLineApplication.IsShowingInformation = true;
            this.Out.WriteLine(this.GetHelpText(commandName));
        }

        public virtual string GetHelpText(string commandName = null)
        {
            StringBuilder stringBuilder1 = new StringBuilder("Usage:");
            for (CommandLineApplication commandLineApplication = this; commandLineApplication != null; commandLineApplication = commandLineApplication.Parent)
                stringBuilder1.Insert(6, string.Format(" {0}", (object)commandLineApplication.Name));
            CommandLineApplication commandLineApplication1;
            if (commandName == null || string.Equals(this.Name, commandName, StringComparison.OrdinalIgnoreCase))
            {
                commandLineApplication1 = this;
            }
            else
            {
                commandLineApplication1 = this.Commands.SingleOrDefault<CommandLineApplication>((Func<CommandLineApplication, bool>)(cmd => string.Equals(cmd.Name, commandName, StringComparison.OrdinalIgnoreCase)));
                if (commandLineApplication1 != null)
                    stringBuilder1.AppendFormat(" {0}", (object)commandName);
                else
                    commandLineApplication1 = this;
            }
            StringBuilder stringBuilder2 = new StringBuilder();
            StringBuilder stringBuilder3 = new StringBuilder();
            StringBuilder stringBuilder4 = new StringBuilder();
            List<CommandArgument> list1 = commandLineApplication1.Arguments.Where<CommandArgument>((Func<CommandArgument, bool>)(a => a.ShowInHelpText)).ToList<CommandArgument>();
            if (list1.Any<CommandArgument>())
            {
                stringBuilder1.Append(" [arguments]");
                stringBuilder4.AppendLine();
                stringBuilder4.AppendLine("Arguments:");
                string format = string.Format("  {{0, -{0}}}{{1}}", (object)(list1.Max<CommandArgument>((Func<CommandArgument, int>)(a => a.Name.Length)) + 2));
                foreach (CommandArgument commandArgument in list1)
                {
                    stringBuilder4.AppendFormat(format, (object)commandArgument.Name, (object)commandArgument.Description);
                    stringBuilder4.AppendLine();
                }
            }
            List<CommandOption> list2 = commandLineApplication1.GetOptions().Where<CommandOption>((Func<CommandOption, bool>)(o => o.ShowInHelpText)).ToList<CommandOption>();
            if (list2.Any<CommandOption>())
            {
                stringBuilder1.Append(" [options]");
                stringBuilder2.AppendLine();
                stringBuilder2.AppendLine("Options:");
                string format = string.Format("  {{0, -{0}}}{{1}}", (object)(list2.Max<CommandOption>((Func<CommandOption, int>)(o => o.Template.Length)) + 2));
                foreach (CommandOption commandOption in list2)
                {
                    stringBuilder2.AppendFormat(format, (object)commandOption.Template, (object)commandOption.Description);
                    stringBuilder2.AppendLine();
                }
            }
            List<CommandLineApplication> list3 = commandLineApplication1.Commands.Where<CommandLineApplication>((Func<CommandLineApplication, bool>)(c => c.ShowInHelpText)).ToList<CommandLineApplication>();
            if (list3.Any<CommandLineApplication>())
            {
                stringBuilder1.Append(" [command]");
                stringBuilder3.AppendLine();
                stringBuilder3.AppendLine("Commands:");
                string format = string.Format("  {{0, -{0}}}{{1}}", (object)(list3.Max<CommandLineApplication>((Func<CommandLineApplication, int>)(c => c.Name.Length)) + 2));
                foreach (CommandLineApplication commandLineApplication2 in (IEnumerable<CommandLineApplication>)list3.OrderBy<CommandLineApplication, string>((Func<CommandLineApplication, string>)(c => c.Name)))
                {
                    stringBuilder3.AppendFormat(format, (object)commandLineApplication2.Name, (object)commandLineApplication2.Description);
                    stringBuilder3.AppendLine();
                }
                if (this.OptionHelp != null)
                {
                    stringBuilder3.AppendLine();
                    stringBuilder3.AppendFormat(string.Format("Use \"{0} [command] --{1}\" for more information about a command.", (object)commandLineApplication1.Name, (object)this.OptionHelp.LongName));
                    stringBuilder3.AppendLine();
                }
            }
            if (commandLineApplication1.AllowArgumentSeparator)
                stringBuilder1.Append(" [[--] <arg>...]");
            stringBuilder1.AppendLine();
            StringBuilder stringBuilder5 = new StringBuilder();
            stringBuilder5.AppendLine(this.GetFullNameAndVersion());
            stringBuilder5.AppendLine();
            return stringBuilder5.ToString() + stringBuilder1.ToString() + stringBuilder4.ToString() + stringBuilder2.ToString() + stringBuilder3.ToString() + commandLineApplication1.ExtendedHelpText;
        }

        public void ShowVersion()
        {
            for (CommandLineApplication commandLineApplication = this; commandLineApplication != null; commandLineApplication = commandLineApplication.Parent)
                commandLineApplication.IsShowingInformation = true;
            this.Out.WriteLine(this.FullName);
            this.Out.WriteLine(this.LongVersionGetter());
        }

        public string GetFullNameAndVersion()
        {
            if (this.ShortVersionGetter != null)
                return string.Format("{0} {1}", (object)this.FullName, (object)this.ShortVersionGetter());
            return this.FullName;
        }

        public void ShowRootCommandFullNameAndVersion()
        {
            CommandLineApplication commandLineApplication = this;
            while (commandLineApplication.Parent != null)
                commandLineApplication = commandLineApplication.Parent;
            this.Out.WriteLine(commandLineApplication.GetFullNameAndVersion());
            this.Out.WriteLine();
        }

        private void HandleUnexpectedArg(
          CommandLineApplication command,
          string[] args,
          int index,
          string argTypeName)
        {
            if (command._throwOnUnexpectedArg)
            {
                command.ShowHint();
                throw new CommandParsingException(command, string.Format("Unrecognized {0} '{1}'", (object)argTypeName, (object)args[index]));
            }
            command.RemainingArguments.AddRange((IEnumerable<string>)new ArraySegment<string>(args, index, args.Length - index));
        }

        private class CommandArgumentEnumerator : IEnumerator<CommandArgument>, IEnumerator, IDisposable
        {
            private readonly IEnumerator<CommandArgument> _enumerator;

            public CommandArgumentEnumerator(IEnumerator<CommandArgument> enumerator)
            {
                this._enumerator = enumerator;
            }

            public CommandArgument Current
            {
                get
                {
                    return this._enumerator.Current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return (object)this.Current;
                }
            }

            public void Dispose()
            {
                this._enumerator.Dispose();
            }

            public bool MoveNext()
            {
                if (this.Current == null || !this.Current.MultipleValues)
                    return this._enumerator.MoveNext();
                return true;
            }

            public void Reset()
            {
                this._enumerator.Reset();
            }
        }
    }
}
