﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Api.Database
{
    public interface ICommandMetricDatabase
    {
        Task RecordCommand(string command, DateTime timestamp);
    }
}
