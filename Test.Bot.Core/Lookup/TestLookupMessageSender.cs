﻿using Bot.Api;
using Bot.Core.Lookup;
using Moq;
using System.Linq;
using System.Threading.Tasks;
using Test.Bot.Base;
using Xunit;

namespace Test.Bot.Core.Lookup
{
    public class TestLookupMessageSender : TestBase
    {
        private readonly Mock<IChannel> m_mockChannel = new();

        public TestLookupMessageSender()
        {
            m_mockChannel.Setup(c => c.SendMessageAsync(It.IsAny<string>())).Returns(Task.FromResult(new Mock<IMessage>().Object));
        }

        // NOTE: Expected this will change to an embed eventually

        [Fact]
        public void PassedCharacter_SendsMessageWithRequiredCharacterFields()
        {
            string charName = "Character Name";
            string charAbility = "Character Ability";
            var c = new CharacterData(charName, charAbility, CharacterTeam.Outsider, isOfficial: false);

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(c, Enumerable.Empty<ScriptData>())));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(charName))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(charAbility))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Outsider"))), Times.Once);
            VerifyNothingBadSent();
        }

        [Fact]
        public void PassedCharacter_SendsMessageWithFlavorText()
        {
            string flavorText = "Flavor Text";
            var c = CreateBasicCharacter();
            c.FlavorText = flavorText;

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(c, Enumerable.Empty<ScriptData>())));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(flavorText))), Times.Once);
            VerifyNothingBadSent();
        }

        [Fact(Skip ="Regular messages do not have images")]
        public void PassedCharacter_SendsMessageWithImage()
        {
            // TODO
        }

        [Fact]
        public void PassedCharacter_OfficialCharacterSendsWikiLink()
        {
            string charName = "the character name";
            var c = new CharacterData(charName, "some ability", CharacterTeam.Minion, isOfficial: true);

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(c, Enumerable.Empty<ScriptData>())));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(OfficialWikiHelper.WikiPrefixUrl + "The_Character_Name"))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Official"))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Minion"))), Times.Once);
            VerifyNothingBadSent();
        }

        [Fact]
        public void PassedCharacter_UnofficialCharacterSendsNoWikiLink()
        {
            var c = new CharacterData("the character name", "some ability", CharacterTeam.Demon, isOfficial: false);

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(c, Enumerable.Empty<ScriptData>())));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(OfficialWikiHelper.WikiPrefixUrl))), Times.Never);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Official"))), Times.Never);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Demon"))), Times.Once);
            VerifyNothingBadSent();
        }

        [Fact]
        public void PassedScripts_SendsScriptNames()
        {
            var script1 = new ScriptData("Script 1", isOfficial: false);
            var script2 = new ScriptData("Script 2", isOfficial: false);

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(CreateBasicCharacter(), new[] { script1, script2 })));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Found in", System.StringComparison.InvariantCultureIgnoreCase))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(script1.Name))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(script2.Name))), Times.Once);
        }

        [Fact]
        public void PassedOfficialScript_LinksToWiki()
        {
            var officialScript = new ScriptData("Trouble Brewing", isOfficial: true);

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(CreateBasicCharacter(), new[] { officialScript })));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Official", System.StringComparison.InvariantCultureIgnoreCase))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains(OfficialWikiHelper.WikiPrefixUrl + "Trouble_Brewing"))), Times.Once);
            VerifyNothingBadSent();
        }

        [Fact]
        public void PassedNoScript_NoFoundIn()
        {
            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(CreateBasicCharacter(), Enumerable.Empty<ScriptData>())));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("Found in", System.StringComparison.InvariantCultureIgnoreCase))), Times.Never);
            VerifyNothingBadSent();
        }

        [Fact]
        public void ScriptAuthorsProvided_OutputsAuthor()
        {
            var script1 = new ScriptData("Script 1", isOfficial: true);
            string author1 = "The Pandemonium Institute";
            script1.Author = author1;

            var script2 = new ScriptData("Script 2", isOfficial: false);
            string author2 = "some person";
            script2.Author = author2;            

            var ms = new LookupMessageSender(GetServiceProvider());
            AssertCompletedTask(() => ms.SendLookupMessageAsync(m_mockChannel.Object, new LookupCharacterItem(CreateBasicCharacter(), new[] { script1, script2 })));

            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains($"by {script1.Author}", System.StringComparison.InvariantCultureIgnoreCase))), Times.Once);
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains($"by {script2.Author}", System.StringComparison.InvariantCultureIgnoreCase))), Times.Once);
        }

        [Fact(Skip ="Waiting on embed branch")]
        public void CustomScriptWithAlmanac_LinkToAlmanac()
        {
            // TODO: Should have 1 links - to the almanac as a whole
        }        

        [Fact(Skip ="Waiting on embed branch")]
        public void CustomScriptWithBloodstarAlmanac_LinkToAlmanacAndCharacter()
        {
            // TODO: Should have 2 links - to the almanac as a whole, and another directly to the character in the almanac
        }

        private void VerifyNothingBadSent()
        {
            m_mockChannel.Verify(c => c.SendMessageAsync(It.Is<string>(s => s.Contains("\r\n\r\n"))), Times.Never); // null check
            m_mockChannel.Verify(c => c.SendMessageAsync(It.IsAny<string>()), Times.Once); // sending 2 messages
        }

        private static CharacterData CreateBasicCharacter() => new("charname", "charAbility", CharacterTeam.Townsfolk, isOfficial: false);
    }
}
