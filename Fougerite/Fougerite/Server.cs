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
        public static IEnumerable<string> ForceCallForCommands = new List<string>();
        private readonly string path = Path.Combine(Util.GetRootFolder(), Path.Combine("Save", "GlobalBanList.ini"));

        public void LookForRustPP()
        {
            if (HRustPP) { return; }
            foreach (ModuleContainer m in ModuleManager.Modules.Where(m => m.Plugin.Name.Equals("RustPP")))
            {
                HRustPP = true;
                break;
            }
        }

        public void BanPlayer(Fougerite.Player player, string Banner = "Console", string reason = "You were banned.", Fougerite.Player Sender = null)
        {
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            player.Message(red + " " + reason);
            player.Message(red + " Banned by: " + Banner);
            if (Sender != null)
            {
                var loc = player.Location.ToString();
                Sender.Message("You banned " + player.Name);
                Sender.Message("Player's IP: " + player.IP);
                Sender.Message("Player's ID: " + player.SteamID);
                Sender.Message("Player's Location: " + loc);
            }
            player.Disconnect();
            foreach (Fougerite.Player pl in Players.Where(pl => pl.Admin || pl.Moderator))
            {
                pl.Message(red + player.Name + white + " was banned by: "
                           + green + Banner);
                pl.Message(red + " Reason: " + reason);
            }
            BanPlayerIP(player.IP, player.Name, reason, Banner);
            BanPlayerID(player.SteamID, player.Name, reason, Banner);
        }

        public void BanPlayerIP(string ip, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            IniParser ini = GlobalBanList;
            ini.AddSetting("Ips", ip, name);
            ini.AddSetting("AdminWhoBanned", name + "|" + ip, adminname + "|" + reason);
            ini.Save();
        }

        public void BanPlayerID(string id, string name = "1", string reason = "You were banned.", string adminname = "Unknown")
        {
            IniParser ini = GlobalBanList;
            ini.AddSetting("Ids", id, name);
            ini.AddSetting("AdminWhoBanned", name + "|" + id, adminname + "|" + reason);
            ini.Save();
        }

        public bool IsBannedID(string id)
        {
            IniParser ini = GlobalBanList;
            return (ini.GetSetting("Ids", id) != null);
        }

        public bool IsBannedIP(string ip)
        {
            IniParser ini = GlobalBanList;
            return (ini.GetSetting("Ips", ip) != null);
        }

        public bool UnbanByName(string name, string UnBanner = "Console", Fougerite.Player Sender = null)
        {
            var ids = FindIDsOfName(name);
            var ips = FindIPsOfName(name);
            string red = "[color #FF0000]";
            string green = "[color #009900]";
            string white = "[color #FFFFFF]";
            if (ids.Count == 0 && ips.Count == 0)
            {
                if (Sender != null) { Sender.Message(red + "Couldn't find any names matching with " + name); }
                return false;
            }
            foreach (Fougerite.Player pl in Players.Where(pl => pl.Admin || pl.Moderator))
            {
                pl.Message(red + name + white + " was unbanned by: "
                           + green + UnBanner + white + " Different matches: " + ids.Count);
            }
            IniParser ini = GlobalBanList;
            var iptub = ips.Last();
            var idtub = ids.Last();
            ini.DeleteSetting("Ips", iptub);
            ini.DeleteSetting("Ids", idtub);
            ini.Save();
            return true;
        }

        public bool UnbanByIP(string ip)
        {
            IniParser ini = GlobalBanList;
            if (ini.GetSetting("Ips", ip) != null)
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
            if (ini.GetSetting("Ids", id) != null)
            {
                ini.DeleteSetting("Ids", id);
                ini.Save();
                return true;
            }
            return false;
        }

        public List<string> FindIPsOfName(string name)
        {
            IniParser ini = GlobalBanList;
            var ips = ini.EnumSection("Ips");
            string l = name.ToLower();
            List<string> collection = new List<string>();
            foreach (var ip in ips)
            {
                if (ini.GetSetting("Ips", ip).ToLower() == l) collection.Add(ip);
            }
            return collection;
        }

        public List<string> FindIDsOfName(string name)
        {
            IniParser ini = GlobalBanList;
            var ids = ini.EnumSection("Ids");
            string l = name.ToLower();
            List<string> collection = new List<string>();
            foreach (var id in ids)
            {
                if (ini.GetSetting("Ids", id).ToLower() == l) collection.Add(id);
            }
            return collection;
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
                            select match.DefaultIfEmpty().FirstOrDefault();

                    Logger.LogDebug(string.Format("[FindPlayer] search={0} matches={1}", search, string.Join(", ", query.Select(p => p.SteamID).ToArray<string>())));
                    return query.DefaultIfEmpty().FirstOrDefault();
                }
            }
            var list = players.Where(x => x.Name.ToLower().Contains(search.ToLower()) || string.Equals(x.Name, search, StringComparison.CurrentCultureIgnoreCase)).ToList();
            if (list.Count >= 1)
            {
                Logger.LogDebug("[FindPlayer] Matches: " + list.Count + " First Match: " + list[0].Name);
                return list[0];
            }

            Logger.LogDebug("[FindPlayer] 0 Matches");
            return null;
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