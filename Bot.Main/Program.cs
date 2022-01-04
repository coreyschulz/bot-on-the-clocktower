﻿using Bot.Api;
using Bot.Base;
using Bot.Core;
using Bot.Database;
using Bot.DSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bot.Main
{
    public class Program
    {
        static async Task Main(string[] _)
        {
            DotEnv.Load(@"..\..\..\..\.env");

            var sp = RegisterServices();
            // Register IBotSystem (and any other DSharp services) first, since it's a core factory piece
            sp = DSharp.ServiceFactory.RegisterServices(sp);
            sp = Database.ServiceFactory.RegisterServices(sp);
            sp = Core.ServiceFactory.RegisterServices(sp);

            DatabaseFactory dbp = new(sp);
            sp = dbp.Connect();

            //// TEST CODE
            //// Demonstrates that we can successfully get a Town's details from the ITownLookup service
            //ITownLookup itl = sp.GetService<ITownLookup>();
            //ITown t = await itl.GetTown(128585855097896963, 826858511438839879);
            //Console.WriteLine(t);
            //// END TEST CODE

            BotSystemRunner botRunner = new(sp);

            await botRunner.RunAsync(CancellationToken.None);
        }

        public static IServiceProvider RegisterServices()
        {
            var sp = new ServiceProvider();
            sp.AddService<IEnvironment>(new ProgramEnvironment());
            return sp;
        }
    }
}
