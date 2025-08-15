using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

using Godot;
using ParadisisNostalga.Wheel.Engine.States;
using ParadisisNostalga.Wheel.Engine.States;
using ParadisisNostalga.Wheel.Engine.States.States;
using static ParadisisNostalga.Wheel.Belligerant;
using static ParadisisNostalga.Wheel.Belligerant;

namespace ParadisisNostalga.Wheel
{
    /// <summary>
    /// Interface for all actors in the game. Provides a foundation for behaviour, state management, and Theatre integration.
    ///
    /// Implementation Summary / Foundation:
    /// - All actors (player, enemies, NPCs) should implement IActor for unified Theatre and state machine integration.
    /// - Each actor must have a unique ActorKey, assigned on creation or by the Theatre system.
    /// - The Theatre manages all IActor instances, assigns keys, and coordinates input/AI/behaviour.
    /// - Use SetBehaviour to switch between Player, AI, or other control types at runtime.
    /// - GetCurrentState should return a string representing the actor's current logical state ("Idle", "Running", etc.).
    /// - UpdatePosition and Tick are called by the Theatre or game loop for world and logic updates.
    /// - HandleInput is the entry point for all input (player or AI), which should be fed to the actor's state system.
    /// - GetBelligerant returns the underlying Belligerant (if any) for deep integration (inventory, stats, etc.).
    /// - GetActorState returns a unique AI/thinking state object for advanced behaviour (optional).
    /// - Implementations should wire up state transitions, input feeding, and save/load hooks as needed.
    /// - This interface is the main contract for extensible, modular, and testable actor logic in the game.
    /// </summary>
    using Engine.States;

    public interface IActor : Engine.States.IState
    {
        /// Unique key for this actor instance (should be set on creation or Theatre assignment)
        string ActorKey { get; set; }

        /// Reference to the Theatre this actor belongs to
        Theatre Theatre { get; set; }

        /// Set the behaviour type for this actor (Player, Fugue, Mezzopiano, etc.)
        void SetBehaviour(Theatre.ActorBehaviourType behaviour);

        /// Get the current state of the actor (e.g., Idle, Running, Attacking, etc.)
        string GetCurrentState();

        /// Update the actor's position in the world
        void UpdatePosition(float x, float y);

        /// Handle input from player or AI (feed to state system)
        void HandleInput(StateOutput input);

        /// Called every frame to update the actor (AI, state, etc.)
        void Tick(float delta);

        /// Get the underlying Belligerant (if any)
        Belligerant GetBelligerant();

        /// Get the unique thinking/AI state for this actor (optional, for advanced AI)
        ActorState GetActorState();

        Theatre GetScriptKey(string scriptKey);


        
    }

    /// <summary>
    /// Example of a unique ActorState class for advanced AI/behaviour, inheriting from States
    /// </summary>
    /// 
    /// 
    /// 
    /// 

    public partial class ActMapper : Engine.States.States
    {


        public partial class ActMapper : Engine.States.States.;
        {
          public GetaActorKey ActorKey (

          )
        }



        public partial class ActorState : Engine.States.States
        {
            // Add unique fields or logic for advanced actor thinking/AI here
            public string Mood { get; set; } = "Thinking";
            // Example: "Stressed", "Desperate", "Manic", "Vengeful", etc.
        }
    }