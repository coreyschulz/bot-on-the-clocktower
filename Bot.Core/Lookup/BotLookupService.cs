﻿using Bot.Api;
using Bot.Api.Database;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Bot.Core.Lookup
{
    public class BotLookupService : IBotLookupService
    {
        private readonly IGuildInteractionQueue m_interactionQueue;
        private readonly IGuildInteractionErrorHandler m_errorHandler;
        private readonly ILookupRoleDatabase m_lookupDb;

        public BotLookupService(IServiceProvider serviceProvider)
        {
            serviceProvider.Inject(out m_interactionQueue);
            serviceProvider.Inject(out m_errorHandler);
            serviceProvider.Inject(out m_lookupDb);
        }

        public Task LookupAsync(IBotInteractionContext ctx, string lookupString) => m_interactionQueue.QueueInteractionAsync($"Looking up \"{lookupString}\"...", ctx, () => PerformLookupAsync(ctx, lookupString));
        public Task AddScriptAsync(IBotInteractionContext ctx, string scriptJsonUrl) => m_interactionQueue.QueueInteractionAsync($"Adding script at \"{scriptJsonUrl}\"...", ctx, () => PerformAddScriptAsync(ctx, scriptJsonUrl));
        public Task RemoveScriptAsync(IBotInteractionContext ctx, string scriptJsonUrl) => m_interactionQueue.QueueInteractionAsync($"Removing script at \"{scriptJsonUrl}\"...", ctx, () => PerformRemoveScriptAsync(ctx, scriptJsonUrl));
        public Task ListScriptsAsync(IBotInteractionContext ctx) => m_interactionQueue.QueueInteractionAsync($"Finding registered scripts...", ctx, () => PerformListScriptsAsync(ctx));

        private async Task<QueuedInteractionResult> PerformLookupAsync(IBotInteractionContext ctx, string lookupString)
        {
            string result = await m_errorHandler.TryProcessReportingErrorsAsync(ctx.Guild.Id, ctx.Member, l =>
            {
                throw new NotImplementedException();
            });
            return new QueuedInteractionResult(result);
        }

        private async Task<QueuedInteractionResult> PerformAddScriptAsync(IBotInteractionContext ctx, string scriptJsonUrl)
        {
            string result = await m_errorHandler.TryProcessReportingErrorsAsync(ctx.Guild.Id, ctx.Member, async l =>
            {
                await m_lookupDb.AddScriptUrlAsync(ctx.Guild.Id, scriptJsonUrl);
                return $"Script \"{scriptJsonUrl}\" added to lookups for this server.";
            });
            return new QueuedInteractionResult(result);
        }

        private async Task<QueuedInteractionResult> PerformRemoveScriptAsync(IBotInteractionContext ctx, string scriptJsonUrl)
        {
            string result = await m_errorHandler.TryProcessReportingErrorsAsync(ctx.Guild.Id, ctx.Member, async l =>
            {
                await m_lookupDb.RemoveScriptUrlAsync(ctx.Guild.Id, scriptJsonUrl);
                return $"Script \"{scriptJsonUrl}\" removed from lookups for this server.";
            });
            return new QueuedInteractionResult(result);
        }

        private async Task<QueuedInteractionResult> PerformListScriptsAsync(IBotInteractionContext ctx)
        {
            string result = await m_errorHandler.TryProcessReportingErrorsAsync(ctx.Guild.Id, ctx.Member, async l =>
            {
                var scripts = await m_lookupDb.GetScriptUrlsAsync(ctx.Guild.Id);
                if (scripts.Count == 0)
                    return "No custom scripts have been added to this server.";

                var sb = new StringBuilder();
                sb.AppendLine("The following custom scripts were found for this server:");
                foreach (var script in scripts)
                    sb.AppendLine($"⦁ {script}");
                return sb.ToString();
            });
            return new QueuedInteractionResult(result);
        }
    }
}
