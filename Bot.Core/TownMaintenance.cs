﻿using Bot.Api;
using Bot.Api.Database;
using Bot.Core.Callbacks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core
{
    internal class TownMaintenance : ITownMaintenance
    {
        const int NUM_TOWNS_PER_CALLBACK = 5;
        const int MINUTES_PER_CALLBACK = 5;
        const int DAYS_BETWEEN_MAINTENANCE = 14;

        private readonly ICallbackScheduler<Queue<TownKey>> m_callbackScheduler;
        private readonly ICallbackScheduler<bool> m_longWaitScheduler;
        private readonly ITownDatabase m_townDatabase;
        private readonly IBotClient m_botClient;
        private readonly IDateTime m_dateTime;
        private readonly IShutdownPreventionService m_shutdownPrevention;

        private readonly TaskCompletionSource m_shutdownTcs = new();
        private bool m_shutdownRequested = false;
        private bool m_queueProcessing = false;

        List<Func<TownKey, Task>> m_startupTasks = new();

        public TownMaintenance(IServiceProvider sp)
        {
            sp.Inject(out m_dateTime);
            sp.Inject(out m_townDatabase);
            sp.Inject(out m_botClient);
            sp.Inject(out m_shutdownPrevention);

            m_shutdownPrevention.ShutdownRequested += (s, e) =>
            {
                m_shutdownRequested = true;
                if (!m_queueProcessing)
                {
                    m_shutdownTcs.TrySetResult();
                }
            };
            m_shutdownPrevention.RegisterShutdownPreventer(m_shutdownTcs.Task);

            var callbackFactory = sp.GetService<ICallbackSchedulerFactory>();
            // Scheduler for when we're running the queue - every few minutes, process some more towns
            m_callbackScheduler = callbackFactory.CreateScheduler<Queue<TownKey>>(ProcessQueue, TimeSpan.FromMinutes(1));
            // Scheduler for when we're waiting between maintenance periods - only needs to check every day or so
            m_longWaitScheduler = callbackFactory.CreateScheduler<bool>(RunMaintenanceAsync, TimeSpan.FromDays(1));

            m_botClient.Connected += async (o, args) => await RunMaintenanceAsync();
        }

        private async Task ProcessQueue(Queue<TownKey> towns)
        {
            if (m_queueProcessing)
                return;

            m_queueProcessing = true;
            Serilog.Log.Information("TownMaintenance: {townCount} towns remain...", towns.Count());

            int numSent = 0;
            while (numSent < NUM_TOWNS_PER_CALLBACK && towns.Count > 0)
            {
                var townKey = towns.Dequeue();

                Serilog.Log.Information("TownMaintenance: Processing town {townKey}", townKey);

                foreach (var task in m_startupTasks)
                {
                    try
                    {
                        await task(townKey);
                    }
                    catch (Exception)
                    {
                        // todo: something?
                    }
                }
                numSent++;
            }

            if (towns.Count > 0 && !m_shutdownRequested)
            {
                Serilog.Log.Information("TownMaintenance: {townCount} towns remain, scheduling callback...", towns.Count);
                DateTime nextTime = m_dateTime.Now + TimeSpan.FromMinutes(MINUTES_PER_CALLBACK);
                m_callbackScheduler.ScheduleCallback(towns, nextTime);
            }
            else
            {
                DateTime nextTime = m_dateTime.Now + TimeSpan.FromDays(DAYS_BETWEEN_MAINTENANCE);
                Serilog.Log.Information("TownMaintenance: maintenance complete! Next maintenance will be {time}", nextTime);
                m_longWaitScheduler.ScheduleCallback(false, nextTime);


            }

            if(m_shutdownRequested)
                m_shutdownTcs.TrySetResult();
            m_queueProcessing = false;
        }

        public void AddMaintenanceTask(Func<TownKey, Task> startupTask)
        {
            m_startupTasks.Add(startupTask);
        }

        private async Task RunMaintenanceAsync(bool initial=true)
        {
            var allTowns = new Queue<TownKey>(await m_townDatabase.GetAllTowns());

            await ProcessQueue(allTowns);
        }
    }
}
