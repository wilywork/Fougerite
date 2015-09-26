using System.Linq;

namespace Fougerite
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Text.RegularExpressions;
    using UnityEngine;

    public class Util
    {
        private Dictionary<string, System.Type> typeCache = new Dictionary<string, System.Type>();
        private static Util util;

        public void ConsoleLog(string str, [Optional, DefaultParameterValue(false)] bool adminOnly)
        {
            try {
                foreach (Fougerite.Player player in Fougerite.Server.GetServer().Players) {
                    if (!adminOnly) {
                        ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
                    } else if (player.Admin) {
                        ConsoleNetworker.singleton.networkView.RPC<string>("CL_ConsoleMessage", player.PlayerClient.netPlayer, str);
                    }
                }
            } catch { }
        }

        public object CreateArrayInstance(string name, int size)
        {
            System.Type type;
            if (!this.TryFindType(name.Replace('.', '+'), out type)) {
                return null;
            }
            if (type.BaseType.Name == "ScriptableObject") {
                return ScriptableObject.CreateInstance(name);
            }
            return Array.CreateInstance(type, size);
        }

        public object CreateInstance(string name, params object[] args)
        {
            System.Type type;
            if (!this.TryFindType(name.Replace('.', '+'), out type)) {
                return null;
            }
            if (type.BaseType.Name == "ScriptableObject") {
                return ScriptableObject.CreateInstance(name);
            }
            return Activator.CreateInstance(type, args);
        }

        public Quaternion CreateQuat(float x, float y, float z, float w)
        {
            return new Quaternion(x, y, z, w);
        }

        public Vector3 CreateVector(float x, float y, float z)
        {
            return new Vector3(x, y, z);
        }

        public Vector2 CreateVector2(float x, float y)
        {
            return new Vector2(x, y);
        }

        public void DestroyObject(GameObject go)
        {
            NetCull.Destroy(go);
        }

        public static string NormalizePath(string path)
        {
            string normal = path.Replace(@"\\", @"\").Replace(@"//", @"/").Trim();
            return normal;
        }

        public static string GetAbsoluteFilePath(string fileName)
        {
            return Path.Combine(Config.GetPublicFolder(), fileName);
        }

        public static string GetRootFolder()
        {
            return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)));
        }

        public static string GetServerFolder()
        {
            return Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))), "rust_server_Data");
        }

        public object GetStaticField(string className, string field)
        {
            System.Type type;
            if (this.TryFindType(className.Replace('.', '+'), out type)) {
                FieldInfo info = type.GetField(field, BindingFlags.Public | BindingFlags.Static);
                if (info != null) {
                    return info.GetValue(null);
                }
            }
            return null;
        }

        public static Util GetUtil()
        {
            if (util == null) {
                util = new Util();
            }
            return util;
        }

        public float GetVectorsDistance(Vector3 v1, Vector3 v2)
        {
            return Vector3.Distance(v1, v2);
        }

        public float GetVector2sDistance(Vector2 v1, Vector2 v2)
        {
            return Vector2.Distance(v1, v2);
        }

        public static Hashtable HashtableFromFile(string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    return (Hashtable)formatter.Deserialize(stream);
                }
            }
            catch
            {
                return new Hashtable(); 
            }            
        }

        public static void HashtableToFile(Hashtable ht, string path)
        {
            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Create))
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(stream, ht);
                }
            }
            catch { }
        }

        public Vector3 Infront(Fougerite.Player p, float length)
        {
            return (p.PlayerClient.controllable.transform.position + ((Vector3)(p.PlayerClient.controllable.transform.forward * length)));
        }

        public object InvokeStatic(string className, string method, object[] args)
        {
            System.Type type;
            if (!this.TryFindType(className.Replace('.', '+'), out type)) {
                return null;
            }
            MethodInfo info = type.GetMethod(method, BindingFlags.Static);
            if (info == null) {
                return null;
            }
            if (info.ReturnType == typeof(void)) {
                info.Invoke(null, args);
                return true;
            }
            return info.Invoke(null, args);
        }

        public bool IsNull(object obj)
        {
            return (obj == null);
        }

        public void Log(string str)
        {
            Logger.Log(str);
        }

        public Match Regex(string input, string match)
        {
            return new System.Text.RegularExpressions.Regex(input).Match(match);
        }

        public Quaternion RotateX(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(angle, 0f, 0f));
        }

        public Quaternion RotateY(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(0f, angle, 0f));
        }

        public Quaternion RotateZ(Quaternion q, float angle)
        {
            return (q *= Quaternion.Euler(0f, 0f, angle));
        }

        public static void say(uLink.NetworkPlayer player, string playername, string arg)
        {
            if (!string.IsNullOrEmpty(arg) && !string.IsNullOrEmpty(playername) && player != null)
                ConsoleNetworker.SendClientCommand(player, "chat.add " + playername + " " + arg);
        }

        public static void sayAll(string customName, string arg)
        {
            ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayAll(string arg)
        {
            if (!string.IsNullOrEmpty(arg))
                ConsoleNetworker.Broadcast("chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string arg)
        {
            if (!string.IsNullOrEmpty(arg) && player != null)
                ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(Fougerite.Server.GetServer().server_message_name) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public static void sayUser(uLink.NetworkPlayer player, string customName, string arg)
        {
            if (!string.IsNullOrEmpty(arg) && !string.IsNullOrEmpty(customName) && player != null)
                ConsoleNetworker.SendClientCommand(player, "chat.add " + Facepunch.Utility.String.QuoteSafe(customName) + " " + Facepunch.Utility.String.QuoteSafe(arg));
        }

        public void SetStaticField(string className, string field, object val)
        {
            System.Type type;
            if (this.TryFindType(className.Replace('.', '+'), out type)) {
                FieldInfo info = type.GetField(field, BindingFlags.Public | BindingFlags.Static);
                if (info != null) {
                    info.SetValue(null, Convert.ChangeType(val, info.FieldType));
                }
            }
        }

        public TimeSpan ConvertToTime(long ticks)
        {
            TimeSpan ts = TimeSpan.FromTicks(ticks);
            return ts;
        }

        public bool TryFindType(string typeName, out System.Type t)
        {
            lock (this.typeCache) {
                if (!this.typeCache.TryGetValue(typeName, out t)) {
                    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                        t = assembly.GetType(typeName);
                        if (t != null) {
                            break;
                        }
                    }
                    this.typeCache[typeName] = t;
                }
            }
            return (t != null);
        }

        public System.Type TryFindReturnType(string typeName)
        {
            System.Type t;
            if (this.TryFindType(typeName, out t))
                return t;
            throw new Exception("Type not found " + typeName);
        }

        public bool ContainsString(string str, string key)
        {
            if (str.Contains(key)) { return true; }
            return false;
        }

        public ItemDataBlock ConvertNameToData(string name)
        {
            ItemDataBlock byName = DatablockDictionary.GetByName(name);
            if (byName != null) {return byName;}
            return null;
        }

        public BlueprintDataBlock BlueprintOfItem(ItemDataBlock item)
        {
            return DatablockDictionary.All.OfType<BlueprintDataBlock>().FirstOrDefault(obj => obj.resultItem == item);
        }

        [System.Obsolete("Use FindDeployableAt", false)]
        public Entity FindChestAt(Vector3 givenPosition, float dist = 1f, bool forceupdate = false)
        {
            return FindDeployableAt(givenPosition, dist, forceupdate);
        }

        public Entity FindDeployableAt(Vector3 givenPosition, float dist = 1f, bool forceupdate = false)
        {
            foreach (var x in World.GetWorld().DeployableObjects(forceupdate))
            {
                if (Vector3.Distance(x.Location, givenPosition) <= dist) return x;
            }
            return null;
        }

        public Entity FindDoorAt(Vector3 givenPosition, float dist = 2f, bool forceupdate = false)
        {
            foreach (var x in World.GetWorld().BasicDoors(forceupdate))
            {
                if (Vector3.Distance(x.Location, givenPosition) <= dist) return x;
            }
            return null;
        }

        public Entity FindStructureAt(Vector3 givenPosition, float dist = 1f, bool forceupdate = false)
        {
            foreach (var x in World.GetWorld().StructureComponents(forceupdate))
            {
                if (Vector3.Distance(x.Location, givenPosition) <= dist) return x;
            }
            return null;
        }

        public Entity FindLootableAt(Vector3 givenPosition, float dist = 1f)
        {
            foreach (var x in World.GetWorld().LootableObjects)
            {
                if (Vector3.Distance(x.Location, givenPosition) <= dist) return x;
            }
            return null;
        }

        public Entity FindEntityAt(Vector3 givenPosition, float dist = 1f)
        {
            foreach (var x in World.GetWorld().Entities)
            {
                if (Vector3.Distance(x.Location, givenPosition) <= dist) return x;
            }
            return null;
        }

        public List<Entity> FindDeployablesAround(Vector3 givenPosition, float dist = 100f, bool forceupdate = false)
        {
            return World.GetWorld().DeployableObjects(forceupdate).Where(e => Vector3.Distance(e.Location, givenPosition) <= dist).ToList();
        }

        public List<Entity> FindDoorsAround(Vector3 givenPosition, float dist = 100f, bool forceupdate = false)
        {
            return World.GetWorld().BasicDoors(forceupdate).Where(e => Vector3.Distance(e.Location, givenPosition) <= dist).ToList();
        }

        public List<Entity> FindStructuresAround(Vector3 givenPosition, float dist = 100f, bool forceupdate = false)
        {
            return World.GetWorld().StructureComponents(forceupdate).Where(e => Vector3.Distance(e.Location, givenPosition) <= dist).ToList();
        }

        public List<Entity> FindLootablesAround(Vector3 givenPosition, float dist = 100f)
        {
            return World.GetWorld().LootableObjects.Where(e => Vector3.Distance(e.Location, givenPosition) <= dist).ToList();
        }

        public List<Entity> FindEntitiesAround(Vector3 givenPosition, float dist = 100f)
        {
            return World.GetWorld().Entities.Where(e => Vector3.Distance(e.Location, givenPosition) <= dist).ToList();
        }

        [System.Obsolete("Use FindEntity", false)]
        public Entity GetEntityatCoords(Vector3 givenPosition)
        {
            return FindEntityAt(givenPosition);
        }

        [System.Obsolete("Use FindEntity", false)]
        public Entity GetEntityatCoords(float x, float y, float z)
        {
            return FindEntityAt(new Vector3(x, y, z));
        }

        [System.Obsolete("Use FindDoorAt", false)]
        public Entity GetDooratCoords(Vector3 givenPosition)
        {
            return FindDoorAt(givenPosition);
        }

        [System.Obsolete("Use FindDoorAt", false)]
        public Entity GetDooratCoords(float x, float y, float z)
        {
            return FindDoorAt(new Vector3(x, y, z));
        }

        public object GetInstanceField(Type type, object instance, string fieldName)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            try
            {
                FieldInfo field = type.GetField(fieldName, bindFlags);
                object v = field.GetValue(instance);
                return v;
            }
            catch (Exception ex)
            {
                Logger.LogError("[Reflection] Failed to get value of " + fieldName + "! " + ex.ToString());
                return null;
            }
        }

        public void SetInstanceField(Type type, object instance, string fieldName, object val)
        {
            BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                | BindingFlags.Static;
            FieldInfo field = type.GetField(fieldName, bindFlags);
            try
            {
                field.SetValue(instance, val);
            }
            catch (Exception ex)
            {
                Logger.LogError("[Reflection] Failed to set value of " + fieldName + "! " + ex.ToString());
            }
        }

        public ulong TimeInMillis
        {
            get { return NetCull.timeInMillis; }
        }
    }
}
