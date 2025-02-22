# Gameplay Command Details

## `/game`

Primary game flow command - will give you convenient buttons for managing the Day / Night cycle as well as bringing villagers back to vote.

![image](https://user-images.githubusercontent.com/151635/162875175-1511caa7-580f-4f07-8706-fc232b3e8fec.png)

## `/night`

Sends all members in the Town Square channel to individual channels within the Nighttime category.

## `/day`

Brings all members from the Nighttime category channels back to the Town Square.

## `/vote`

Brings all members from other Daytime category channels back to the Town Square for nominations to begin.

## `/votetimer <time>`

Runs `/vote` after the specified amount of time. Valid times range from 15 seconds to 20 minutes.

## `/stopvotetimer`

Cancels an existing timer created by `/votetimer`.

## `/endgame`

Removes Storyteller and Villager roles, as well as the **(ST)** nickname prefix. Automatically run on the town after a few hours of inactivity.

## `/storytellers <name> <name>...`

Used to specify multiple Storytellers for a game. Will remove the Storyteller role and **(ST)** nickname prefix from any previous Storyteller(s).

Any of these Storytellers may run the various gameplay commands. They will also be grouped together into the first Cottage during the night.

Example usage:

> `/storytellers Alice Bob`


## `/evil <demon> <minion> <minion> <minion>...`

Sends DMs to the evil players informing them of their teammates.

Example usage:

> `/evil Alice Bob Carol`

The demon gets a message reading:

> Alice: You are the **demon**. Your minions are: Bob, Carol

Minions get a message reading:

> Bob: You are a **minion**. Your demon is: Alice. Your fellow minions are: Carol

## `/lunatic <lunatic> <fake minion> <fake minion> <fake minion>...`

Sends a DM to the Lunatic identical to those sent by `/evil` telling them who their fake minions are.

---

# Setup Command Details

## `/createtown <townName> [storytellerrole] [playerrole] [useNight=true]`

Creates an entire town from nothing, including all of its categories, channels, and roles.

The optional `storytellerrole` is an already-created server role for members of your server who wish to be Storytellers. They will be given access to a channel to control Bot on the Clocktower. If not provided, everyone on the server will see this channel.

The optional `playerrole` is an already-created server role for members of your server who wish to play Blood on the Clocktower. They will be granted access to see the Town Square when a game is not in progress. If not provided, everyone on the server will see the Town Square.

If `usenight` is specified as False, the Nighttime category and its Cottages will not be created. This will disable the `/day` and `/night` commands.

For more information about precisely what this sets up (in case you wanted to do it all yourself manually for some reason), see the `/addtown` command reference below.

## `/destroytown <townName>`

Destroys all the channels, categories, and roles created via the `/createtown` command.

If there are extra channels that the bot does not expect, it will leave them alone and warn you about them. Simply clean them up and run this command again to finish town destruction.

## `/towninfo`

When run in a control channel for a town, reports all the details stored by `/addtown` or `/createtown` - the channel & role names the bot is expecting.

## `/addtown <controlchannel> <townsquare> <daycategory> <storytellerrole> <villagerrole> [nightcategory] [chatchannel]`

`/addtown` tells the bot about all the roles, categories, and channels it needs to know about to do its job. It expects these things are all already created; if they are not, use `/createtown` and it will handle all of this.

**NOTE:** It is recommended that you create a town using `/createtown` above instead of using `/addtown`. But, if you've already got a setup that works for you (or you want your roles and channels to be named differently than what `/createtown` assumes), then `/addtown` might be preferred.

Here is what the bot expects to exist. Note that we are using "Ravenswood Bluff" for the example town name.

* 2 server roles for the currently-running game
  * A "**Ravenswood Bluff Storyteller**" role
  * A "**Ravenswood Bluff Villager**" role
* A "**Ravenswood Bluff**" daytime category
  * Category permissions should be set up to be visible to "**Ravenswood Bluff Villager**", and allow **Bot on the Clocktower** to move members
  * The category should contain these channels:
    * A "**control**" text channel. This is for interacting with the bot. Permissions should make this visible only to the **Bot on the Clocktower** role, as well as any members who may want to be Storytellers. It can be hidden from members who don't intend be a Storyteller, so you can remove "**Ravenswood Bluff Villager**" from the permissions set.
    * A "**Town Square**" voice channel. This is the main lobby for the game. Permissions should allow this to be visible to anyone who wants to play.
    * A variety of other voice channels for private conversations, such as "Dark Alley" and "Graveyard". These can all inherit permissions from the category.
    * A single "game-chat" text channel, also inheriting category permissions. This is for the Villagers to chat, especially during the night phase.
* A "**Ravenswood Bluff - Night**" nighttime category
  * **NOTE**: The nighttime category is optional. If you don't specify a night category, your town won't use the Night. This will disable the `/night` and `/day` commands.
  * Permission should be set up to be visible to "**Ravenswood Bluff Storyteller**", and allow **Bot on the Clocktower** to move members
  * This category can contain a bunch of voice channels that inherit category permissions. Common setup is to use 20 channels all named "Cottage"

Once all this is set up, you can run the `/addtown` command, telling it the name of your main channel, categories, and roles. For the above example, you would run:

> `/addtown control "Town Square" "Ravenswood Bluff" "Ravenswood Bluff - Night" "Ravenswood Bluff Storyteller" "Ravenswood Bluff Villager"`

If that command works, you're ready to run a game!

## `/removetown`

The opposite of `/addtown` - when run in the control channel for a town, removes registration of this town from the bot. The channels and roles will still exist and are not touched.

## `/modifytown [chatchannel] [nightcategory]`

Lets you set the optional channels for a previously-created town


---

# Lookup Command Details

## `/lookup <character name>`

Looks up a character by name. Official characters provided by https://clocktower.online/ are supported.

If custom characters are desired, see the `/addScript` command.

![image](https://user-images.githubusercontent.com/151635/162993413-2dce6201-01cb-41b7-93ee-b1bc5422b419.png)


## `/addscript <script json url>`

Informs the bot about a custom script using its json, collecting any custom characters in it.
The script is only used by your Discord server; other servers will not see your custom characters.

Some extra features are available if they are provided your script json.
* If `_meta` section has an `almanac` property, a link to the script almanac will be provided.
* If the character json has a `flavor` property, this will be included.

These features are all supported by script publishing from https://www.bloodstar.xyz/

![image](https://user-images.githubusercontent.com/151635/163033622-351ca62c-24da-40f7-8952-c18c86907bec.png)

## `/removescript <script json url>`

Tells the bot to forget about a custom script url.

## `/listscripts`

Lists all scripts the bot knows about for your server.

## `/refreshscripts`

Forces a refresh on all the custom scripts known. This is useful if you publish a new script and want to see the changes immediately. Otherwise, the bot will automatically refresh daily. 

---

# Announcement Command Details

By default, the bot will announce significant new features into the control channel for each town. It will only announce a new release once.
You can control whether your server gets these messages:

### `/announce [hearannouncements]`

Tells the bot you whether you wish to receive feature announcements for towns on your server.

---
