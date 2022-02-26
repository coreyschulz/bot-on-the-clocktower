﻿using Bot.Api;
using Bot.Api.Database;
using Bot.Core;
using Bot.Core.Callbacks;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Bot.Base;
using Xunit;

namespace Test.Bot.Core
{
    public class GameTestBase : TestBase
    {
        protected const ulong MockGuildId = 1337;
        protected const ulong MockControlChannelId = 42;
        protected static TownKey MockTownKey = new TownKey(MockGuildId, MockControlChannelId);
        protected const string StorytellerDisplayName = "Peter Storyteller";

        protected readonly Mock<ICallbackScheduler<TownKey>> TownKeyCallbackSchedulerMock = new(MockBehavior.Strict);
        protected readonly Mock<ICallbackSchedulerFactory> CallbackSchedulerFactoryMock = new(MockBehavior.Strict);

        protected readonly Mock<IBotSystem> BotSystemMock = new();
        protected readonly Mock<IBotWebhookBuilder> WebhookBuilderMock = new();

        protected readonly Mock<IGuild> GuildMock = new();
        protected readonly Mock<ITownDatabase> TownLookupMock = new();
        protected readonly Mock<IDateTime> DateTimeMock = new();
        protected readonly Mock<IGameActivityDatabase> GameActivityDatabaseMock = new();
        protected readonly Mock<ITown> TownMock = new();
        protected readonly Mock<ITownRecord> TownRecordMock = new();
        protected readonly Mock<IBotClient> ClientMock = new();

        protected readonly Mock<IChannel> ControlChannelMock = new();
        protected readonly Mock<IChannel> TownSquareMock = new();
        protected readonly Mock<IChannel> DarkAlleyMock = new();
        protected readonly Mock<IChannelCategory> DayCategoryMock = new();
        protected readonly Mock<IChannelCategory> NightCategoryMock = new();
        protected readonly Mock<IChannel> ChatChannelMock = new();

        protected readonly Mock<IRole> StorytellerRoleMock = new();
        protected readonly Mock<IRole> VillagerRoleMock = new();

        protected readonly Mock<IChannel> Cottage1Mock = new();
        protected readonly Mock<IChannel> Cottage2Mock = new();
        protected readonly Mock<IChannel> Cottage3Mock = new();
        protected readonly Mock<IChannel> Cottage4Mock = new();

        protected readonly Mock<IMember> InteractionAuthorMock = new();
        protected readonly Mock<IMember> Villager1Mock = new();
        protected readonly Mock<IMember> Villager2Mock = new();
        protected readonly Mock<IMember> Villager3Mock = new();

        protected readonly Mock<IBotInteractionContext> InteractionContextMock = new();
        protected readonly Mock<IActiveGameService> ActiveGameServiceMock = new();
        protected readonly Mock<IProcessLogger> ProcessLoggerMock = new();
        protected readonly Mock<IComponentService> ComponentServiceMock = new();
        protected readonly Mock<IShuffleService> ShuffleServiceMock = new();

        public GameTestBase()
        {
            RegisterMock(CallbackSchedulerFactoryMock);
            CallbackSchedulerFactoryMock.Setup(csf => csf.CreateScheduler(It.IsAny<Func<TownKey, Task>>(), It.IsAny<TimeSpan>())).Returns(TownKeyCallbackSchedulerMock.Object);

            TownKeyCallbackSchedulerMock.Setup(cs => cs.ScheduleCallback(It.IsAny<TownKey>(), It.IsAny<DateTime>()));

            Mock<IBotWebhookBuilder> builderMock = new();
            BotSystemMock.Setup(c => c.CreateWebhookBuilder()).Returns(WebhookBuilderMock.Object);

            // WithContent returns the mock again so you can chain calls
            WebhookBuilderMock.Setup(c => c.WithContent(It.IsAny<string>())).Returns(WebhookBuilderMock.Object);
            WebhookBuilderMock.Setup(c => c.AddComponents(It.IsAny<IBotComponent[]>())).Returns(WebhookBuilderMock.Object);

            RegisterMock(BotSystemMock);
            RegisterMock(ClientMock);
            RegisterMock(TownLookupMock);
            RegisterMock(DateTimeMock);
            RegisterMock(GameActivityDatabaseMock);
            RegisterMock(ActiveGameServiceMock);
            RegisterMock(ComponentServiceMock);
            RegisterMock(ShuffleServiceMock);

            ShuffleServiceMock.Setup(ss => ss.Shuffle(It.IsAny<IEnumerable<Tuple<IChannel, IMember>>>()))
                .Returns((IEnumerable<Tuple<IChannel, IMember>> input) => input.Reverse());
            ShuffleServiceMock.Setup(ss => ss.Shuffle(It.IsAny<IEnumerable<IMember>>()))
                .Returns((IEnumerable<IMember> input) => input.Reverse());

            // TownLookup expects MockGuildId and MockChannelId and returns the TownRecord
            TownLookupMock.Setup(tl => tl.GetTownRecord(It.Is<ulong>(gid => gid == MockGuildId), It.Is<ulong>(cid => cid == MockControlChannelId))).ReturnsAsync(TownRecordMock.Object);
            TownLookupMock.Setup(tl => tl.GetTownRecords(It.Is<ulong>(a => a == MockGuildId))).ReturnsAsync(new[] { TownRecordMock.Object });

            // ResolveTown expects the TownRecord and returns the Town
            ClientMock.Setup(c => c.ResolveTownAsync(It.Is<ITownRecord>(tr => tr == TownRecordMock.Object))).ReturnsAsync(TownMock.Object);
            ClientMock.Setup(c => c.GetGuild(It.Is<ulong>(x => x == MockGuildId))).ReturnsAsync(GuildMock.Object);

            // By default, the ActiveGameService won't find a game for this context
            IGame? defaultGame = null;
            ActiveGameServiceMock.Setup(a => a.TryGetGame(It.IsAny<TownKey>(), out defaultGame)).Returns(false);

            ProcessLoggerMock.Setup(c => c.LogException(It.IsAny<Exception>(), It.IsAny<string>()));

            var memberList = new Dictionary<ulong, IMember>();
            memberList[101] = InteractionAuthorMock.Object;
            memberList[102] = Villager1Mock.Object;
            memberList[103] = Villager2Mock.Object;
            memberList[104] = Villager3Mock.Object;

            GuildMock.SetupGet(x => x.Id).Returns(MockGuildId);
            GuildMock.SetupGet(x => x.Members).Returns(memberList);
            ControlChannelMock.SetupGet(x => x.Id).Returns(MockControlChannelId);

            InteractionContextMock.SetupGet(x => x.Guild).Returns(GuildMock.Object);
            InteractionContextMock.SetupGet(x => x.Channel).Returns(ControlChannelMock.Object);
            InteractionContextMock.SetupGet(x => x.Member).Returns(InteractionAuthorMock.Object);

            TownMock.SetupGet(t => t.Guild).Returns(GuildMock.Object);
            TownMock.SetupGet(t => t.ControlChannel).Returns(ControlChannelMock.Object);
            TownMock.SetupGet(t => t.TownSquare).Returns(TownSquareMock.Object);
            TownMock.SetupGet(t => t.DayCategory).Returns(DayCategoryMock.Object);
            TownMock.SetupGet(t => t.NightCategory).Returns(NightCategoryMock.Object);
            TownMock.SetupGet(t => t.ChatChannel).Returns(ChatChannelMock.Object);
            TownMock.SetupGet(t => t.StorytellerRole).Returns(StorytellerRoleMock.Object);
            TownMock.SetupGet(t => t.VillagerRole).Returns(VillagerRoleMock.Object);
            TownMock.SetupGet(t => t.TownRecord).Returns(TownRecordMock.Object);

            var villagerRoleName = "BotC Villager Mock";
            var storytellerRoleName = "BotC Storyteller Mock";
            var controlChannelName = "botc_mover_mock";
            var chatChannelName = "chat_mock";
            var townSquareName = "Town Square Mock";
            var darkAlleyName = "Dark Alley Mock";

            VillagerRoleMock.SetupGet(r => r.Name).Returns(villagerRoleName);
            VillagerRoleMock.SetupGet(r => r.Mention).Returns($"@{villagerRoleName}");
            VillagerRoleMock.Name = $"Role: {villagerRoleName}";
            StorytellerRoleMock.Name = $"Role: {storytellerRoleName}";

            SetupChannelMock(ControlChannelMock, controlChannelName, false);
            SetupChannelMock(ChatChannelMock, chatChannelName, false);


            SetupChannelMock(TownSquareMock, townSquareName);
            TownSquareMock.SetupGet(t => t.Users).Returns(new[] { InteractionAuthorMock.Object, Villager1Mock.Object, Villager2Mock.Object, Villager3Mock.Object });
            SetupChannelMock(DarkAlleyMock, darkAlleyName);

            DayCategoryMock.SetupGet(c => c.Channels).Returns(new[] { ControlChannelMock.Object, ChatChannelMock.Object, TownSquareMock.Object, DarkAlleyMock.Object });
            
            SetupChannelMock(Cottage1Mock, "Cottage 1 Mock");
            SetupChannelMock(Cottage2Mock, "Cottage 2 Mock");
            SetupChannelMock(Cottage3Mock, "Cottage 3 Mock");
            SetupChannelMock(Cottage4Mock, "Cottage 4 Mock");
            Cottage1Mock.SetupGet(x => x.Position).Returns(1);
            Cottage2Mock.SetupGet(x => x.Position).Returns(2);
            Cottage3Mock.SetupGet(x => x.Position).Returns(3);
            Cottage4Mock.SetupGet(x => x.Position).Returns(4);
            // Purposely don't order the collection of cottages in their display order
            NightCategoryMock.SetupGet(c => c.Channels).Returns(new[] { Cottage1Mock.Object, Cottage3Mock.Object, Cottage4Mock.Object, Cottage2Mock.Object});

            TownRecordMock.SetupGet(tr => tr.GuildId).Returns(MockGuildId);
            TownRecordMock.SetupGet(tr => tr.ControlChannelId).Returns(MockControlChannelId);
            TownRecordMock.SetupGet(tr => tr.VillagerRole).Returns(villagerRoleName);
            TownRecordMock.SetupGet(tr => tr.ControlChannel).Returns(controlChannelName);
            TownRecordMock.SetupGet(tr => tr.TownSquare).Returns(townSquareName);

            SetupUserMock(InteractionAuthorMock, StorytellerDisplayName);
            SetupUserMock(Villager1Mock, "Bob");
            SetupUserMock(Villager2Mock, "Alice");
            SetupUserMock(Villager3Mock, "Carl");
        }

        protected static void SetupChannelMock(Mock<IChannel> channel, string name, bool isVoice=true)
        {
            channel.Name = name;
            channel.SetupGet(c => c.Users).Returns(Array.Empty<IMember>());
            channel.SetupGet(c => c.Name).Returns(name);
            channel.SetupGet(c => c.IsVoice).Returns(isVoice);
        }

        protected static void SetupUserMock(Mock<IMember> member, string name)
		{
            member.Name = name;
            member.SetupGet(x => x.DisplayName).Returns(name);
            member.SetupGet(x => x.Roles).Returns(Array.Empty<IRole>());
        }

        protected Mock<IGame> MockGameInProgress()
        {
            var gameMock = CreateGameMock();

            var gameObject = gameMock.Object;
            ActiveGameServiceMock.Setup(ags => ags.TryGetGame(It.IsAny<TownKey>(), out gameObject)).Returns(true);

            return gameMock;
        }

        protected Mock<IGame> CreateGameMock()
        {
            Mock<IGame> gameMock = new();
            gameMock.SetupGet(g => g.TownKey).Returns(MockTownKey);

            // Actually back the Game mock with lists for storytellers and villagers :/
            var storytellers = new List<IMember>(new[] { InteractionAuthorMock.Object });
            var villagers = new List<IMember>(new[] { Villager1Mock.Object, Villager2Mock.Object, Villager3Mock.Object });
            gameMock.SetupGet(g => g.AllPlayers).Returns(() => storytellers.Concat(villagers).ToList());
            gameMock.SetupGet(g => g.Storytellers).Returns(() => storytellers);
            gameMock.SetupGet(g => g.Villagers).Returns(() => villagers);
            gameMock.Setup(g => g.AddStoryteller(It.IsAny<IMember>())).Callback<IMember>((m) => storytellers.Add(m));
            gameMock.Setup(g => g.RemoveStoryteller(It.IsAny<IMember>())).Callback<IMember>((m) => storytellers.Remove(m));
            gameMock.Setup(g => g.AddVillager(It.IsAny<IMember>())).Callback<IMember>((m) => villagers.Add(m));
            gameMock.Setup(g => g.RemoveVillager(It.IsAny<IMember>())).Callback<IMember>((m) => villagers.Remove(m));

            InteractionAuthorMock.SetupGet(m => m.DisplayName).Returns(MemberHelper.StorytellerTag + StorytellerDisplayName);
            InteractionAuthorMock.SetupGet(m => m.Roles).Returns(new[] { StorytellerRoleMock.Object });
            Villager1Mock.SetupGet(m => m.Roles).Returns(new[] { VillagerRoleMock.Object });
            Villager2Mock.SetupGet(m => m.Roles).Returns(new[] { VillagerRoleMock.Object });
            Villager3Mock.SetupGet(m => m.Roles).Returns(new[] { VillagerRoleMock.Object });

            return gameMock;
        }


        // Common assumptions we want to make for most of our Interactions
        protected void VerifyContext()
        {
            // Most interactions should Defer to allow for latency
            InteractionContextMock.Verify(c => c.DeferInteractionResponse(), Times.Once);
        }

        protected BotGameplayInteractionHandler CreateGameplayInteractionHandler()
        {
            return new(GetServiceProvider(), new BotGameplay(GetServiceProvider()), new BotVoteTimer(GetServiceProvider()));
        }

        protected void RunCurrentGameAssertComplete()
        {
            BotGameplay gs = new(GetServiceProvider());
            var t = gs.CurrentGameAsync(InteractionContextMock.Object, ProcessLoggerMock.Object);
            t.Wait(50);
            Assert.True(t.IsCompleted);
        }
    }
}
