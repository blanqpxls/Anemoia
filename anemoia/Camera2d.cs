using Godot;
using System;

public partial class Camera2d : Camera2D
{
    public const float DEAD_ZONE = 160.0f; // Dead zone for camera movement

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            // Get the mouse position relative to the center of the viewport
            Vector2 target = mouseMotion.Position - GetViewport().GetVisibleRect().Size * 0.5f;

            if (target.Length() < DEAD_ZONE)
            {
                Position = Vector2.Zero; // Center camera on player
            }
            else
            {
                // Move camera in the direction of the mouse, scaled by distance outside dead zone
                Position = target.Normalized() * (target.Length() - DEAD_ZONE) * 0.5f;
            }
        }
    }

    public override void _Ready()
    {
        // Optionally, center camera on player at start
        Player player = GetNodeOrNull<Player>("../Player");
        if (player != null)
        {
            GlobalPosition = player.GlobalPosition;
        }
        else
        {
            GD.PrintErr("Player node not found in Camera2d _Ready!");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // You can keep your existing follow logic here if you want
    }
}
