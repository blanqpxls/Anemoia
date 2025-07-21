using Godot; // 1: Import Godot engine for node and math types
using System; // 2: Import base .NET types
using System.Collections.Generic; // 3: Import generic collections for dictionaries/lists

namespace Engine.States // 4: Namespace for all state machine logic
{
    // Revert the eStates enum to its original location
    public enum eStates
    {
        Idle,       // Standing still
        Moving,     // Walking/running
        Attacking,  // Attacking
        Retired,    // Dead/retired
        Air,        // In air (falling/jumping)
        InAir,      // Jumping (separate from Air for logic split)
        gStrike,    // Ground attack
        gDefending, // Ground defend
        Parry,      // Parry
        Hook        // Hook
    };

    public partial class States : CharacterBody2D
    {
        // === CONSTANTS ===
        public const float DashTime = 0.15f; // 1: Dash duration (seconds)
        public const float DashSpeed = 240f; // 2: Dash speed
        public const float Gravity = 900f; // 3: Gravity force per second
        public const float MaxFall = 160f; // 4: Max normal fall speed
        public const float FastMaxFall = 240f; // 5: Max fast fall speed
        public const float RunAccel = 1000f; // 6: Acceleration when running
        public const float RunReduce = 400f; // 7: Deceleration when stopping
        public const float AirMult = 0.65f; // 8: Air control multiplier
        public const float MaxRun = 90f; // 9: Max horizontal run speed
        public const float JumpSpeed = -105f; // 10: Initial jump velocity (up)
        public const float JumpHBoost = 40f; // 11: Horizontal boost on jump
        public const float JumpGraceTime = 0.1f; // 12: Coyote time window
        public const float VarJumpTime = 0.2f; // 13: Variable jump hold time
        public const float ClimbMaxStamina = 110f; // 14: Max stamina for climbing
        public const float ClimbUpCost = 100f / 2.2f; // 15: Stamina/sec climbing up
        public const float ClimbStillCost = 100f / 10f; // 16: Stamina/sec standing on wall
        public const float ClimbJumpCost = 110f / 4f; // 17: Stamina for wall jump
        public const float ClimbUpSpeed = -45f; // 18: Climb up speed
        public const float ClimbDownSpeed = 80f; // 19: Climb down speed
        public const float ClimbSlipSpeed = 30f; // 20: Wall slip speed
        public const float ClimbAccel = 900f; // 21: Climb acceleration
        public const float DuckFriction = 500f; // 22: Ducking friction
        public const float DuckCorrectSlide = 50f; // 23: Slide correction speed
        public const float DuckSuperJumpXMult = 1.25f; // 24: Super jump X multiplier
        public const float DuckSuperJumpYMult = 0.5f; // 25: Super jump Y multiplier
        public const float WallSlideStartMax = 20f; // 26: Max speed to start wall slide
        public const float WallSlideTime = 1.2f; // 27: Wall slide time
        public const float WallJumpHSpeed = MaxRun + JumpHBoost; // 28: Wall jump horizontal speed
        public const float WallJumpForceTime = 0.16f; // 29: Wall jump force time
        public const float WallJumpSpeed = MaxRun + JumpHBoost; // 30: Wall jump speed
        public const float WallJumpCheckDist = 3f; // 31: Wall check distance
        public const float WallSlideFriction = 0.5f; // 32: Wall slide friction
        public const float DuckFrictionMult = 0.5f; // 33: Duck friction multiplier
        public const float WalkSpeed = 64f; // 34: Walk speed
        public const float GStrike = 10f; // 35: Gravity strike force
        public const float HookPullSpeed = 400f; // 36: Hook pull speed

        // === FIELDS ===
        private ParadisisNostalga.Wheel.Belligerant parentBelligerant; // 37: Reference to parent Belligerant
        public string ScriptKey { get; set; } // 38: Unique actor key
        public InputCommand CurrentInput { get; set; } = InputCommand.None; // 39: Current input command
        public float Stamina { get; set; } = 100f; // 40: Current stamina
        private IState currentState; // 41: Current state
        private Dictionary<eStates, IState> stateMap; // 42: State map
        public float Speed { get; set; } = 300f; // 43: General speed property

        // === MAIN PHYSICS LOOP ===
        public override void _PhysicsProcess(double delta) // 170: Main update loop
        {
            var parentScene = GetParent(); // 171: Get parent
            bool isEmilia = parentScene != null && parentScene.Name != null && parentScene.Name.ToString().ToLowerInvariant().Contains("emilia"); // 172: Only process if emilia
            bool validKey = !string.IsNullOrEmpty(ScriptKey); // 173: Only process if ScriptKey is valid
            if (isEmilia && validKey) // 174: If both true
            {
                FeedInputFromStateTable(); // 175: Update input
                if (currentState != null) // 176: If state exists
                {
                    GD.Print($"[_PhysicsProcess] State: {GetCurrentStateName()} Input: {CurrentInput} Velocity: {Velocity}"); // 177: Debug print
                    currentState.Update((float)delta, CurrentInput); // 178: Update state
                    GD.Print($"[_PhysicsProcess] After Update: State: {GetCurrentStateName()} Velocity: {Velocity}"); // 179: Debug print
                }
                MoveAndSlide(); // 180: Move character
                if (parentScene is Node2D parent2D && parent2D != this) // 181: If parent is Node2D
                {
                    parent2D.Position = this.Position; // 182: Sync parent position
                }
            }
            else
            {
                Velocity = Vector2.Zero; // 183: Freeze movement
                GD.Print($"[_PhysicsProcess] Skipping state logic: isEmilia={isEmilia} validKey={validKey}"); // 184: Debug print
            }
        }

        public void FeedInputFromStateTable()
        {
            if (IStateTable.Table.TryGetValue(ScriptKey, out var output)) // 154: If AI/remote input
            {
                CurrentInput = new InputCommand // 155: Map output to input
                {
                    MoveLeft = output.WantsToWalkLeft, // 156: Walk left
                    MoveRight = output.WantsToWalkRight, // 157: Walk right
                    MoveUp = false, // 158: Not mapped
                    MoveDown = false, // 159: Not mapped
                    Jump = output.WantsToJump, // 160: Jump
                    Attack = output.WantsToAttack, // 161: Attack
                    Defend = output.WantsToDefend, // 162: Defend
                    Dash = output.WantsToDash, // 163: Dash
                    Climb = output.WantsToClimb, // 164: Climb
                    Parry = output.WantsToParry, // 165: Parry
                    Hook = output.WantsToHook // 166: Hook
                };
                GD.Print($"[FeedInputFromStateTable] ScriptKey({ScriptKey}) output: {CurrentInput}"); // 167: Debug print
            }
            else
            {
                GD.Print("[FeedInputFromStateTable] No input found, using None"); // 169: Debug print
            }
        }

        // === GET CURRENT STATE NAME ===
        public string GetCurrentStateName() // 185: Get current state name
        {
            if (currentState != null) // 186: If state exists
                return currentState.GetType().Name; // 187: Return class name
            return "Unknown"; // 188: If no state
        }

        // Method to change the current state
        public void ChangeState(eStates newState)
        {
            if (stateMap == null || !stateMap.ContainsKey(newState))
            {
                GD.PrintErr($"[ChangeState] State {newState} not found in state map.");
                return;
            }

            currentState?.Exit(); // Exit the current state
            currentState = stateMap[newState]; // Set the new state
            currentState.Enter(); // Enter the new state

            GD.Print($"[ChangeState] Transitioned to {newState} state.");
        }
    }

    // --- State Implementations ---
    public class IdleState : IState
    {
        private readonly States owner;
        public IdleState(States owner) { this.owner = owner; }
        public void Enter() { GD.Print("[IdleState] Entering Idle State"); }
        public void Update(float delta, InputCommand input)
        {
            GD.Print($"[IdleState.Update] Input: {input} Velocity: {owner.Velocity}");
            // Apply gravity if not on floor
            if (!owner.IsOnFloor())
            {
                owner.ChangeState(eStates.Air);
                return;
            }
            // Decelerate X velocity to zero (Celeste style)
            owner.Velocity = new Vector2(
                Mathf.MoveToward(owner.Velocity.X, 0, States.RunReduce * delta),
                owner.Velocity.Y
            );
            // Handle jump
            if (input.Jump)
            {
                owner.Velocity = new Vector2(owner.Velocity.X, States.JumpSpeed);
                owner.ChangeState(eStates.InAir);
                return;
            }
            // Handle movement input
            if (input.MoveRight || input.MoveLeft)
                owner.ChangeState(eStates.Moving);
        }
        public void Exit() { GD.Print("[IdleState] Exiting Idle State"); }
        public void CurrentState() { GD.Print("[IdleState] Current state is IdleState"); }
        public void InAirState() { owner.ChangeState(eStates.Air); }
    }

    public class MovingState : IState
    {
        private readonly States owner; // Reference to the main States controller

        // Constructor: takes the owner as a parameter
        public MovingState(States owner)
        {
            this.owner = owner; // Set the owner reference
        }

        // Called when entering the Moving state
        public void Enter()
        {
            GD.Print("[MovingState] Entering Moving State"); // Debug print
        }

        // Called every frame while in the Moving state
        public void Update(float delta, InputCommand input)
        {
            GD.Print($"[MovingState.Update] Input: {input} Velocity: {owner.Velocity}"); // Debug print

            // Apply gravity if not on the floor
            if (!owner.IsOnFloor())
            {
                owner.ChangeState(eStates.Air); // Transition to Air state
                return;
            }

            // Handle jump input
            if (input.Jump)
            {
                owner.Velocity = new Vector2(owner.Velocity.X, States.JumpSpeed); // Set jump velocity
                owner.ChangeState(eStates.InAir); // Transition to InAir state
                return;
            }

            // Movement logic (Celeste style)
            float x = 0; // Initialize horizontal input
            if (input.MoveRight) x += 1; // Move right
            if (input.MoveLeft) x -= 1; // Move left

            float target = x * States.MaxRun; // Calculate target velocity
            float accel = States.RunAccel * delta; // Calculate acceleration

            // Smoothly adjust velocity towards target
            owner.Velocity = new Vector2(
                Mathf.MoveToward(owner.Velocity.X, target, accel),
                owner.Velocity.Y
            );

            // Transition to Idle state if no horizontal input
            if (x == 0)
                owner.ChangeState(eStates.Idle);
        }

        // Called when exiting the Moving state
        public void Exit()
        {
            GD.Print("[MovingState] Exiting Moving State"); // Debug print
        }

        // Debug: print current state
        public void CurrentState()
        {
            GD.Print("[MovingState] Current state is MovingState");
        }

        // Called if the character transitions to an in-air state
        public void InAirState()
        {
            owner.ChangeState(eStates.Air); // Transition to Air state
        }
    }

    // Landed on floor
    public class GStrike : IState
    {
        private readonly States owner;
        public GStrike(States owner) { this.owner = owner; }
        public void Enter() => GD.Print("Entering Ground Attacking State");
        public void Update(float delta, InputCommand input) { /* GStrike logic */ }
        public void Exit() => GD.Print("Exiting Ground Attacking State");
        public void CurrentState() => GD.Print("Current state is GStrike");
        public void InAirState() { GD.Print("Transitioning to InAirState from GStrike"); owner.ChangeState(eStates.Air); }
    }

    public class gDefending : IState
    {
        private readonly States owner;
        public gDefending(States owner) { this.owner = owner; }
        public void Enter() => GD.Print("Entering Ground Defending State");
        public void Update(float delta, InputCommand input) { /* GDefending logic */ }
        public void Exit() => GD.Print("Exiting Ground Defending State");
        public void CurrentState() => GD.Print("Current state is gDefending");
        public void InAirState() { GD.Print("Transitioning to InAirState from gDefending"); owner.ChangeState(eStates.Air); }
    }

    public class DashState : IState
    {
        private readonly States owner;
        private float dashTimer;
        public DashState(States owner) { this.owner = owner; }
        public void Enter()
        {
            GD.Print("Entering Dash State");
            dashTimer = States.DashTime;
            // Dash logic can use input if needed
        }
        public void Update(float delta, InputCommand input)
        {
            dashTimer -= delta;
            if (dashTimer <= 0)
                owner.ChangeState(eStates.Idle);
        }
        public void Exit() => GD.Print("Exiting Dash State");
        public void CurrentState() => GD.Print("Current state is DashState");
        public void InAirState() { GD.Print("Transitioning to InAirState from DashState"); owner.ChangeState(eStates.Air); }
    }

    public class ClimbState : IState
    {
        private readonly States owner;
        public ClimbState(States owner) { this.owner = owner; }
        public void Enter() => GD.Print("Entering Climb State");
        public void Update(float delta, InputCommand input)
        {
            if (input.MoveUp)
            {
                owner.Velocity = new Vector2(
                    owner.Velocity.X,
                    Math.Max(owner.Velocity.Y - States.ClimbAccel * delta, States.ClimbUpSpeed)
                );
                owner.Stamina -= 1 * delta;
            }
            else if (input.MoveDown)
            {
                owner.Velocity = new Vector2(
                    owner.Velocity.X,
                    Math.Min(owner.Velocity.Y + States.ClimbAccel * delta, States.ClimbDownSpeed)
                );
                owner.Stamina -= 1 * delta;
            }
            else
            {
                owner.Velocity = new Vector2(
                    owner.Velocity.X,
                    Mathf.Lerp(owner.Velocity.Y, 0, States.ClimbSlipSpeed * delta)
                );
            }
            if (owner.Stamina <= 0)
                owner.ChangeState(eStates.Air);
        }
        public void Exit() => GD.Print("Exiting Climb State");
        public void CurrentState() => GD.Print("Current state is ClimbState");
        public void InAirState() { GD.Print("Transitioning to InAirState from ClimbState"); owner.ChangeState(eStates.Air); }
    }

    public class ParryState : IState
    {
        private readonly States owner;
        public ParryState(States owner) { this.owner = owner; }
        public void Enter() => GD.Print("Entering Parry State");
        public void Update(float delta, InputCommand input) { /* Parry logic */ }
        public void Exit() => GD.Print("Exiting Parry State");
        public void CurrentState() => GD.Print("Current state is ParryState");
        public void InAirState() { GD.Print("Transitioning to InAirState from ParryState"); owner.ChangeState(eStates.Air); }
    }

    public class GStaggered : IState
    {
        private readonly States owner; // Reference to the main States controller

        // Constructor: takes the owner as a parameter
        public GStaggered(States owner)
        {
            this.owner = owner; // Set the owner reference
        }

        // Called when entering the Staggered state
        public void Enter()
        {
            GD.Print("Entering Staggered State"); // Debug print
        }

        // Called every frame while in the Staggered state
        public void Update(float delta, InputCommand input)
        {
            // Logic for staggered state (e.g., temporary immobility or recovery)
            GD.Print("[GStaggered.Update] Staggered logic not yet implemented");
        }

        // Called when exiting the Staggered state
        public void Exit()
        {
            GD.Print("Exiting Staggered State"); // Debug print
        }

        // Debug: print current state
        public void CurrentState()
        {
            GD.Print("Current state is GStaggered");
        }

        // Called if the character transitions to an in-air state
        public void InAirState()
        {
            GD.Print("Transitioning to InAirState from GStaggered");
            owner.ChangeState(eStates.Air); // Transition to Air state
        }
    }

    public class GSymphonCool : IState
    {
        private readonly States owner; // Reference to the main States controller

        // Constructor: takes the owner as a parameter
        public GSymphonCool(States owner)
        {
            this.owner = owner; // Set the owner reference
        }

        // Called when entering the Cooldown state
        public void Enter()
        {
            GD.Print("Entering GSymphon Cooldown State"); // Debug print
        }

        // Called every frame while in the Cooldown state
        public void Update(float delta, InputCommand input)
        {
            // Logic for cooldown state (e.g., waiting for an ability to recharge)
            GD.Print("[GSymphonCool.Update] Cooldown logic not yet implemented");
        }

        // Called when exiting the Cooldown state
        public void Exit()
        {
            GD.Print("Exiting GSymphon Cooldown State"); // Debug print
        }

        // Debug: print current state
        public void CurrentState()
        {
            GD.Print("Current state is GSymphonCool");
        }

        // Called if the character transitions to an in-air state
        public void InAirState()
        {
            GD.Print("Transitioning to InAirState from GSymphonCool");
            owner.ChangeState(eStates.Air); // Transition to Air state
        }
    }

    public class GStrikeSpecialCool : IState
    {
        private readonly States owner; // Reference to the main States controller

        // Constructor: takes the owner as a parameter
        public GStrikeSpecialCool(States owner)
        {
            this.owner = owner; // Set the owner reference
        }

        // Called when entering the Special Cooldown state
        public void Enter()
        {
            GD.Print("Entering GStrike Special Cooldown State"); // Debug print
        }

        // Called every frame while in the Special Cooldown state
        public void Update(float delta, InputCommand input)
        {
            // Logic for special cooldown state (e.g., waiting for a powerful ability to recharge)
            GD.Print("[GStrikeSpecialCool.Update] Special Cooldown logic not yet implemented");
        }

        // Called when exiting the Special Cooldown state
        public void Exit()
        {
            GD.Print("Exiting GStrike Special Cooldown State"); // Debug print
        }

        // Debug: print current state
        public void CurrentState()
        {
            GD.Print("Current state is GStrikeSpecialCool");
        }

        // Called if the character transitions to an in-air state
        public void InAirState()
        {
            GD.Print("Transitioning to InAirState from GStrikeSpecialCool");
            owner.ChangeState(eStates.Air); // Transition to Air state
        }
    }

    // --- HookState: Handles grappling hook movement and transition ---
    public class HookState : IState
    {
        private readonly States owner; // Reference to the main States controller
        private Vector2 hookTarget;    // The target position to pull towards
        private readonly float pullSpeed = States.HookPullSpeed; // Pull speed constant

        // Constructor: takes the owner and the hook target position
        public HookState(States owner, Vector2 hookTarget)
        {
            this.owner = owner;           // Set the owner reference
            this.hookTarget = hookTarget; // Set the target position
        }

        // Called when entering the Hook state
        public void Enter()
        {
            GD.Print("Entering Hook State"); // Debug print
        }

        // Called every frame while in the Hook state
        public void Update(float delta, InputCommand input)
        {
            // Calculate direction vector from current position to hook target
            Vector2 direction = (hookTarget - owner.Position).Normalized();
            // Set velocity towards the hook target at pullSpeed
            owner.Velocity = direction * pullSpeed;
            // If close enough to the target, transition to Idle state
            if (owner.Position.DistanceTo(hookTarget) < 10f)
                owner.ChangeState(eStates.Idle);
        }

        // Called when exiting the Hook state
        public void Exit() => GD.Print("Exiting Hook State");

        // Debug: print current state
        public void CurrentState() => GD.Print("Current state is HookState");

        // Called if the character transitions to an in-air state
        public void InAirState() => owner.ChangeState(eStates.Air);
    }

    // Define the IState interface
    public interface IState
    {
        void Enter(); // Called when entering the state
        void Update(float delta, InputCommand input); // Called every frame while in the state
        void Exit(); // Called when exiting the state
        void CurrentState(); // Debug: print current state
        void InAirState(); // Transition to in-air state
    }

    // Define the InputCommand struct
    public struct InputCommand
    {
        public bool MoveLeft; // Move left input
        public bool MoveRight; // Move right input
        public bool MoveUp; // Move up input
        public bool MoveDown; // Move down input
        public bool Jump; // Jump input
        public bool Attack; // Attack input
        public bool Defend; // Defend input
        public bool Dash; // Dash input
        public bool Climb; // Climb input
        public bool Parry; // Parry input
        public bool Hook; // Hook input

        // Default empty input command
        public static InputCommand None => new InputCommand();
    }

    // Define a placeholder static class for IStateTable
    public static class IStateTable
    {
        public static Dictionary<string, StateOutput> Table { get; } = new Dictionary<string, StateOutput>();
    }

    // Define a placeholder struct for StateOutput
    public struct StateOutput
    {
        public bool WantsToWalkLeft;
        public bool WantsToWalkRight;
        public bool WantsToJump;
        public bool WantsToAttack;
        public bool WantsToDefend;
        public bool WantsToDash;
        public bool WantsToClimb;
        public bool WantsToParry;
        public bool WantsToHook;
    }
}
