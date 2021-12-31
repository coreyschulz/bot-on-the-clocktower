﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Api
{
	public interface ITown
	{
		public ulong GuildId { get; set; }
		public ulong ControlChannelId { get; set; }
		public ulong DayCategoryId { get; set; }
		public ulong NightCategoryId { get; set; }
		public ulong ChatChannelId { get; set; }
		public ulong TownSquareId { get; set; }
		public ulong StoryTellerRoleId { get; set; }
		public ulong VillagerRoleId { get; set; }
		public string? AuthorName { get; set; }
		public DateTime Timestamp { get; set; }
	}
}
