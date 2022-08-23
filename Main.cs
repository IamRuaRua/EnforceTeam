using System;
using System.Collections;
using System.Reflection;
using Terraria;
using Terraria.Localization;
using TerrariaApi.Server;
using TShockAPI;

namespace EnforceTeam

{
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin
    {
        public override string Name => "EnforceTeam";

        public override string Description => "强制玩家加入队伍";

        public override string Author => "rua";
        
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public int teamID=0;
        public ArrayList OnlinePlayers = new ArrayList();
        public Main(Terraria.Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnServerJoin);
            ServerApi.Hooks.ServerChat.Register(this, OnServerChat);
            ServerApi.Hooks.ServerLeave.Register(this, OnServerLeave);
            Commands.ChatCommands.Add(new Command(permissions: "TeamManager", cmd: this.team, "team"));
            teamID = 3;
        }
        void OnServerChat(ServerChatEventArgs args)
        {
            //发消息自动入队
            //var player = TShock.Players[args.Who];
           // player.TPlayer.team = teamID;
            //NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
        }
        void team(CommandArgs args)
        {
            var cmdArgs = args.Parameters;
            var sendPlayer = args.Player;
            try
            {
                switch (cmdArgs[0])
                {
                    case "help":
                        sendPlayer.SendMessage("/ team join username teamID          -- 让某用户加入某个队伍"
                                            + "\n/ team alljoin teamID                   -- 让所以玩家加入某个队伍"
                                            +"\nteamD:0(无队伍)   1(红队)   2(绿队)\n3(蓝队)   4(黄队)   5(粉队)", new Microsoft.Xna.Framework.Color(255, 255, 0));
                        return;
                    case "join":
                        TSPlayer player = TSPlayer.FindByNameOrID(cmdArgs[1])[0];
                        player.TPlayer.team = int.Parse(cmdArgs[2]);
                        NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
                        return;
                    case "alljoin":
                    case "allJoin":
                        teamID = int.Parse(cmdArgs[1]);
                        AllJoin();
                        return;
                    default:
                        sendPlayer.SendErrorMessage("语法错误,使用/team help获取帮助");
                        return;
                }

            }
            catch (Exception)
            {
                sendPlayer.SendErrorMessage("语法错误,使用/team help获取帮助");
                return;
            }
           
        }
        void AllJoin()
        {
        for (int i = 0; i < OnlinePlayers.Count; i++)
            {
                var player = TSPlayer.FindByNameOrID((string)OnlinePlayers[i])[0];
                if (player != null)
                {
                    FoundPlayer found = Util.GetPlayer(player, player.Name);
                    if (found.valid)
                    {
                        found.plr.TPlayer.team = teamID;
                        NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, player.Index, 0f, 0f, 0f, 0);
                        player.SendInfoMessage("");
                    }

                }
            }

        }

        void OnServerJoin(JoinEventArgs args)
        {
            TSPlayer tSPlayer = TShock.Players[args.Who];
            string name = tSPlayer.Name;
            OnlinePlayers.Add(name);
            tSPlayer.TPlayer.team = teamID;
            NetMessage.SendData((int)PacketTypes.PlayerTeam, -1, -1, NetworkText.Empty, tSPlayer.Index, 0f, 0f, 0f, 0);
        }
        void OnServerLeave(LeaveEventArgs args)
        {
            TSPlayer tSPlayer = TShock.Players[args.Who];
            string name = tSPlayer.Name;
            OnlinePlayers.Remove(name);
        }
        void RunCMDOnTshock(string command)
        {
            CommandEventArgs args = new CommandEventArgs();
            var prop = args.GetType().GetProperty("Command");
            prop.SetValue(args, command);
            ServerApi.Hooks.ServerCommand.Invoke(args);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
            base.Dispose(disposing);
        }
    }
}
