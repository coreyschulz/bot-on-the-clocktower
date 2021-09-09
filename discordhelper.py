'''Helper functions for dealing with common Discord cases'''
import traceback

import discord

async def verify_not_dm_or_send_error(ctx) -> bool:
    '''Verify that a message is not a DM; send an error if it is'''
    if isinstance(ctx.channel, discord.DMChannel):
        await ctx.send("Whoops, you probably meant to send that in a text channel instead of a DM!")
        return False
    return True

async def send_error_to_author(ctx, error=None) -> None:
    '''Send an error message to the author of the message in the context'''
    if error:
        formatted = error
    else:
        formatted = '```\n' + traceback.format_exc(3) + '\n```'
        traceback.print_exc()
    await ctx.author.send(f"Alas, an error has occurred:\n{formatted}\n(from message `{ctx.message.content}`)")

def get_channel_from_category_by_name(category:discord.CategoryChannel, name:str) -> discord.abc.GuildChannel:
    '''Get a channel by name given a category'''
    return discord.utils.find(lambda c: (c.type == discord.ChannelType.voice or c.type == discord.ChannelType.text) and c.name == name, category.channels)

def get_category_by_name(guild:discord.Guild, name:str) -> discord.CategoryChannel:
    '''Get a category by name given a Guild'''
    return discord.utils.find(lambda c: c.type == discord.ChannelType.category and c.name == name, guild.channels)

def get_role_by_name(guild:discord.Guild, name:str) -> discord.Role:
    ''' Get a role by name given a Guild'''
    return discord.utils.find(lambda r: r.name==name, guild.roles)

# Get a category by ID or name, preferring ID
def get_category(guild:discord.Guild, name:str, cat_id:int) -> discord.CategoryChannel:
    '''Get a category by ID or name, preferring ID, given a  Guild'''
    cat_by_id = discord.utils.find(lambda c: c.type == discord.ChannelType.category and c.id == cat_id, guild.channels)
    return cat_by_id or discord.utils.find(lambda c: c.type == discord.ChannelType.category and c.name == name, guild.channels)

# Get a channel by ID or name, preferring ID
def get_channel_from_category(category:discord.CategoryChannel, name:str, chan_id:int) -> discord.abc.GuildChannel:
    '''Get a channel by ID or name, preferring ID, given a Guild'''
    chan_by_id = discord.utils.find(lambda c: (c.type == discord.ChannelType.voice or c.type == discord.ChannelType.text) and c.id == chan_id, category.channels)
    return chan_by_id or discord.utils.find(lambda c: (c.type == discord.ChannelType.voice or c.type == discord.ChannelType.text) and c.name == name, category.channels)

# Get a role by ID or name, preferring ID
def get_role(guild:discord.Guild, name:str, role_id:int) -> discord.Role:
    '''Get a role by ID or name, preferring ID, given a Guild'''
    role_by_id = discord.utils.find(lambda r: r.id == role_id, guild.roles)
    return role_by_id or discord.utils.find(lambda r: r.name==name, guild.roles)
