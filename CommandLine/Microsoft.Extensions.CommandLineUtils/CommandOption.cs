using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.CommandLineUtils
{
    public class CommandOption
    {
        public CommandOption(string template, CommandOptionType optionType)
        {
            this.Template = template;
            this.OptionType = optionType;
            this.Values = new List<string>();
            string template1 = this.Template;
            char[] separator = new char[2] { ' ', '|' };
            foreach (string str1 in template1.Split(separator, StringSplitOptions.RemoveEmptyEntries))
            {
                if (str1.StartsWith("--"))
                    this.LongName = str1.Substring(2);
                else if (str1.StartsWith("-"))
                {
                    string str2 = str1.Substring(1);
                    if (str2.Length == 1 && !this.IsEnglishLetter(str2[0]))
                        this.SymbolName = str2;
                    else
                        this.ShortName = str2;
                }
                else
                {
                    if (!str1.StartsWith("<") || !str1.EndsWith(">"))
                        throw new ArgumentException(string.Format("Invalid template pattern '{0}'", (object)template), nameof(template));
                    this.ValueName = str1.Substring(1, str1.Length - 2);
                }
            }
            if (string.IsNullOrEmpty(this.LongName) && string.IsNullOrEmpty(this.ShortName) && string.IsNullOrEmpty(this.SymbolName))
                throw new ArgumentException(string.Format("Invalid template pattern '{0}'", (object)template), nameof(template));
        }

        public string Template { get; set; }

        public string ShortName { get; set; }

        public string LongName { get; set; }

        public string SymbolName { get; set; }

        public string ValueName { get; set; }

        public string Description { get; set; }

        public List<string> Values { get; private set; }

        public CommandOptionType OptionType { get; private set; }

        public bool ShowInHelpText { get; set; } = true;

        public bool Inherited { get; set; }

        public bool TryParse(string value)
        {
            switch (this.OptionType)
            {
                case CommandOptionType.MultipleValue:
                    this.Values.Add(value);
                    break;
                case CommandOptionType.SingleValue:
                    if (this.Values.Any<string>())
                        return false;
                    this.Values.Add(value);
                    break;
                case CommandOptionType.NoValue:
                    if (value != null)
                        return false;
                    this.Values.Add("on");
                    break;
            }
            return true;
        }

        public bool HasValue()
        {
            return this.Values.Any<string>();
        }

        public string Value()
        {
            if (!this.HasValue())
                return (string)null;
            return this.Values[0];
        }

        private bool IsEnglishLetter(char c)
        {
            if (c >= 'a' && c <= 'z')
                return true;
            if (c >= 'A')
                return c <= 'Z';
            return false;
        }
    }
}
