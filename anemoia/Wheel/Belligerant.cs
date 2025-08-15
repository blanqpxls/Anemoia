namespace ParadisisNostalga.Wheel
{
    using Godot;
    using System;
    using System.Collections.Generic; // For GetValueOrDefault
    using Engine.States; // Add this at the top to use eStates and States

    // Belligerant is a Character and provides integration for state/input systems
    // Unified Belligerant class (merged Character logic)


    namespace Belligerant
    {


        public partial class Belligerant : CharacterBody2D
        {
            [Export] public float Speed = 300.0f;
            [Export] public float JumpVelocity = 600.0f;
            [Export] public float AttackDamage = 10f;
            [Export] public float AttCooldown = 0.5f;
            [Export] public float Health = 500.0f;
            [Export] public float SymphonProfundity = 25;
            [Export] public string aActorKey = ""; // Unique actor key

            public Composition.InventoryData Inventory { get; private set; } = new Composition.InventoryData();
            public Composition.HotbarData hotbar { get; private set; } = new Composition.HotbarData();

            public Composition.EquipmentData equipment { get; private set; } = new Composition.EquipmentData();
            public Composition.ManaData mana { get; private set; } = new Composition.ManaData();
            public Composition.NotesData notes { get; private set; } = new Composition.NotesData();
            public Composition.TempMotifsData tempMotifs { get; private set; } = new Composition.TempMotifsData();
            public Composition.MotifModifiersData motifModifiers { get; private set; } = new Composition.MotifModifiersData();






            private float _attackTimer = 0f;
            private States stateManager;

            public string ScriptKey { get; private set; }
            public Player PlayerRef { get; private set; }
            public Composition.CompositionData Composition { get; private set; }

          

            public Belligerant(string scriptKey, Player player, Composition.CompositionData composition)
            {
                ScriptKey = this.aActorKey = scriptKey;

                Composition = composition;
                this.Health = composition.BaseHealth;
                Health = composition.BaseHealth;
                AttackDamage = composition.BaseDamage;
                Speed = composition.BaseSpeed;
                JumpVelocity = composition.BaseJumpVelocity;
                SymphonProfundity = composition.BaseSymphonProfundity;
                Inventory = new Composition.InventoryData();
                hotbar = new Composition.HotbarData();
                equipment = new Composition.EquipmentData();
                mana = new Composition.ManaData();
                notes = new Composition.NotesData();
                tempMotifs = new Composition.TempMotifsData();
                motifModifiers = new Composition.MotifModifiersData();
            }

            public override void _Ready()
            {
                base._Ready();
                stateManager = GetNodeOrNull<States>("States");
                if (string.IsNullOrEmpty(aActorKey))
                    aActorKey = Guid.NewGuid().ToString();
                if (stateManager != null)
                {
                    stateManager.ScriptKey = aActorKey;
                    stateManager.Speed = this.Speed;
                }

                // Attach a Camera2D to the player-controlled Belligerant
                if (stateManager != null && stateManager.ScriptKey == "Player")
                {
                    var camera = new Camera2D();
                    camera.MakeCurrent(); // Set as the active camera
                    AddChild(camera);

                    // Add a control graphic above the Belligerant
                    var controlGraphic = new TextureRect
                    {
                        Name = "ControlGraphic",
                        Texture = (Texture2D)GD.Load("res://path_to_control_graphic.png"), // Replace with actual path
                        StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
                    };
                    controlGraphic.GlobalPosition = GlobalPosition - new Vector2(0, 100); // Position above the character
                    AddChild(controlGraphic);
                }

                // Check if this is the player vessel (ScriptKey == 'Player' and aActorKey == parent name)
                var parent = GetParent();
                if (stateManager != null && stateManager.ScriptKey == "Player" && parent != null && aActorKey == parent.Name)
                {
                    SetExecutiveOverride(true);
                    // Load Emilia's stats (example: from Theatre registry)
                    var emiliaData = ParadisisNostalga.Wheel.Theatre.GetActorDefaultData("Emilia");
                    if (emiliaData != null)
                    {
                        this.Health = 500.0f;
                        this.AttackDamage = 10f;
                        this.Speed = 300.0f;
                        this.JumpVelocity = 600.0f;
                        this.SymphonProfundity = 25;
                        // Optionally, load inventory, etc.
                    }
                    GD.Print($"[Belligerant] Executive override enabled and Emilia's stats loaded for {Name}");
                }
            }

            public void SimpleAIMove(string currentControlId)
            {
                if (aActorKey != currentControlId)
                    return;
                if (GD.Randi() % 100 < 2)
                {
                    int dir = (int)(GD.Randi() % 3) - 1;
                    if (dir == -1)
                    {
                        Input.ActionPress("ui_left");
                        Input.ActionRelease("ui_right");
                    }
                    else if (dir == 1)
                    {
                        Input.ActionPress("ui_right");
                        Input.ActionRelease("ui_left");
                    }
                    else
                    {
                        Input.ActionRelease("ui_left");
                        Input.ActionRelease("ui_right");
                    }
                }
            }

            public bool ExecutiveOverride { get; set; } = false;

            public void SetExecutiveOverride(bool enabled)
            {
                ExecutiveOverride = enabled;
                if (enabled)
                    GD.Print($"Executive override enabled for {Name} (iKey: {aActorKey})");
                else
                    GD.Print($"Executive override disabled for {Name} (iKey: {aActorKey})");
            }
            public override void _PhysicsProcess(double delta)
            {
                // Always let States handle input and state logic
                if (stateManager != null)
                {
                    // If this Belligerant is under player control, States will read input directly
                    stateManager.FeedInputFromStateTable();
                    // Sync Belligerant's velocity to States for AI or external control
                    this.Velocity = stateManager.Velocity;
                }
                _attackTimer -= (float)delta;
                // Move and slide using Belligerant's velocity (set by States)
                MoveAndSlide();
            }

            public string CurrentControlId { get; set; } = "";

            private static string _currentControlId = "";
            public static string CurrentControlIdStatic
            {
                get => _currentControlId;
                set => _currentControlId = value;
            }
            public void TakeDamage(float damage)
            {
                Health -= damage;
                GD.Print($"{Name} took {damage} damage! Health: {Health}");
                if (Health <= 0)
                {
                    if (stateManager != null)
                        stateManager.ChangeState(eStates.Retired);
                    Die();
                }
            }

            /// <summary>
            /// Called when the character dies. Override this method to implement custom death behavior in derived classes.
            /// </summary>
            public virtual void Die()
            {
                GD.Print($"{Name} has died.");
                QueueFree();
            }

            /// <summary>
            /// Performs an attack action for the character. Can be overridden in derived classes to implement custom attack logic.
            /// </summary>
            public virtual void Attack()
            {
                if (_attackTimer > 0 || Health <= 0.3f * Composition.BaseHealth || Composition.BaseStamina <= 0)
                    return; // Prevent attack if cooldown is active, health is too low, or stamina is depleted

                GD.Print($"{Name} attacks for {AttackDamage} damage!");

                // Adjust attack damage based on health and modifiers
                float adjustedDamage = AttackDamage * (Health / Composition.BaseHealth);
                adjustedDamage *= (1 + (Composition.MotifModifiers.Modifiers.ContainsKey("DamageBoost") ? Composition.MotifModifiers.Modifiers["DamageBoost"] : 0) * 0.1f);
                GD.Print($"Adjusted damage: {adjustedDamage}");

                // Reduce stamina on attack
                Composition.BaseStamina -= 10f;

                _attackTimer = AttCooldown;
                if (stateManager != null)
                    stateManager.ChangeState(eStates.Attacking);
            }

            // Add Parry logic to the Belligerant class
            public void Parry()
            {
                if (_attackTimer > 0 || Composition.BaseStamina <= 0)
                    return; // Prevent parry if cooldown is active or stamina is depleted

                GD.Print($"{Name} attempts a parry!");

                // Extend a detector for parry
                var parryDetector = new Area2D();
                AddChild(parryDetector);

                // Logic to detect interactions with attacks
                parryDetector.Connect("body_entered", new Callable(this, nameof(OnParryInteraction)));

                // Set cooldown for parry and reduce stamina
                _attackTimer = Composition.ParryWindow;
                Composition.BaseStamina -= 15f;
            }

            private void OnParryInteraction(Node body)
            { }


            // Add logic for projectile deflection
            public void DeflectProjectile(Projectile projectile)
            {
                float dilation = (float)((Speed / 100) * GD.RandRange(0.35, 1.15) * GD.RandRange(0.5f * Composition.BaseStamina, Composition.BaseStamina));
                dilation *= Composition.ProjectileDeflectionMultiplier;
                projectile.Velocity -= dilation;

                GD.Print($"{Name} deflected a projectile! New velocity: {projectile.Velocity}");
            }

            public void OnStateChanged(eStates newState)
            {
                GD.Print($"{Name} transitioned to state: {newState}");
            }

            // Expose the stateManager for integration
            public States StatesManager => stateManager;
        }
    }

    // end class Character

    // Define placeholder classes for Projectile and AttackObject
    public class Projectile
    {
        public float Velocity { get; set; }
    }

    public class AttackObject
    {
        public string Name { get; set; }
        public bool IsParryable { get; set; }

        public void StaggerSource(System.Action<string> logAction)
        {
            logAction?.Invoke($"{Name} is staggered!");
        }
    }

}