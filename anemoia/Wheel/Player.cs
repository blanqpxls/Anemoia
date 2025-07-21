using Godot;
using System;
using System.Collections.Generic;
using ParadisisNostalga.Wheel;
using Engine.States;
using static ParadisisNostalga.Wheel.Composition;

public partial class Player : CharacterBody2D,
    ParadisisNostalga.Wheel.IHasHotbar,
    ParadisisNostalga.Wheel.IHasEquipment,
    ParadisisNostalga.Wheel.IHasMana,
    ParadisisNostalga.Wheel.IHasNotes,
    ParadisisNostalga.Wheel.IHasTempMotifs,
    ParadisisNostalga.Wheel.IHasMotifModifiers
{
    [Export] public float MaxRun = 300.0f;
    [Export] public float JumpVelocity = -900.0f;
    [Export] public float AttackDamage = 10f;	
    [Export] public float AttCooldown = 0.5f;
    [Export] public float Health = 500.0f;
    [Export] public int SymphonProfundity = 25;
    [Export] public int MaxJumps = 2; // Maximum number of jumps
    [Export] public float Acceleration = 130.0f;
    [Export] public float Deceleration = 130.0f; 

    private int jumpCount = 0;
    private bool isJumping = false;

    // Reference to the player's composition (default to Emilia)
    public ParadisisNostalga.Wheel.Composition.CompositionData Composition { get; private set; }
    // Reference to the Belligerant wrapper for state/input system
    public ParadisisNostalga.Wheel.Belligerant Emmmii { get; private set; }
    // Unique ScriptKey for this player (for Theatre and SaveManager)
    public string ScriptKey { get; private set; }

    // Inventory, hotbar, equipment, etc. (implementations for interfaces)
    public ParadisisNostalga.Wheel.Composition.InventoryData Inventory { get; private set; } = new ParadisisNostalga.Wheel.Composition.InventoryData();
    public ParadisisNostalga.Wheel.HotbarData Hotbar { get; private set; } = new ParadisisNostalga.Wheel.HotbarData();
    public ParadisisNostalga.Wheel.EquipmentData Equipment { get; private set; } = new ParadisisNostalga.Wheel.EquipmentData();
    public ParadisisNostalga.Wheel.ManaData Mana { get; private set; } = new ParadisisNostalga.Wheel.ManaData();
    public ParadisisNostalga.Wheel.NotesData Notes { get; private set; } = new ParadisisNostalga.Wheel.NotesData();
    public ParadisisNostalga.Wheel.TempMotifsData TempMotifs { get; private set; } = new ParadisisNostalga.Wheel.TempMotifsData();
    public ParadisisNostalga.Wheel.MotifModifiersData MotifModifiers { get; private set; } = new ParadisisNostalga.Wheel.MotifModifiersData();

    public static Player Instance { get; private set; }

    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Attacking
    }

    public PlayerState currentState = PlayerState.Idle;

    public override void _Ready()
    {
        Instance = this;
        ScriptKey = "Player";
        Composition = ParadisisNostalga.Wheel.Composition.BaseCompositions[ParadisisNostalga.Wheel.Composition.CompositionType.Emilia];
        Emmmii = new ParadisisNostalga.Wheel.Belligerant(ScriptKey, this, Composition);
        // Add Emmmii (the Belligerant) as a child of the Player node so it is in the scene tree
        AddChild(Emmmii);
        var theatre = new ParadisisNostalga.Wheel.Theatre();
        theatre.AssignActorId(Emmmii);
        theatre.SetActorBehaviour(Emmmii, ParadisisNostalga.Wheel.Theatre.ActorBehaviourType.Player);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Basic player input handling
        bool moveRight = Input.IsActionPressed("ui_right") || Input.IsKeyPressed(Key.D);
        bool moveLeft = Input.IsActionPressed("ui_left") || Input.IsKeyPressed(Key.A);
        bool jump = Input.IsActionPressed("ui_accept") || Input.IsActionPressed("ui_jump");
        bool attack = Input.IsActionPressed("ui_attack");

        // Feed input to the Belligerant's States system
        if (Emmmii != null && Emmmii.StatesManager != null)
        {
            var output = new Engine.States.StateOutput
            {
                WantsToMove = moveRight || moveLeft,
                WantsToAttack = attack,
                WantsToJump = jump,
                // Add more as needed
            };
            Engine.States.IStateTable.Table[ScriptKey] = output;
            Emmmii.StatesManager.FeedInputFromStateTable();
        }

        // Optionally, print for test
        if (moveRight)
            GD.Print("Player wants to move right");
        if (moveLeft)
            GD.Print("Player wants to move left");
        if (jump)
            GD.Print("Player wants to jump");
        if (attack)
            GD.Print("Player wants to attack");

        // Update composition type if state changes (example: change archetype on state)
 
    }

    private void HandleIdleState()
    {
        
    }

    private void HandleRunningState()
    {
        // Implement running state logic here
    }

    private void HandleJumpingState()
    {
        // Implement jumping state logic here
    }

    private void HandleAttackingState()
    {
        // Implement attacking state logic here
    }

    private void Jump()
    {
        if (jumpCount < MaxJumps)
        {
            Velocity = new Vector2(Velocity.X, JumpVelocity);
            jumpCount++;
            isJumping = true;
            GD.Print("Player jumped");
        }
    }

    private void Land()
    {
        jumpCount = 0;
        isJumping = false;
        GD.Print("Player landed");
    }

    public void OnStateChanged(eStates newState)
    {
        GD.Print("Player notified of state change: " + newState);
        switch (newState)
        {
            case eStates.Idle:
                currentState = PlayerState.Idle;
                Land();
                break;
            case eStates.Moving:
                currentState = PlayerState.Running;
                break;
            case eStates.Air:
            case eStates.InAir:
                currentState = PlayerState.Jumping;
                Jump();
                break;
            case eStates.Attacking:
                currentState = PlayerState.Attacking;
                break;
            default:
                GD.Print($"Unhandled state transition: {newState}");
                break;
        }
    }

    // Implement interface methods for IHasHotbar, IHasEquipment, etc.
    public List<string> GetHotbar() => Hotbar.Items;
    public void SetHotbar(List<string> hotbar) => Hotbar.Items = hotbar;
    public List<string> GetEquipped() => Equipment.Items;
    public void SetEquipped(List<string> equipped) => Equipment.Items = equipped;
    public int GetMana() => Mana.Value;
    public void SetMana(int mana) => Mana.Value = mana;
    public int GetNotes() => Notes.Value;
    public void SetNotes(int notes) => Notes.Value = notes;
    public int GetTempMotifs() => TempMotifs.Value;
    public void SetTempMotifs(int tempMotifs) => TempMotifs.Value = tempMotifs;
    public List<string> GetMotifModifiers() => MotifModifiers.Items;
    public void SetMotifModifiers(List<string> modifiers) => MotifModifiers.Items = modifiers;
    public void AddMotifModifier(string modifier) { if (!MotifModifiers.Items.Contains(modifier)) MotifModifiers.Items.Add(modifier); }
    public void RemoveMotifModifier(string modifier) { MotifModifiers.Items.Remove(modifier); }
}