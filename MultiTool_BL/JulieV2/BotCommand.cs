using Multitool.Reflection.ObjectFlatteners;

using System;
using System.Collections.Generic;

namespace Multitool.JulieV2.Commands
{
    public class BotCommand
    {
        public User Author { get; internal set; }
        public string Content { get; internal set; }
        public string CommandName { get; internal set; }
        public DateTime Date { get; internal set; }
        public CommandStatus Status { get; internal set; }
        public string Name { get; internal set; }
        [ListFlattener(nameof(Messages), typeof(CommonXmlObjectFlattener))]
        public List<string> Messages { get; internal set; }
    }
}
