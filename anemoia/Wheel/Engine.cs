using System;
using System.Collections.Generic;
using Godot;
using Engine.States; // Use eStates from States.cs

namespace Engine
{
    /// <summary>
    /// Main game engine node for state notifications and global logic.
    /// It uses the eStates enum to manage game states and notify other systems of state changes.
    /// It serves as the central hub for managing game states and notifying other systems of state changes.
    /// it also uses IState and  yeah, this is a comment that was added to the file.
    ///  
    /// 
    /// 
    /// This class serves as the central hub for managing game states and notifying other systems of state changes.
    /// It initializes the available states and provides a method to notify when a state change occurs.
    /// 
    /// </summary>
    public partial class GameEngine : Node
    {
        public static GameEngine Instance { get; private set; }
        private List<Engine.States.eStates> states = new List<Engine.States.eStates>();

        public override void _Ready()
        {
            Instance = this;
            foreach (Engine.States.eStates state in Enum.GetValues(typeof(Engine.States.eStates)))
            {
                states.Add(state);
            }
        }

        public void NotifyStateChange(Engine.States.eStates newState)
        {
            GD.Print("Engine notified of state change: " + newState);
            // TODO: Notify player or other systems of state change here.
            // Example: ParadisisNostalga.Wheel.Player.Instance?.OnStateChanged(newState);
        }

        // Add any additional engine-wide methods here
    }
}

