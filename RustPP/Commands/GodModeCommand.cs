namespace RustPP.Commands
{
    using Fougerite;
    using System;
    using System.Collections.Generic;

    internal class GodModeCommand : ChatCommand
    {
        private List<ulong> userIDs = new List<ulong>();

        public override void Execute(ref ConsoleSystem.Arg Arguments, ref string[] ChatArguments)
        {
            var pl = Fougerite.Server.Cache[Arguments.argUser.userID];
            if (pl.CommandCancelList.Contains("god"))
            {
                if (userIDs.Contains(pl.UID))
                {
                    userIDs.Remove(pl.UID);
                    pl.PlayerClient.controllable.character.takeDamage.SetGodMode(false);
                }
                return;
            }
            if (!this.userIDs.Contains(Arguments.argUser.userID))
            {
                this.userIDs.Add(Arguments.argUser.userID);
                pl.PlayerClient.controllable.character.takeDamage.SetGodMode(true);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "God mode has been activated!");
            }
            else
            {
                this.userIDs.Remove(Arguments.argUser.userID);
                pl.PlayerClient.controllable.character.takeDamage.SetGodMode(false);
                Util.sayUser(Arguments.argUser.networkPlayer, Core.Name, "God mode has been deactivated!");
            }
        }

        public bool IsOn(ulong uid)
        {
            return this.userIDs.Contains(uid);
        }
    }
}