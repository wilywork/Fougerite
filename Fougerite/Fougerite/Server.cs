using System;
using System.IO;

namespace Fougerite
{
    using System.Linq;
    using System.Collections.Generic;

    public class Server
    {
        private ItemsBlocks _items;
        private StructureMaster _serverStructs = new StructureMaster();
        public Fougerite.Data data = new Fougerite.Data();
        private List<Fougerite.Player> players = new List<Fougerite.Player>();
        private static Fougerite.Server server;
        private bool HRustPP = false;
        public string server_message_name = "Fougerite";
        public static IDictionary<ulong, Fougerite.Player> Cache = new Dictionary<ulong, Fougerite.Player>();
        public static IDictionary<Fougerite.Player, List<string>> CommandCancelList = new Dictionary<Player, List<string>>();
        public static IEnumerable<string> ForceCallForCommands = new List<string>();
        private string path = Path.Combine(Util.GetRootFolder(), Path.Combine("Save", "GlobalBanList.ini"));

        public void LookForRustPP()
        {
            if (HRustPP) { return; }
            foreach (ModuleContainer m in ModuleManager.Modules.Where(m => m.Plugin.Name.Equals("RustPP")))
            {
                HRustPP = true;
                break;
            }
        }

        public void BanPlayer(Fougerite.Player player, string Banner = "Console", string reason = "You were banned.")
        {
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            foreach (Fougerite.Player pl in Players.Where(pl => pl.Admin || pl.Moderator))
            {
                pl.Message(red + player.Name + white + " was banned by: "
                           + green + Banner);
                pl.Message(red + " Reason: " + reason);
            }
            BanPlayerIP(player.IP, player.Name, reason, Banner);
            BanPlayerID(player.SteamID, player.Name, reason, Banner);
            player.Message(red + " " + reason);
            player.Message(red + " Banned by: " + Banner);
            player.Disconnect();
        }

        public void BanPlayerIP(string ip, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ips", ip))
            {
                return;
            }
            ini.AddSetting("Ips", ip, name);
            /*if (!name.Equals("1"))
            {
                if (ini.ContainsSetting("NameIps", name))
                {
                    for (int i = 1; i <= 150; i++)
                    {
                        if (!ini.ContainsSetting("NameIps", name + i))
                        {
                            ini.AddSetting("NameIps", name + i, ip);
                            break;
                        }
                    }
                }
                else
                {
                    ini.AddSetting("NameIps", name, ip);
                }
            }*/
            if (!ini.ContainsSetting("AdminWhoBanned", name + "|" + ip))
            {
                ini.AddSetting("AdminWhoBanned", name + "|" + ip, adminname + "|" + reason);
            }
            ini.Save();
        }

        public void BanPlayerID(string id, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ids", id))
            {
                return;
            }
            ini.AddSetting("Ids", id, name);
            /*if (!name.Equals("1"))
            {
                if (ini.ContainsSetting("NameIds", name))
                {
                    for (int i = 1; i <= 150; i++)
                    {
                        if (!ini.ContainsSetting("NameIps", name + i))
                        {
                            ini.AddSetting("NameIds", name + i, id);
                            break;
                        }
                    }
                }
                else
                {
                    ini.AddSetting("NameIds", name, id);
                }
            }*/
            if (!ini.ContainsSetting("AdminWhoBanned", name + "|" + id))
            {
                ini.AddSetting("AdminWhoBanned", name + "|" + id, adminname + "|" + reason);
            }
            ini.Save();
        }

        public bool IsBannedID(string id)
        {
            IniParser ini = GlobalBanList;
            return ini.ContainsSetting("Ids", id);
        }

        public bool IsBannedIP(string ip)
        {
            IniParser ini = GlobalBanList;
            return ini.ContainsSetting("Ips", ip);
        }

        public bool UnbanByName(string name, string UnBanner = "Console")
        {
            string id = FindIDOfName(name);
            string ip = FindIPOfName(name);
            if (id == null)
            {
                return false;
            }
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            foreach (Fougerite.Player pl in Server.GetServer().Players.Where(pl => pl.Admin || pl.Moderator))
            {
                pl.Message(red + name + white + " was unbanned by: "
                           + green + UnBanner);
            }
            IniParser ini = GlobalBanList;
            //var iprq = ini.GetSetting("NameIps", ip);
            //var idrq = ini.GetSetting("NameIds", id);
            ini.DeleteSetting("Ips", ip);
            ini.DeleteSetting("Ids", id);
            //ini.DeleteSetting("NameIps", name);
            //ini.DeleteSetting("NameIds", name);
            ini.Save();
            return true;
        }

        public bool UnbanByIP(string ip)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ips", ip))
            {
                ini.DeleteSetting("Ips", ip);
                ini.Save();
                return true;
            }
            return false;
        }

        public bool UnbanByID(string id)
        {
            IniParser ini = GlobalBanList;
            if (ini.ContainsSetting("Ids", id))
            {
                ini.DeleteSetting("Ids", id);
                ini.Save();
                return true;
            }
            return false;
        }

        public string FindIPOfName(string name)
        {
            IniParser ini = GlobalBanList;
            var ips = ini.EnumSection("Ips");
            string l = name.ToLower();
            foreach (var ip in ips.Where(ip => ini.GetSetting("Ips", ip).ToLower() == l))
            {
                return ip;
            }
            return null;
        }

        public string FindIDOfName(string name)
        {
            IniParser ini = GlobalBanList;
            var ids = ini.EnumSection("Ids");
            string l = name.ToLower();
            foreach (var id in ids.Where(ip => ini.GetSetting("Ids", ip).ToLower() == l))
            {
                return id;
            }
            return null;
        }

        public void Broadcast(string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Message(arg);
            }
        }

        public void BroadcastFrom(string name, string arg)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.MessageFrom(name, arg);
            }
        }

        public void BroadcastNotice(string s)
        {
            foreach (Fougerite.Player player in this.Players)
            {
                player.Notice(s);
            }
        }

        public Fougerite.Player FindPlayer(string search)
        {
            IEnumerable<Fougerite.Player> query;
            if (search.StartsWith("7656119"))
            {
                ulong uid;
                if (ulong.TryParse(search, out uid))
                {
                    if (Cache.ContainsKey(uid))
                    {
                        return Cache[uid];
                    }
                    query = from player in this.players
                            where player.UID == uid
                            select player;

                    if (query.Count() == 1)
                        return query.First();
                }
                else
                {
                    query = from player in this.players
                            group player by search.Similarity(player.SteamID) into match
                            orderby match.Key descending
                            select match.FirstOrDefault();

                    Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.SteamID).ToArray<string>())));
                    return query.FirstOrDefault();
                }
            }
            query = from player in this.players
                    group player by search.Similarity(player.Name) into match
                    orderby match.Key descending
                    select match.FirstOrDefault();

            Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.Name).ToArray<string>())));
            return query.FirstOrDefault();
        }

        public static Fougerite.Server GetServer()
        {
            if (server == null)
            {
                server = new Fougerite.Server();
            }
            return server;
        }

        public void Save()
        {
            AvatarSaveProc.SaveAll();
            ServerSaveManager.AutoSave();
        }

        public List<string> ChatHistoryMessages
        {
            get
            {
                return Fougerite.Data.GetData().chat_history;
            }
        }

        public List<string> ChatHistoryUsers
        {
            get
            {
                return Fougerite.Data.GetData().chat_history_username;
            }
        }

        public ItemsBlocks Items
        {
            get
            {
                return this._items;
            }
            set
            {
                this._items = value;
            }
        }

        public List<Fougerite.Player> Players
        {
            get
            {
                return this.players;
            }
        }

        public List<Sleeper> Sleepers
        {
            get
            {
                var query = from s in UnityEngine.Object.FindObjectsOfType<SleepingAvatar>()
                            select new Sleeper(s.GetComponent<DeployableObject>());
                return query.ToList<Sleeper>();
            }
        }

        /*
         *  ETC....
         */

        public bool HasRustPP 
        {
            get { return HRustPP; }
        }

        public RustPPExtension GetRustPPAPI()
        {
            if (HasRustPP) 
            {
                 return new RustPPExtension();
            }
            return null;
        }

        public IniParser GlobalBanList
        {
            get
            {
                if (!File.Exists(path))
                {
                    File.Create(path).Dispose();
                }
                return new IniParser(path);
            }
        }
    }
}