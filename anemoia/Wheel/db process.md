# States.cs Line-by-Line Process and Variable Documentation

This document provides a detailed, step-by-step explanation of the `States.cs` file, including what each variable, method, and class is doing, and how the process flows from input to output. This is intended to help with manual debugging and understanding of the codebase.

---

## 1. Enums and Structs

### `eStates` (enum)
Defines all possible states for the character state machine, e.g. Idle, Moving, Attacking, Air, Climb, etc.

### `InputCommand` (struct)
Represents all possible input actions the character can receive (MoveLeft, MoveRight, Jump, Attack, etc). Used to pass input to state logic.

### `StateOutput` (struct)
Represents the output of a state, e.g. what the state wants to do (move, jump, attack, etc). Used for AI or remote input.

### `IStateTable` (static class)
A global dictionary mapping ScriptKey (unique actor ID) to StateOutput. Used to feed input to each actor.

---

## 2. State Machine Core

### `IState` (interface)
Defines the contract for all state classes: `Enter()`, `Update()`, `Exit()`, `CurrentState()`, `InAirState()`.

### `States` (partial class, inherits CharacterBody2D)
The main state machine node. Handles input, state transitions, and movement logic.

#### Key Variables:
- `parentBelligerant`: Reference to parent node for executive override (player control).
- `ScriptKey`: Unique string ID for this actor instance.
- `CurrentInput`: The current input command for this frame.
- `Stamina`: Current stamina value for the character.
- `currentState`: The currently active state (implements IState).
- `stateMap`: Dictionary mapping eStates to IState instances.
- Movement constants: Gravity, MaxFall, RunAccel, MaxRun, JumpSpeed, etc. (Celeste-style platformer tuning).
- `Speed`: General speed property for movement.

#### Key Methods:
- `_Ready()`: Initializes state map, sets default state, assigns ScriptKey if needed.
- `StartHookState(Vector2)`: Starts the Hook state with a target position.
- `ChangeState(eStates)`: Handles state transitions, calls Exit/Enter on states.
- `FeedInputFromStateTable()`: Updates `CurrentInput` from executive override or global state table.
- `_PhysicsProcess(double)`: Main update loop. Feeds input, updates state, moves character, syncs parent position if needed.
- `GetCurrentStateName()`: Returns the name of the current state for debugging.

---

## 3. State Implementations

Each state class implements `IState` and contains logic for a specific character state.

### Example: `IdleState`
- Handles gravity, deceleration, jump, and transition to Moving if input is detected.

### Example: `MovingState`
- Handles movement acceleration, jump, and transition to Idle if no movement input.

### Example: `AirState`, `ClimbState`, `DashState`, etc.
- Each handles its own movement, stamina, and transitions.

---

## 4. Input to Output Process (Numbered Steps)

1. **Input Received**: Input is either from player (executive override) or from the global state table, mapped by ScriptKey.
2. **FeedInputFromStateTable()**: Called every frame to update `CurrentInput`.
3. **_PhysicsProcess()**: Main loop. Checks if this is the emilia node and ScriptKey is valid.
4. **State Update**: Calls `currentState.Update(delta, CurrentInput)` to process input and update velocity/state.
5. **MoveAndSlide()**: Applies velocity to move the character using Godot's physics.
6. **Parent Sync**: If this node is a child of a Node2D (e.g. emilia), updates the parent's position to match.
7. **State Transitions**: States can call `owner.ChangeState(eStates.X)` to switch to another state.
8. **Debug Output**: Print statements throughout for tracing state, input, and velocity.

---

## 5. Variable/Method Reference Table

| Name                | Type         | Purpose/Usage                                                                 |
|---------------------|--------------|-------------------------------------------------------------------------------|
| `eStates`           | enum         | All possible states for the state machine                                      |
| `InputCommand`      | struct       | All possible input actions for the character                                   |
| `StateOutput`       | struct       | Output of a state (for AI/remote input)                                       |
| `IStateTable`       | static class | Global table mapping ScriptKey to StateOutput                                  |
| `IState`            | interface    | Contract for all state classes                                                |
| `States`            | class        | Main state machine node, handles input, state, and movement                    |
| `parentBelligerant` | Belligerant  | Reference to parent for executive override                                     |
| `ScriptKey`         | string       | Unique ID for this actor                                                      |
| `CurrentInput`      | InputCommand | Current input for this frame                                                  |
| `Stamina`           | float        | Current stamina value                                                         |
| `currentState`      | IState       | Currently active state                                                        |
| `stateMap`          | Dictionary   | Maps eStates to IState instances                                              |
| Movement constants  | const float  | Tuning values for movement, jump, climb, etc.                                 |
| `Speed`             | float        | General speed property                                                        |
| `_Ready()`          | method       | Initializes state machine                                                     |
| `StartHookState()`  | method       | Starts Hook state with a target                                               |
| `ChangeState()`     | method       | Handles state transitions                                                     |
| `FeedInputFromStateTable()` | method | Updates input from executive override or state table                          |
| `_PhysicsProcess()` | method       | Main update loop, processes input/state/movement                              |
| `GetCurrentStateName()` | method   | Returns name of current state for debugging                                   |

---

## 6. Example State Class (Line-by-Line)

```csharp
public class IdleState : IState
{
    private readonly States owner; // Reference to the main state machine
    public IdleState(States owner) { this.owner = owner; } // Constructor
    public void Enter() { GD.Print("[IdleState] Entering Idle State"); } // Called when entering Idle
    public void Update(float delta, InputCommand input)
    {
        GD.Print($"[IdleState.Update] Input: {input} Velocity: {owner.Velocity}"); // Debug
        if (!owner.IsOnFloor()) // If not on floor, go to Air state
        {
            owner.ChangeState(eStates.Air);
            return;
        }
        owner.Velocity = new Vector2(
            Mathf.MoveToward(owner.Velocity.X, 0, States.RunReduce * delta), // Decelerate X
            owner.Velocity.Y
        );
        if (input.Jump) // If jump pressed, jump and go to InAir
        {
            owner.Velocity = new Vector2(owner.Velocity.X, States.JumpSpeed);
            owner.ChangeState(eStates.InAir);
            return;
        }
        if (input.MoveRight || input.MoveLeft) // If movement input, go to Moving
            owner.ChangeState(eStates.Moving);
    }
    public void Exit() { GD.Print("[IdleState] Exiting Idle State"); } // Called when leaving Idle
    public void CurrentState() { GD.Print("[IdleState] Current state is IdleState"); }
    public void InAirState() { owner.ChangeState(eStates.Air); }
}
```

---

## 7. State Class Patterns
- Each state class follows the same pattern: holds a reference to the owner, implements Enter/Update/Exit, and handles its own logic and transitions.

---

## 8. Debugging Tips
- Use the debug print statements to trace input, state, and velocity each frame.
- Check the state transitions in the Update methods to ensure correct flow.
- Use the variable/method reference table above to quickly identify what each part of the code is doing.

---

*This document is auto-generated for the `States.cs` file. Update as needed for new states or changes to the state machine.*
