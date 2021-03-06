﻿/*
 *  This file is part of uEssentials project.
 *      https://uessentials.github.io/
 *
 *  Copyright (C) 2015-2016  Leonardosc
 *
 *  This program is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation; either version 2 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along
 *  with this program; if not, write to the Free Software Foundation, Inc.,
 *  51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
*/

using Essentials.Api.Command;
using Essentials.Api.Command.Source;
using Essentials.Api.Unturned;
using Essentials.Common;
using Essentials.I18n;

namespace Essentials.Commands
{
    [CommandInfo(
        Name = "experience",
        Aliases = new[] { "exp" },
        Description = "Give experience to you/player",
        Usage = "[amount] <target>"
    )]
    public class CommandExperience : EssCommand
    {
        private const int MAX = 10000000;

        public override CommandResult OnExecute( ICommandSource src, ICommandArgs args )
        {
            if ( args.Length == 0 || (args.Length == 1 && src.IsConsole))
            {
                return CommandResult.ShowUsage();
            }

            if ( !args[0].IsInt )
            {
                return CommandResult.Lang(EssLang.INVALID_NUMBER, args[0]);
            }

            var amount = args[0].ToInt;

            if ( amount > MAX || amount < -MAX )
            {
                return CommandResult.Lang(EssLang.NUMBER_BETWEEN, -MAX, MAX );
            }

            if ( args.Length == 2 )
            {
                if ( args[1].Is( "*" ) )
                {
                    UServer.Players.ForEach( p => GiveExp( p, amount ) );

                    if (amount >= 0)
                        EssLang.EXPERIENCE_GIVEN.SendTo( src, amount, EssLang.EVERYONE );
                    else
                        EssLang.EXPERIENCE_TAKE.SendTo( src, -amount, EssLang.EVERYONE );
                }
                else if ( !args[1].IsValidPlayerName )
                {
                    return CommandResult.Lang( EssLang.PLAYER_NOT_FOUND, args[1] );
                }
                else
                {
                    var player = args[1].ToPlayer;

                    if (amount >= 0)
                        EssLang.EXPERIENCE_GIVEN.SendTo( src, amount, player.DisplayName );
                    else
                        EssLang.EXPERIENCE_TAKE.SendTo( src, -amount, player.DisplayName );

                    GiveExp( player, amount );
                }
            }
            else
            {
                GiveExp( src.ToPlayer(), amount );
            }

            return CommandResult.Success();
        }

        private void GiveExp( UPlayer player, int amount )
        {
            var playerExp = player.UnturnedPlayer.skills.Experience;

            if ( amount < 0 )
            {
                if ( (playerExp - amount) < 0 )
                    playerExp = 0;
                else
                    playerExp += (uint) amount;
            }
            else
            {
                playerExp += (uint) amount;
            }

            if (amount >= 0)
                EssLang.EXPERIENCE_RECEIVED.SendTo( player, amount );
            else
                EssLang.EXPERIENCE_LOST.SendTo( player, -amount );

            player.UnturnedPlayer.skills.Experience = playerExp;
            player.UnturnedPlayer.skills.askSkills( player.CSteamId );
        }
    }
}
