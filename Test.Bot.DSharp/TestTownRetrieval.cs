﻿using Bot.Api;
using Bot.Api.Database;
using Bot.Core;
using Moq;
using System.Collections.Generic;
using Test.Bot.Base;
using Xunit;

namespace Test.Bot.DSharp
{
    public class TestTownRetrieval : TestBase
    {
        private readonly Mock<IBotClient> m_mockClient = new(MockBehavior.Strict);

        private readonly Mock<IChannel> m_mockControlChannel = new(MockBehavior.Strict);
        private readonly Mock<IChannel> m_mockTownSquareChannel = new(MockBehavior.Strict);
        private readonly Mock<IChannel> m_mockChatChannel = new(MockBehavior.Strict);
        private readonly Mock<IChannelCategory> m_mockDayChannelCategory = new(MockBehavior.Strict);
        private readonly Mock<IChannelCategory> m_mockNightChannelCategory = new(MockBehavior.Strict);
        private readonly Mock<ITownRecord> m_mockTownRecord = new(MockBehavior.Strict);
        private readonly Mock<ITownDatabase> m_mockTownDb = new(MockBehavior.Strict);
        private readonly Mock<IGuild> m_mockGuild = new(MockBehavior.Strict);
        private readonly Mock<IRole> m_mockStorytellerRole = new(MockBehavior.Strict);
        private readonly Mock<IRole> m_mockVillagerRole = new(MockBehavior.Strict);

        private readonly Dictionary<ulong, IRole> m_roleDict = new();

        private const string MismatchedName = "mismatched name";
        private const string ControlName = "control chan";
        private const string TownSquareName = "TS chan";
        private const string ChatName = "chat chan";
        private const string DayCategoryName = "day cat";
        private const string NightCategoryName = "night cat";
        private const string StorytellerRoleName = "storyteller role";
        private const string VillagerRoleName = "villager role";

        private const ulong MismatchedId = 0;
        private const ulong ControlId = 1;
        private const ulong TownSquareId = 2;
        private const ulong ChatId = 3;
        private const ulong DayCategoryId = 4;
        private const ulong NightCategoryId = 5;
        private const ulong StorytellerRoleId = 6;
        private const ulong VillagerRoleId = 7;
        private const ulong GuildId = 77;

        public TestTownRetrieval()
        {
            RegisterMock(m_mockClient);
            RegisterMock(m_mockTownDb);

            var env = RegisterMock(new Mock<IEnvironment>());
            env.Setup(e => e.GetEnvironmentVariable(It.IsAny<string>())).Returns("env var");

            SetupChannelMock(m_mockGuild, m_mockControlChannel, ControlId, ControlName, false);
            SetupChannelMock(m_mockGuild, m_mockTownSquareChannel, TownSquareId, TownSquareName, true);
            SetupChannelMock(m_mockGuild, m_mockChatChannel, ChatId, ChatName, false);
            SetupChannelCategoryMock(m_mockGuild, m_mockDayChannelCategory, DayCategoryId, DayCategoryName);
            SetupChannelCategoryMock(m_mockGuild, m_mockNightChannelCategory, NightCategoryId, NightCategoryName);
            SetupRoleMock(m_roleDict, m_mockStorytellerRole, StorytellerRoleId, StorytellerRoleName);
            SetupRoleMock(m_roleDict, m_mockVillagerRole, VillagerRoleId, VillagerRoleName);

            m_mockTownRecord.SetupGet(tr => tr.ControlChannel).Returns(ControlName);
            m_mockTownRecord.SetupGet(tr => tr.ControlChannelId).Returns(ControlId);
            m_mockTownRecord.SetupGet(tr => tr.TownSquare).Returns(TownSquareName);
            m_mockTownRecord.SetupGet(tr => tr.TownSquareId).Returns(TownSquareId);
            m_mockTownRecord.SetupGet(tr => tr.ChatChannel).Returns(ChatName);
            m_mockTownRecord.SetupGet(tr => tr.ChatChannelId).Returns(ChatId);
            m_mockTownRecord.SetupGet(tr => tr.DayCategory).Returns(DayCategoryName);
            m_mockTownRecord.SetupGet(tr => tr.DayCategoryId).Returns(DayCategoryId);
            m_mockTownRecord.SetupGet(tr => tr.NightCategory).Returns(NightCategoryName);
            m_mockTownRecord.SetupGet(tr => tr.NightCategoryId).Returns(NightCategoryId);
            m_mockTownRecord.SetupGet(tr => tr.StorytellerRole).Returns(StorytellerRoleName);
            m_mockTownRecord.SetupGet(tr => tr.StorytellerRoleId).Returns(StorytellerRoleId);
            m_mockTownRecord.SetupGet(tr => tr.VillagerRole).Returns(VillagerRoleName);
            m_mockTownRecord.SetupGet(tr => tr.VillagerRoleId).Returns(VillagerRoleId);
            m_mockTownRecord.SetupGet(tr => tr.GuildId).Returns(GuildId);

            static void SetupChannelMock(Mock<IGuild> guildMock, Mock<IChannel> channelMock, ulong channelId, string channelName, bool expectedVoice)
            {
                channelMock.SetupGet(c => c.Id).Returns(channelId);
                channelMock.SetupGet(c => c.Name).Returns(channelName);
                channelMock.SetupGet(c => c.IsVoice).Returns(expectedVoice);
                guildMock.Setup(c => c.GetChannel(It.Is<ulong>(id => id == channelId))).Returns(channelMock.Object);
            }

            static void SetupChannelCategoryMock(Mock<IGuild> guildMock, Mock<IChannelCategory> channelMock, ulong channelId, string channelName)
            {
                channelMock.SetupGet(c => c.Id).Returns(channelId);
                channelMock.SetupGet(c => c.Name).Returns(channelName);
                guildMock.Setup(c => c.GetChannelCategory(It.Is<ulong>(id => id == channelId))).Returns(channelMock.Object);
            }

            static void SetupRoleMock(IDictionary<ulong, IRole> roleDict, Mock<IRole> roleMock, ulong roleId, string roleName)
            {
                roleMock.SetupGet(r => r.Name).Returns(roleName);
                roleMock.SetupGet(r => r.Id).Returns(roleId);
                roleDict.Add(roleId, roleMock.Object);
            }

            m_mockClient.Setup(c => c.GetGuildAsync(It.Is<ulong>(id => id == GuildId))).ReturnsAsync(m_mockGuild.Object);
            m_mockGuild.SetupGet(g => g.Id).Returns(GuildId);
            m_mockGuild.SetupGet(g => g.Roles).Returns(m_roleDict);

            IChannel[] channels = new[] { m_mockChatChannel.Object, m_mockControlChannel.Object, m_mockTownSquareChannel.Object };
            IChannelCategory[] channelCategories = new[] { m_mockDayChannelCategory.Object, m_mockNightChannelCategory.Object };
            m_mockDayChannelCategory.SetupGet(cc => cc.Channels).Returns(channels);
            m_mockGuild.SetupGet(g => g.ChannelCategories).Returns(channelCategories);

            m_mockTownDb.Setup(db => db.UpdateTownAsync(It.IsAny<ITown>())).ReturnsAsync(true);
        }

        [Fact]
        public void TownResolve_AllCorrect_NoRequestsUpdate()
        {
            TestResolve_VerifyTownNotUpdated();
        }

        [Fact]
        public void TownResolve_ChatNameOff_RequestsUpdate() => TestResolve_ChannelNameOff(m_mockChatChannel);
        [Fact]
        public void TownResolve_TownSquareNameOff_RequestsUpdate() => TestResolve_ChannelNameOff(m_mockTownSquareChannel);
        [Fact]
        public void TownResolve_ControlNameOff_RequestsUpdate() => TestResolve_ChannelNameOff(m_mockControlChannel);
        [Fact]
        public void TownResolve_DayCategoryNameOff_RequestsUpdate() => TestResolve_ChannelCategoryNameOff(m_mockDayChannelCategory);
        [Fact]
        public void TownResolve_NightCategoryNameOff_RequestsUpdate() => TestResolve_ChannelCategoryNameOff(m_mockNightChannelCategory);

        [Fact]
        public void TownResolve_StorytellerRoleNameOff_RequestsUpdate() => TestResolve_RoleNameOff(m_mockStorytellerRole);
        [Fact]
        public void TownResolve_VillagerRoleNameOff_RequestsUpdate() => TestResolve_RoleNameOff(m_mockVillagerRole);

        private void TestResolve_ChannelNameOff(Mock<IChannel> channel)
        {
            channel.SetupGet(c => c.Name).Returns(MismatchedName);
            TestResolve_VerifyTownUpdated();
        }

        private void TestResolve_ChannelCategoryNameOff(Mock<IChannelCategory> channelCategory)
        {
            channelCategory.SetupGet(c => c.Name).Returns(MismatchedName);
            TestResolve_VerifyTownUpdated();
        }
        
        private void TestResolve_RoleNameOff(Mock<IRole> role)
        {
            role.SetupGet(c => c.Name).Returns(MismatchedName);
            TestResolve_VerifyTownUpdated();
        }

        [Fact]
        public void TownResolve_ChatIdOff_RequestsUpdate() => TestResolve_ChannelIdOff(ChatId, m_mockChatChannel);
        [Fact]
        public void TownResolve_TownSquareIdOff_RequestsUpdate() => TestResolve_ChannelIdOff(TownSquareId, m_mockTownSquareChannel);
        [Fact]
        public void TownResolve_ControlIdOff_RequestsUpdate() => TestResolve_ChannelIdOff(ControlId, m_mockControlChannel);
        [Fact]
        public void TownResolve_DayCategoryIdOff_RequestsUpdate() => TestResolve_ChannelCategoryIdOff(DayCategoryId, m_mockDayChannelCategory);
        [Fact]
        public void TownResolve_NightCategoryIdOff_RequestsUpdate() => TestResolve_ChannelCategoryIdOff(NightCategoryId, m_mockNightChannelCategory);

        [Fact]
        public void TownResolve_StorytellerIdOff_RequestsUpdate() => TestResolve_RoleIdOff(m_mockStorytellerRole);
        [Fact]
        public void TownResolve_VillagerIdOff_RequestsUpdate() => TestResolve_RoleIdOff(m_mockVillagerRole);

        private void TestResolve_ChannelIdOff(ulong originalId, Mock<IChannel> channel)
        {
            m_mockGuild.Setup(g => g.GetChannel(It.Is<ulong>(l => l == originalId))).Returns((IChannel?)null);
            channel.SetupGet(c => c.Id).Returns(MismatchedId);
            TestResolve_VerifyTownUpdated();
        }

        private void TestResolve_ChannelCategoryIdOff(ulong originalId, Mock<IChannelCategory> channelCategory)
        {
            m_mockGuild.Setup(g => g.GetChannelCategory(It.Is<ulong>(l => l == originalId))).Returns((IChannelCategory?)null);
            channelCategory.SetupGet(c => c.Id).Returns(MismatchedId);
            TestResolve_VerifyTownUpdated();
        }

        private void TestResolve_RoleIdOff(Mock<IRole> roleMock)
        {
            ulong originalId = roleMock.Object.Id;
            m_roleDict.Remove(originalId);
            m_roleDict.Add(MismatchedId, roleMock.Object);
            roleMock.SetupGet(r => r.Id).Returns(MismatchedId);
            TestResolve_VerifyTownUpdated();
        }

        private void TestResolve_VerifyTownUpdated() => TestResolve_VerifyTownUpdatedTimes(Times.Once());
        private void TestResolve_VerifyTownNotUpdated() => TestResolve_VerifyTownUpdatedTimes(Times.Never());

        [Fact]
        public void TestResolve_ChatVoiceMismatch_NullChannel()
        {
            m_mockChatChannel.SetupGet(c => c.IsVoice).Returns(true);
            var town = PerformResolve();
            Assert.NotNull(town);
            Assert.Null(town!.ChatChannel);
        }

        [Fact]
        public void TestResolve_ControlVoiceMismatch_NullChannel()
        {
            m_mockControlChannel.SetupGet(c => c.IsVoice).Returns(true);
            var town = PerformResolve();
            Assert.NotNull(town);
            Assert.Null(town!.ControlChannel);
        }

        [Fact]
        public void TestResolve_TownSquareVoiceMismatch_NullChannel()
        {
            m_mockTownSquareChannel.SetupGet(c => c.IsVoice).Returns(false);
            var town = PerformResolve();
            Assert.NotNull(town);
            Assert.Null(town!.TownSquare);
        }

        private void TestResolve_VerifyTownUpdatedTimes(Times numTimes)
        {
            PerformResolve();
            m_mockTownDb.Verify(db => db.UpdateTownAsync(It.Is<ITown>(t => UpdatedTownMatches(t))), numTimes);
        }

        private ITown? PerformResolve()
        {
            var tr = new TownResolver(GetServiceProvider());
            var resolveTask = tr.ResolveTownAsync(m_mockTownRecord.Object);
            resolveTask.Wait(50);
            Assert.True(resolveTask.IsCompleted);
            return resolveTask.Result;
        }

        private bool UpdatedTownMatches(ITown update)
        {
            return
                update.Guild == m_mockGuild.Object &&
                update.TownSquare == m_mockTownSquareChannel.Object &&
                update.ChatChannel == m_mockChatChannel.Object &&
                update.ControlChannel == m_mockControlChannel.Object &&
                update.DayCategory == m_mockDayChannelCategory.Object &&
                update.NightCategory == m_mockNightChannelCategory.Object &&
                update.StorytellerRole == m_mockStorytellerRole.Object &&
                update.VillagerRole == m_mockVillagerRole.Object;
        }
    }
}
