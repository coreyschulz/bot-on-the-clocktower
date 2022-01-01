﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Api
{
	public interface IGuild
	{
		public ulong Id { get; }

		public IReadOnlyDictionary<ulong, IRole> Roles { get; }
	}
}
