﻿using Bot.Api;
using DSharpPlus.Entities;

namespace Bot.DSharp
{
    public class DSharpMessage : DiscordWrapper<DiscordMessage>, IMessage
    {
        public DSharpMessage(DiscordMessage wrapped)
            : base(wrapped)
        {
        }
    }
}