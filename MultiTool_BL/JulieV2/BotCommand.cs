using System;

namespace MultiToolBusinessLayer.JulieV2
{
    public class BotCommand
    {
        public User Author { get; internal set; }
        public string Content { get; internal set; }
        public string CommandName { get; internal set; }
        public DateTime Date { get; internal set; }
        public bool Failed { get; internal set; }
        public string Name { get; internal set; }
    }
}
