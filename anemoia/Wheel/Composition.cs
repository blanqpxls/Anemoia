using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace ParadisisNostalga.Wheel
{
    public class Composition
    {
        public enum CompositionType
        {


            Emilia,


            Luca,

            KarlvanderWeiss,

            Lullaby,

            Cassidy,

            Bella,

            Willow,

            Demon,

            Memory,

            Morgan,


            Maia,

            Sean,

            Sienna,


            Gideon,


            Harmony,


            Melody,


            Symphony,
         

        
        }

        // Inventory data structure
        public class InventoryData
        {
            public List<string> Items { get; set; } = new(); // Item IDs or names
            public Dictionary<string, int> Quantities { get; set; } = new(); // ItemID -> count
            // Add more as needed (equipment, currency, etc.)
        }
        

        public class HotbarData
        {
            public List<string> Items { get; set; } = new(); // Hotbar item IDs or names
            public int SelectedIndex { get; set; } = 0; // Currently selected hotbar slot
            // Add more as needed (cooldowns, etc.)
        }

        public class EquipmentData
        {
            public Dictionary<string, string> EquippedItems { get; set; } = new(); // Slot -> ItemID
            // Add more as needed (equipment stats, durability, etc.)
        }

        public class ManaData
        {
            public int CurrentMana { get; set; } = 0; // Current mana points    
            public int MaxMana { get; set; } = 100; // Maximum mana points
            public int RegenerationRate { get; set; } = 1;

        }
        
        public class NotesData
        {
            public List<string> Notes { get; set; } = new(); // List of notes (text)
            public Dictionary<string, string> Metadata { get; set; } = new(); // e.g. note ID -> metadata (tags, timestamps, etc.)
            // Add more as needed (categories, pinned notes, etc.)
        }

        public class TempMotifsData
        {
            public List<string> Motifs { get; set; } = new(); // Temporary motifs (e.g. buffs, effects)
            public Dictionary<string, int> Durations { get; set; } = new(); // MotifID -> duration in seconds
            // Add more as needed (cooldowns, etc.)
        }
        public class MotifModifiersData
        {
            public Dictionary<string, int> Modifiers { get; set; } = new(); // ModifierID -> value (e.g. speed boost, damage increase)
            // Add more as needed (modifier types, durations, etc.)
        }


        public class CompMods
        {
            public Dictionary<string, int> Mods { get; set; } = new(); // ModifierID -> value (e.g. speed boost, damage increase)
            // Add more as needed (modifier types, durations, etc.)
        }

        // Composition data structure
        public class CompositionData
        {
            public string Name { get; set; }
            public float BaseDamage { get; set; }
            public float BaseHealth { get; set; }
            public float BaseSpeed { get; set; }

            public float BaseJumpVelocity { get; set; } = -900f; // Default jump velocity
            public float BaseSymphonProfundity { get; set; } = 0f;

        
            public string AppearanceAsset { get; set; }
            public string LastGameState { get; set; } // e.g. serialized state or state id
            public int SaveSlot { get; set; } // which save file
            public InventoryData Inventory { get; set; } = new InventoryData();

            public EquipmentData Equipment { get; set; } = new EquipmentData();
            public ManaData Mana { get; set; } = new ManaData();

            [Export] public NotesData Notes { get; set; } = new NotesData();
            public TempMotifsData TempMotifs { get; set; } = new TempMotifsData();

            public MotifModifiersData MotifModifiers { get; set; } = new MotifModifiersData();

            public Dictionary<string, int> CompMods { get; set; } = new Dictionary<string, int>();

            public float BaseStamina { get; set; } = 100f; // Default stamina value
            public int Level { get; set; } = 1; // Default level
            public float ParryWindow { get; set; } = 0.5f; // Time window for successful parry
            public float ProjectileDeflectionMultiplier { get; set; } = 1.0f; // Multiplier for projectile deflection calculations
        }

        // Static dictionary of base compositions
        public static readonly Dictionary<CompositionType, CompositionData> BaseCompositions = new()
        {
            { CompositionType.Emilia, new CompositionData {
                Name = "Emilia",
                BaseDamage = 10f,
                BaseHealth = 500f,
                BaseSpeed = 300f,
                AppearanceAsset = "res://assets/characters/emilia.png",
                LastGameState = null,
                SaveSlot = 0,
                Inventory = new InventoryData(), // This will be loaded/updated from save data
                CompMods = new Dictionary<string, int> { { "SpeedBoost", 1 }, { "DamageBoost", 2 } }
            } },
            // Add other compositions here as needed


            { CompositionType.Luca, new CompositionData {
                Name = "Luca",
                BaseDamage = 12f,
                BaseHealth = 450f,
                BaseSpeed = 320f,
                AppearanceAsset = "res://assets/characters/luca.png",
                LastGameState = null,
                SaveSlot = 0,
                Inventory = new InventoryData(),
                CompMods = new Dictionary<string, int> { { "SpeedBoost", 1 }, { "DamageBoost", 2 } }
            }},
        };

        // Get base stats for a composition
        public static CompositionData GetBaseComposition(CompositionType type)
        {
            if (BaseCompositions.TryGetValue(type, out var data))
                return data;
            return null;
        }

        // Animation data for each composition (e.g. frame counts, animation names, etc.)
        public class AnimationProfile
        {
            public string IdleAnimation { get; set; }
            public string RunAnimation { get; set; }
            public string AttackAnimation { get; set; }
            public int IdleFrames { get; set; }
            public int RunFrames { get; set; }
            public int AttackFrames { get; set; }
            // Add more as needed (jump, death, etc.)
        }

        // Map each composition to its animation profile
        public static readonly Dictionary<CompositionType, AnimationProfile> AnimationProfiles = new()
        {
            { CompositionType.Emilia, new AnimationProfile {
                IdleAnimation = "EmiliaIdle",
                RunAnimation = "EmiliaRun",
                AttackAnimation = "EmiliaAttack",
                IdleFrames = 6,
                RunFrames = 8,
                AttackFrames = 5
            }}
            // Add more as needed
        };

        // Get animation profile for a composition
        public static AnimationProfile GetAnimationProfile(CompositionType type)
        {
            if (AnimationProfiles.TryGetValue(type, out var profile))
                return profile;
            return null;
        }
    }
}