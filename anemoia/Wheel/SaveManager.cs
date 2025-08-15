namespace ParadisisNostalga.Wheel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.Json;
    using System.Linq; // Add this for FirstOrDefault
    using Godot;
    using Engine.States; // Add this to ensure the correct States class is referenced
    using System.Runtime.CompilerServices;

    public class Actors
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class BelligerentSaveData
    {
        public string Id { get; set; }
        public float Health { get; set; }
        public float AttackDamage { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        // Add more properties as needed
    }

    public class TheatreSaveData
    {
        public string TheatreId { get; set; }
        public string ActorId { get; set; }
        public string CharacterId { get; set; }
        public List<BelligerentSaveData> Belligerants { get; set; }
    }

    public static class SaveManager
    {
        private static string SavePath => "user://savegame.json";

        [Serializable]
        public class ActorSaveData
        {
            public string Id { get; set; }
            public string ScriptKey { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Health { get; set; }
            public float AttackDamage { get; set; }
            public string State { get; set; } // e.g. state name or enum
            public string Composition { get; set; } // e.g. "Emilia"
            public List<string> Inventory { get; set; } = new();
            public Dictionary<string, int> InventoryQuantities { get; set; } = new();
            public List<string> Hotbar { get; set; } = new(); // Hotbar item ids
            public int Mana { get; set; }
            public int Notes { get; set; }
            public int TempMotifs { get; set; }
            public List<string> Equipped { get; set; } = new(); // Equipped item ids
            public List<string> MotifModifiers { get; set; } = new(); // e.g. "Tempo", "Forte", "Legato", "Staccato"
            // Add more as needed
        }

        [Serializable]
        public class GameSaveData
        {
            public string SceneName { get; set; }
            public float WorldTime { get; set; }
            public Dictionary<string, bool> Flags { get; set; } = new();
            public List<ActorSaveData> Actors { get; set; } = new();
            // Add more as needed (player stats, world state, etc.)
        }


        public static void DevTheatreSave(TheatreSaveData data)
        {
            bool isDevMode = true;
            
        }

        public static void SaveTheatre(TheatreSaveData data)
        {
            var json = JsonSerializer.Serialize(data);
            File.WriteAllText(SavePath, json);
        }

        public static TheatreSaveData LoadTheatre()
        {
            if (!File.Exists(SavePath))
                return null;
            var json = File.ReadAllText(SavePath);
            return JsonSerializer.Deserialize<TheatreSaveData>(json);
        }


        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }

        // 2. Save and load using binary serialization
        public static void SaveGame(GameSaveData data, string path = "user://save.dat")

        {
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Write);
            var bytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(data);
            file.StoreBuffer(bytes);
        }

        public static GameSaveData LoadGame(string path = "user://save.dat")
        {
            if (!Godot.FileAccess.FileExists(path))
                return null;
            using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
            var bytes = file.GetBuffer((long)file.GetLength());
            return System.Text.Json.JsonSerializer.Deserialize<GameSaveData>(bytes);
        }

        // 3. Gather data from all actors in the scene
        public static GameSaveData GatherGameData(IEnumerable<Belligerant> actors, string sceneName, float worldTime, Dictionary<string, bool> flags)
        {
            var save = new GameSaveData
            {
                SceneName = sceneName,
                WorldTime = worldTime,
                Flags = flags
            };
            foreach (var actor in actors)
            {
                var states = actor.GetNodeOrNull<States>("States");
                var compositionType = ParadisisNostalga.Wheel.Composition.CompositionType.Emilia; // Default, replace with actual lookup if needed
                var compData = ParadisisNostalga.Wheel.Composition.GetBaseComposition(compositionType);
                var inventory = compData?.Inventory.Items ?? new List<string>();
                var inventoryQuantities = compData?.Inventory.Quantities ?? new Dictionary<string, int>();
                // Example: get hotbar, mana, notes, temp motifs, equipped from actor if available
                var hotbar = actor is IHasHotbar h ? h.GetHotbar() : new List<string>();
                var equipped = actor is IHasEquipment e ? e.GetEquipped() : new List<string>();
                int mana = actor is IHasMana m ? m.GetMana() : 0;
                int notes = actor is IHasNotes n ? n.GetNotes() : 0;
                int tempMotifs = actor is IHasTempMotifs t ? t.GetTempMotifs() : 0;
                var motifModifiers = actor is IHasMotifModifiers mm ? mm.GetMotifModifiers() : new List<string>();
                save.Actors.Add(new ActorSaveData
                {
                    Id = actor.aActorKey,
                    ScriptKey = states?.ScriptKey,
                    X = actor.Position.X,
                    Y = actor.Position.Y,
                    Health = actor.Health,
                    AttackDamage = actor.AttackDamage,
                    State = states?.ToString(),
                    Composition = compData?.Name ?? "Emilia",
                    Inventory = new List<string>(inventory),
                    InventoryQuantities = new Dictionary<string, int>(inventoryQuantities),
                    Hotbar = new List<string>(hotbar),
                    Mana = mana,
                    Notes = notes,
                    TempMotifs = tempMotifs,
                    Equipped = new List<string>(equipped),
                    MotifModifiers = new List<string>(motifModifiers)
                });
            }
            return save;
        }

        // 4. Apply loaded data to the game
        public static void ApplyGameData(GameSaveData save, IEnumerable<Belligerant> actors)
        {
            foreach (var actorData in save.Actors)
            {
                var actor = actors.FirstOrDefault(a => a.aActorKey == actorData.Id);
                if (actor == null) continue;
                actor.Position = new Godot.Vector2(actorData.X, actorData.Y);
                actor.Health = actorData.Health;
                actor.AttackDamage = actorData.AttackDamage;
                var states = actor.GetNodeOrNull<States>("States");
                if (states != null && !string.IsNullOrEmpty(actorData.ScriptKey))
                    states.ScriptKey = actorData.ScriptKey;
                var compType = ParadisisNostalga.Wheel.Composition.CompositionType.Emilia;
                var compData = ParadisisNostalga.Wheel.Composition.GetBaseComposition(compType);
                if (compData != null)
                {
                    compData.Inventory.Items = new List<string>(actorData.Inventory);
                    compData.Inventory.Quantities = new Dictionary<string, int>(actorData.InventoryQuantities);
                    // Optionally update UI here
                }
                // Restore hotbar, equipped, mana, notes, temp motifs if supported
                if (actor is IHasHotbar h) h.SetHotbar(actorData.Hotbar);
                if (actor is IHasEquipment e) e.SetEquipped(actorData.Equipped);
                if (actor is IHasMana m) m.SetMana(actorData.Mana);
                if (actor is IHasNotes n) n.SetNotes(actorData.Notes);
                if (actor is IHasTempMotifs t) t.SetTempMotifs(actorData.TempMotifs);
                if (actor is IHasMotifModifiers mm) mm.SetMotifModifiers(actorData.MotifModifiers);
            }
        }

        // 5. Stage loader: loads scene, actors, and positions them
        public static void LoadStage(GameSaveData save, Node sceneRoot, Func<string, Belligerant> spawnActor)
        {
            foreach (var actorData in save.Actors)
            {
                var actor = sceneRoot.GetChildren().OfType<Belligerant>().FirstOrDefault(a => a.aActorKey == actorData.Id);
                if (actor == null)
                {
                    actor = spawnActor(actorData.Composition);
                    if (actor == null) continue;
                    sceneRoot.AddChild(actor);
                }
                actor.Position = new Godot.Vector2(actorData.X, actorData.Y);
                actor.Health = actorData.Health;
                actor.AttackDamage = actorData.AttackDamage;
                var states = actor.GetNodeOrNull<States>("States");
                if (states != null && !string.IsNullOrEmpty(actorData.ScriptKey))
                    states.ScriptKey = actorData.ScriptKey;
                var compType = ParadisisNostalga.Wheel.Composition.CompositionType.Emilia;
                var compData = ParadisisNostalga.Wheel.Composition.GetBaseComposition(compType);
                if (compData != null)
                {
                    compData.Inventory.Items = new List<string>(actorData.Inventory);
                    compData.Inventory.Quantities = new Dictionary<string, int>(actorData.InventoryQuantities);
                }
                if (actor is IHasHotbar h) h.SetHotbar(actorData.Hotbar);
                if (actor is IHasEquipment e) e.SetEquipped(actorData.Equipped);
                if (actor is IHasMana m) m.SetMana(actorData.Mana);
                if (actor is IHasNotes n) n.SetNotes(actorData.Notes);
                if (actor is IHasTempMotifs t) t.SetTempMotifs(actorData.TempMotifs);
                if (actor is IHasMotifModifiers mm) mm.SetMotifModifiers(actorData.MotifModifiers);
            }
        }
    } // End of SaveManager class

    // Interfaces for hotbar, equipment, mana, notes, temp motifs
    public interface IHasHotbar
    {
        List<string> GetHotbar();
        void SetHotbar(List<string> hotbar);
    }
    public interface IHasEquipment
    {
        List<string> GetEquipped();
        void SetEquipped(List<string> equipped);
    }
    public interface IHasMana
    {
        int GetMana();
        void SetMana(int mana);
    }
    public interface IHasNotes
    {
        int GetNotes();
        void SetNotes(int notes);
    }
    public interface IHasTempMotifs
    {
        int GetTempMotifs();
        void SetTempMotifs(int tempMotifs);
    }
    public interface IHasMotifModifiers
    {
        List<string> GetMotifModifiers();
        void SetMotifModifiers(List<string> modifiers);
        void AddMotifModifier(string modifier);
        void RemoveMotifModifier(string modifier);
    }
} // end namespace
