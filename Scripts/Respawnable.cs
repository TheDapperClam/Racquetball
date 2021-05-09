using Godot;
using System.Collections.Generic;

public class Respawnable : KinematicBody2D
{
    private static readonly List<Respawnable> currentRespawnables = new List<Respawnable> ();

    [Signal] public delegate void on_respawn ();

    [Export] protected readonly NodePath animationPlayerNodePath;
    [Export] protected string startingAnimation = "Idle";
    protected AnimationPlayer animationPlayer;

    protected Node startingParent;
    protected Vector2 startingPosition;
    protected float startingRotation;
    protected Vector2 startingScale;
    protected bool startingCollision;
    protected uint startingCollisionMask;
    protected bool isDead;

    public sealed override void _EnterTree () {
        currentRespawnables.Add ( this );
        _RespawnableEnterTree ();
    }

    public sealed override void _ExitTree () {
        currentRespawnables.Remove ( this );
        _RespawnableExitTree ();
    }

    public sealed override void _Ready () {
        animationPlayer = GetNode<AnimationPlayer> ( animationPlayerNodePath );
        startingParent = GetParent ();
        startingPosition = Position;
        startingRotation = Rotation;
        startingScale = Scale;
        startingCollisionMask = CollisionMask;
        animationPlayer.Play ( startingAnimation );
        _RespawnableReady ();
    }

    public virtual void Respawn () {
        GD.Print ( "Respawning: " + Name );
        GetParent ().RemoveChild ( this );
        startingParent.AddChild ( this );
        Owner = startingParent;
        isDead = false;
        Position = startingPosition;
        Rotation = startingRotation;
        Scale = startingScale;
        animationPlayer.Play ( startingAnimation );
        CollisionMask = startingCollisionMask;
        SetAsToplevel ( false );
        EmitSignal ( nameof ( on_respawn ) );
    }

    public static void RespawnAll () {
        List<Respawnable> tempRespawnables = new List<Respawnable> ();
        tempRespawnables.AddRange ( currentRespawnables );
        foreach ( Respawnable r in tempRespawnables )
            r.Respawn ();
    }

    protected virtual void _RespawnableReady () { }
    protected virtual void _RespawnableEnterTree () { }
    protected virtual void _RespawnableExitTree () { }
}
