using Fougerite;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Fougerite.Events;
using UnityEngine;

namespace GlitchFix
{
    public class GlitchFix : Fougerite.Module
    {
        private bool enabled;
        private bool GiveBack;
        private bool Ramp;
        private bool Struct;
        private bool RockGlitch;
        private bool RockGlitchKill;
        private bool CheckForRampLoot;
        private IniParser Config;
        private Vector3 Vector3Down = new Vector3(0f, -1f, 0f);
        private Vector3 Vector3Up = new Vector3(0f, 1f, 0f);
        private int terrainLayer;

        public override string Name
        {
            get { return "GlitchFix"; }
        }

        public override string Author
        {
            get { return "DreTaX"; }
        }

        public override string Description
        {
            get { return "Fixing multiply ramp spawning one over one"; }
        }

        public override Version Version
        {
            get { return new Version("1.4.3");}
        }

        public override uint Order
        {
            get { return 2; }
        }

        public override void Initialize()
        {
            Config = new IniParser(Path.Combine(ModuleFolder, "GlitchFix.cfg"));
            enabled = Config.GetBoolSetting("Settings", "enabled");
            GiveBack = Config.GetBoolSetting("Settings", "giveback");
            Ramp = Config.GetBoolSetting("Settings", "rampstackcheck");
            Struct = Config.GetBoolSetting("Settings", "structurecheck");
            RockGlitch = Config.GetBoolSetting("Settings", "RockGlitch");
            RockGlitchKill = Config.GetBoolSetting("Settings", "RockGlitchKill");
            CheckForRampLoot = Config.GetBoolSetting("Settings", "CheckForRampLoot");
            terrainLayer = LayerMask.GetMask(new string[] { "Static", "Terrain" });
            if (enabled)
            {
                Fougerite.Hooks.OnEntityDeployed += EntityDeployed;
                Fougerite.Hooks.OnPlayerSpawned += OnPlayerSpawned;
            }
        }

        public override void DeInitialize()
        {
            if (enabled)
            {
                Fougerite.Hooks.OnEntityDeployed -= EntityDeployed;
                Fougerite.Hooks.OnPlayerSpawned -= OnPlayerSpawned;
            }
        }

        public void OnPlayerSpawned(Fougerite.Player player, SpawnEvent se)
        {
            if (RockGlitch)
            {
                var loc = player.Location;
                Vector3 cachedPosition = loc;
                RaycastHit cachedRaycast;
                cachedPosition.y += 100f;
                try
                {
                    if (Physics.Raycast(loc, Vector3Up, out cachedRaycast, terrainLayer))
                    {
                        cachedPosition = cachedRaycast.point;
                    }
                    if (!Physics.Raycast(cachedPosition, Vector3Down, out cachedRaycast, terrainLayer)) return;
                }
                catch
                {
                    return;
                }
                if (cachedRaycast.collider.gameObject.name != "") return;
                if (cachedRaycast.point.y < player.Y) return;
                Logger.LogDebug(player.Name + " tried to rock glitch at " + player.Location);
                Server.GetServer().Broadcast(player.Name + " don't try to rock glitch =)");
                foreach (Collider collider in Physics.OverlapSphere(player.Location, 3f))
                {
                    if (collider.gameObject.name == "SleepingBagA(Clone)")
                        TakeDamage.KillSelf(collider.GetComponent<IDMain>());
                }
                if (RockGlitchKill)
                {
                    player.Message("Glitching gets you killed.");
                    player.Kill();
                }
            }
        }

        public void EntityDeployed(Fougerite.Player Player, Fougerite.Entity Entity)
        {
            if (Entity != null)
            {
                if (Entity.Name.Contains("Foundation") || Entity.Name.Contains("Ramp") 
                    || Entity.Name.Contains("Pillar") || Entity.Name == "WoodDoor" || Entity.Name == "MetalDoor")
                {
                    string name = Entity.Name;
                    var location = Entity.Location;
                    if (Ramp && Entity.Name.Contains("Ramp"))
                    {
                        StructureComponent[] structurelist = UnityEngine.Object.FindObjectsOfType(typeof(StructureComponent)) as StructureComponent[];
                        if (structurelist != null && structurelist.Where(structure => structure.name.Contains("Ramp") && Entity.InstanceID != structure.GetInstanceID()).Any(structure => (int)Math.Round(Vector3.Distance(location, structure.gameObject.transform.position)) == 0))
                        {
                            Entity.Destroy();
                            if (GiveBack && Player.IsOnline)
                            {
                                switch (name)
                                {
                                    case "WoodFoundation":
                                        name = "Wood Foundation";
                                        break;
                                    case "MetalFoundation":
                                        name = "Metal Foundation";
                                        break;
                                    case "WoodRamp":
                                        name = "Wood Ramp";
                                        break;
                                    case "MetalRamp":
                                        name = "Metal Ramp";
                                        break;
                                    case "WoodPillar":
                                        name = "Wood Pillar";
                                        break;
                                    case "MetalPillar":
                                        name = "Metal Pillar";
                                        break;
                                }
                                Player.Inventory.AddItem(name, 1);
                            }
                        }
                    }
                    if (Struct)
                    {
                        float d = 4.5f;
                        if (Entity.Name.Contains("Pillar"))
                        {
                            d = 0.40f;
                        }
                        else if (Entity.Name.Contains("Door"))
                        {
                            d = 0.40f;
                        }
                        else if (Entity.Name.Contains("Foundation"))
                        {
                            d = 4.5f;
                        }
                        else if (Entity.Name.Contains("Ramp"))
                        {
                            if (!CheckForRampLoot)
                            {
                                return;
                            }
                            d = 3.5f;
                        }
                        var x = Physics.OverlapSphere(location, d);
                        if (x.Any(l => l.name.ToLower().Contains("woodbox") || l.name.ToLower().Contains("smallstash")))
                        {
                            Entity.Destroy();
                            if (Player.IsOnline && GiveBack)
                            {
                                switch (name)
                                {
                                    case "WoodFoundation":
                                        name = "Wood Foundation";
                                        break;
                                    case "MetalFoundation":
                                        name = "Metal Foundation";
                                        break;
                                    case "WoodRamp":
                                        name = "Wood Ramp";
                                        break;
                                    case "MetalRamp":
                                        name = "Metal Ramp";
                                        break;
                                    case "WoodPillar":
                                        name = "Wood Pillar";
                                        break;
                                    case "MetalPillar":
                                        name = "Metal Pillar";
                                        break;
                                    case "WoodDoor":
                                        name = "Wood Door";
                                        break;
                                    case "MetalDoor":
                                        name = "Metal Door";
                                        break;
                                }
                                Player.Inventory.AddItem(name, 1);
                            }
                        }
                    }
                }
            }
        }
    }
}
