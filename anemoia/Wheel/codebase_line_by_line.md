# Codebase Line-by-Line Numbered Explanations

This document provides a line-by-line, numbered explanation for each major file in the codebase, focusing on the main gameplay and engine logic. Each section lists the file, then provides a code block with line numbers and a detailed explanation for each line or block. Use this as a reference for deep debugging and understanding.

---

## Wheel/States.cs

> See the previous section in `db process.md` for a full breakdown. Here is a line-by-line, numbered explanation for the main state machine logic:

```csharp
1  using Godot; // Import Godot engine types
2  using System; // Import base .NET types
3  using System.Collections.Generic; // Import generic collections
4  
5  namespace Engine.States // Define the Engine.States namespace
6  {
7      // ... Enums and structs ...
8      // ... IState interface ...
9      // ...existing code...
10     public partial class States : CharacterBody2D // Main state machine node, inherits Godot's CharacterBody2D
11     {
12         public const float DashTime = 0.15f; // Dash duration in seconds
13         public const float DashSpeed = 240f; // Dash speed
14         private ParadisisNostalga.Wheel.Belligerant parentBelligerant; // Reference to parent for executive override
15         public string ScriptKey { get; set; } // Unique actor key
16         public InputCommand CurrentInput { get; set; } = InputCommand.None; // Current input for this frame
17         public float Stamina { get; set; } = 100f; // Stamina value
18         private IState currentState; // Current state instance
19         private Dictionary<eStates, IState> stateMap; // State enum to state instance map
20         // ... Movement constants ...
21         public float Speed { get; set; } = 300f; // General speed property
22         public override void _Ready() // Called when node enters the scene tree
23         {
24             parentBelligerant = GetParent() as ParadisisNostalga.Wheel.Belligerant; // Cache parent
25             stateMap = new Dictionary<eStates, IState> // Initialize state map
26             {
27                 { eStates.Idle, new IdleState(this) },
28                 { eStates.Moving, new MovingState(this) },
29                 // ...other states...
30             };
31             currentState = stateMap[eStates.Idle]; // Start in Idle
32             currentState.Enter(); // Call Enter on initial state
33             if (string.IsNullOrEmpty(ScriptKey)) // If no ScriptKey, assign one
34                 ScriptKey = Guid.NewGuid().ToString();
35         }
36         // ...existing code...
37         public void FeedInputFromStateTable() // Update input from table or executive override
38         {
39             var parentScene = GetParent();
40             bool isEmilia = parentScene != null && parentScene.Name != null && parentScene.Name.ToString().ToLowerInvariant().Contains("emilia");
41             bool validKey = !string.IsNullOrEmpty(ScriptKey);
42             if (!(isEmilia && validKey))
43             {
44                 CurrentInput = InputCommand.None;
45                 GD.Print($"[FeedInputFromStateTable] Skipping input: isEmilia={isEmilia} validKey={validKey}");
46                 return;
47             }
48             if (parentBelligerant != null && parentBelligerant.ExecutiveOverride)
49             {
50                 CurrentInput = new InputCommand
51                 {
52                     MoveLeft = Input.IsActionPressed("ui_left"),
53                     MoveRight = Input.IsActionPressed("ui_right"),
54                     // ...other input mappings...
55                 };
56                 GD.Print($"[FeedInputFromStateTable] ExecutiveOverride: {CurrentInput}");
57             }
58             else if (IStateTable.Table.TryGetValue(ScriptKey, out var output))
59             {
60                 CurrentInput = new InputCommand
61                 {
62                     MoveLeft = output.WantsToWalkLeft,
63                     MoveRight = output.WantsToWalkRight,
64                     // ...other output mappings...
65                 };
66                 GD.Print($"[FeedInputFromStateTable] ScriptKey({ScriptKey}) output: {CurrentInput}");
67             }
68             else
69             {
70                 CurrentInput = InputCommand.None;
71                 GD.Print("[FeedInputFromStateTable] No input found, using None");
72             }
73         }
74         public override void _PhysicsProcess(double delta) // Main physics loop
75         {
76             var parentScene = GetParent();
77             bool isEmilia = parentScene != null && parentScene.Name != null && parentScene.Name.ToString().ToLowerInvariant().Contains("emilia");
78             bool validKey = !string.IsNullOrEmpty(ScriptKey);
79             if (isEmilia && validKey)
80             {
81                 FeedInputFromStateTable(); // Always update input
82                 if (currentState != null)
83                 {
84                     GD.Print($"[_PhysicsProcess] State: {GetCurrentStateName()} Input: {CurrentInput} Velocity: {Velocity}");
85                     currentState.Update((float)delta, CurrentInput); // State logic
86                     GD.Print($"[_PhysicsProcess] After Update: State: {GetCurrentStateName()} Velocity: {Velocity}");
87                 }
88                 MoveAndSlide(); // Move using Godot physics
89                 if (parentScene is Node2D parent2D && parent2D != this)
90                 {
91                     parent2D.Position = this.Position; // Sync parent position
92                 }
93             }
94             else
95             {
96                 Velocity = Vector2.Zero; // Freeze movement
97                 GD.Print($"[_PhysicsProcess] Skipping state logic: isEmilia={isEmilia} validKey={validKey}");
98             }
99         }
100        public string GetCurrentStateName() // Returns current state name
101        {
102            if (currentState != null)
103                return currentState.GetType().Name;
104            return "Unknown";
105        }
106    }
// ... State classes follow ...
```

---

## Other Key Files

### main.gd
```gdscript
1  extends Node2D
2  
3  func _on_button_4_pressed():
4      get_tree().quit() # Quits the game
5  
6  func _on_button_pressed():
7      get_tree().change_scene_to_file("res://world.tscn") # Loads the world scene
```

### player.gd
```gdscript
1  extends CharacterBody2D
2  @export var speed : float = 200.0 # Movement speed
3  @export var jump_velocity : float = -150.0 # Jump velocity
4  @export var double_jump_velocity : float = -100 # Double jump velocity
5  @onready var animated_sprite : AnimatedSprite2D = $AnimatedSprite2D # Sprite reference
6  var gravity = ProjectSettings.get_setting("physics/2d/default_gravity") # Gravity value
7  var has_double_jumped : bool = false # Double jump flag
8  var animation_locked : bool = false # Animation lock flag
```

### NEwMain.cs
```csharp
1  using Godot;
2  using System;
3  public partial class NEwMain : Node2D
4  {
5      private string journalCampaign;
6      private bool quitConfig;
7      public override void _Ready()
8      { /* ... */ }
9      private void OnJournalButtonPressed()
10     { /* ... */ }
11     private void OnCampaignButtonPressed()
12     { /* ... */ }
13     private void OnQuitButtonPressed()
14     { /* ... */ }
15     private void OnStartButtonPressed()
16     { /* ... */ }
17     private void OnDevTestButtonPressed()
18     { /* ... */ }
19 }
```

### Motifs.cs
```csharp
1  using System;
2  using System.Collections.Generic;
3  using System.Linq;
4  using System.Threading.Tasks;
5  using Godot;
6  namespace Motifs
7  {
8      public class Motif
9      {
10         public int Id { get; set; }
11         public string Name { get; set; }
12         // Additional properties such as damage, cooldown, etc.
13         public Motif(int id, string name)
14         {
15             Id = id;
16             Name = name;
17         }
18     }
19 }
```

---

*For any file not listed, request a specific file for a line-by-line breakdown. This document covers the main gameplay and engine logic. For Godot scene files (.tscn), see the Godot editor for node structure and script attachment.*
