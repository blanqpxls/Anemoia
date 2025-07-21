using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ParadisisNostalga.Wheel.Theatre;
using Godot;


using static Player;

namespace ParadisisNostalga.Wheel
{
    public class Theatre
    {
        public string Name { get; set; }
        public List<Belligerant> Actors { get; set; } = new List<Belligerant>();

        // Assign a unique ScriptKey to the Character and its States node
        public void AssignActorId(Belligerant character)
        {
            if (string.IsNullOrEmpty(character.aActorKey))
                character.aActorKey = Guid.NewGuid().ToString();
            var states = character.GetNodeOrNull<Engine.States.States>("States");
            if (states != null)
                states.ScriptKey = character.aActorKey;
            if (!Actors.Contains(character))
                Actors.Add(character);
        }

        // Call this each frame to update all actors' input from the global IStateTable
        public void FeedInputsToActors()
        {
            foreach (var actor in Actors)
            {
                var states = actor.GetNodeOrNull<Engine.States.States>("States");
                if (states != null)
                    states.FeedInputFromStateTable();
            }
        }

        public enum ActorBehaviourType
        {
            Player,
            Fugue,
            Mezzopiano,

            Forte,

            Pianissimo,

            FortePiano,
        }

        // Assign a behaviour to an actor
        public void SetActorBehaviour(Belligerant actor, ActorBehaviourType behaviour)
        {
            switch (behaviour)
            {
                case ActorBehaviourType.Player:
                    actor.SetExecutiveOverride(true); // Player is always under executive override
                    break;
                case ActorBehaviourType.Fugue:
                case ActorBehaviourType.Mezzopiano:
                    actor.SetExecutiveOverride(false); // AI
                    break;
            }
        }

        // Get an actor by their iKey (actor key)
        public Belligerant GetActorByKey(string key)
        {
            return Actors.Find(a => a.aActorKey == key);
        }


        // Registry of actors by behaviour type and actor key, including executive override and default mood/state
        // Usage: Acting[behaviour][actorKey] = IActor instance
        public static readonly Dictionary<ActorBehaviourType, Dictionary<string, ActorRegistryEntry>> Acting = new()
        {

            [ActorBehaviourType.Player] = new Dictionary<string, ActorRegistryEntry>
            {
                ["Emilia"] = new ActorRegistryEntry
                {
                    ActorKey = "Emilia",
                    ExecutiveOverride = true,
                    DefaultMood = "Calm",
                    Behaviour = ActorBehaviourType.Player,
                    ScriptKey = "Player",
                }
            }

            // Add more entries for other behaviours/actors as needed
        };

        // Registry entry for an actor's default data, mood, and executive override
        public class ActorRegistryEntry
        {
            public string ActorKey { get; set; }

            public bool ExecutiveOverride { get; set; }
            public string DefaultMood { get; set; }

            public ActorBehaviourType Behaviour { get; set; }

            public object LastGameState { get; set; }
            public int SaveSlot { get; set; }
            public Composition.InventoryData Inventory { get; set; }
            public Dictionary<string, int> CompMods { get; set; }
            public string ScriptKey { get; internal set; }
        }

        // Get the default data for an actor by their key
        public static ActorRegistryEntry GetActorDefaultData(string actorKey)
        {
            return Acting.Values.SelectMany(dict => dict.Values)
                .FirstOrDefault(entry => entry.ActorKey == actorKey);
        }

        /// <summary>
        /// Finds the Player node in the scene tree, gets its Belligerant child, registers it, and sets up executive override and state integration.
        /// </summary>
        /// <param name="root">The root node of the scene tree (e.g., GetTree().Root or GetTree().CurrentScene).</param>
        /// <returns>The registered Belligerant vessel for the player, or null if not found.</returns>
        public Belligerant RegisterPlayerFromTree(Node root)
        {
            if (root == null)
                return null;
            // Find the Player node (by type or name)
            Player playerNode = null;
            foreach (Node child in root.GetChildren())
            {
                if (child is Player p)
                {
                    playerNode = p;
                    break;
                }
                // Recursively search children
                var found = FindPlayerInTree(child);
                if (found != null)
                {
                    playerNode = found;
                    break;
                }
            }
            if (playerNode == null)
                return null;
            // Find Belligerant child of Player
            Belligerant vessel = null;
            foreach (Node child in playerNode.GetChildren())
            {
                if (child is Belligerant b)
                {
                    vessel = b;
                    break;
                }
            }
            if (vessel == null)
                return null;
            // Register and set up
            AssignActorId(vessel);
            SetActorBehaviour(vessel, ActorBehaviourType.Player);
            // Ensure States node is assigned and parented to the Belligerant if not already
            var states = vessel.GetNodeOrNull<Engine.States.States>("States");
            if (states == null)
            {
                // Try to find a States node anywhere under Player and reparent it
                states = FindStatesInTree(playerNode);
                if (states != null)
                {
                    // Remove from old parent and add to vessel
                    states.GetParent()?.RemoveChild(states);
                    vessel.AddChild(states);
                    states.Name = "States";
                }
                else
                {
                    // Optionally, create a new States node if not found
                    // states = new Engine.States.States();
                    // vessel.AddChild(states);
                    // states.Name = "States";
                }
            }
            // Set ScriptKey for state logic
            if (states != null)
                states.ScriptKey = vessel.aActorKey;
            return vessel;
        }

        // Helper: recursively find Player node in tree
        private Player FindPlayerInTree(Node node)
        {
            if (node is Player p)
                return p;
            foreach (Node child in node.GetChildren())
            {
                var found = FindPlayerInTree(child);
                if (found != null)
                    return found;
            }
            return null;
        }

        // Helper: recursively find States node in tree
        private Engine.States.States FindStatesInTree(Node node)
        {
            if (node is Engine.States.States s)
                return s;
            foreach (Node child in node.GetChildren())
            {
                var found = FindStatesInTree(child);
                if (found != null)
                    return found;
            }
            return null;
        }
    }
}
