﻿using Bot.Api;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bot.Core
{
    public static class MemberHelper
    {
        public const string StorytellerTag = "(ST) ";

        public static async Task<bool> MoveToChannelLoggingErrorsAsync(IMember member, IChannel channel, IProcessLogger logger)
        {
            Serilog.Log.Debug("Moving {@user} to {@channel}", member, channel);
            try
            {
                if (!channel.Users.Contains(member))
                {
                    await member.MoveToChannelAsync(channel);
                }
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "MoveToChannel failed while moving {@user} to {@channel}", member, channel);
                if (!IsHandledException(ex))
                    throw;

                logger.LogException(ex, $"move {member.DisplayName} to channel {channel.Name}");
                return false;
            }
        }

        public static async Task<bool> GrantRoleLoggingErrorsAsync(IMember member, IRole role, IProcessLogger logger)
        {
            try
            {
                Serilog.Log.Debug("Granting {@role} to {@user} ", role, member);
                await member.GrantRoleAsync(role);
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "GrantRole failed while granting {@user} role {@role}", member, role);
                if (!IsHandledException(ex))
                    throw;

                logger.LogException(ex, $"grant role '{role.Name}' to {member.DisplayName}");
                return false;
            }
        }

        public static async Task<bool> RevokeRoleLoggingErrorsAsync(IMember member, IRole role, IProcessLogger logger)
        {
            try
            {
                Serilog.Log.Debug("Revoking {@role} from {@user} ", role, member);
                await member.RevokeRoleAsync(role);
                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "RevokeRole failed while revoking {@user} role {@role}", member, role);
                if (!IsHandledException(ex))
                    throw;

                logger.LogException(ex, $"revoke role '{role.Name}' from {member.DisplayName}");
                return false;
            }
        }

        public static string DisplayName(IMember m)
        {
            if (m.DisplayName.StartsWith(StorytellerTag))
                return m.DisplayName[StorytellerTag.Length..];
            else
                return m.DisplayName;
        }
        public static async Task<bool> AddStorytellerTag(IMember member, IProcessLogger logger)
        {
            Serilog.Log.Debug("AddStorytellerTag for member {@member}", member);

            if (member.DisplayName.StartsWith(StorytellerTag))
                return true;

            try
            {
                await member.SetDisplayName(StorytellerTag + member.DisplayName);
                return true;
            }
            catch(UnauthorizedException)
            {
                // Don't spam the server owner, just let this succeed without adding the (ST) tag
                return true;
            }
            catch(Exception ex)
            {
                Serilog.Log.Error(ex, "AddStorytellerTag failed while changing display name of {@member}", member);
                if (!IsHandledException(ex))
                    throw;

                logger.LogException(ex, $"change display name of '{member.DisplayName}'");
                return false;
            }
        }

        public static async Task<bool> RemoveStorytellerTagAsync(IMember member, IProcessLogger logger)
        {
            Serilog.Log.Debug("RemoveStorytellerTag for member {@member}", member);
            if (!member.DisplayName.StartsWith(StorytellerTag))
                return true;

            try
            {
                await member.SetDisplayName(member.DisplayName[StorytellerTag.Length..]);
                return true;
            }
            catch(Exception ex)
            {
                Serilog.Log.Error(ex, "RemoveStorytellerTag failed while changing display name of {@member}", member);
                if (!IsHandledException(ex))
                    throw;

                logger.LogException(ex, $"change display name of '{member.DisplayName}'");
                return false;
            }
        }

     
        private static bool IsHandledException(Exception ex)
        {
            return (ex is UnauthorizedException || ex is NotFoundException || ex is ServerErrorException);
        }
    }
}
