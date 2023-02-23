using Godot;
using System.Collections.Generic;

/// <summary>
/// Class for objects that we want to be able to reset to their starting state
/// </summary>
public abstract class Respawnable : KinematicBody2D
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

    /// <summary>
    /// Function for when our node enters the scene tree.
    /// Use the _RespawnableEnterTree function instead when attempting to override.
    /// </summary>
    public sealed override void _EnterTree () {
        // We always have to add our node to the static list of respawnable nodes
        // whenever it enters the scene, so we'll override the _RespawnableEnterTree function
        // as a safeguard for the base function not being called from the derived class.
        currentRespawnables.Add ( this );
        _RespawnableEnterTree ();
    }

    /// <summary>
    /// Function for when our node exits the scene tree.
    /// Use the _RespawnableExitTree function instead when attempting to override.
    /// </summary>
    public sealed override void _ExitTree () {
        // Issues would occure if this function were to be overridden, and the
        // base function not called, resulting in the respawnable node not being
        // removed from the static list of respawnable nodes.
        // We use the _RespawnableExitTree function to ensure that any code that needs to run does so.
        currentRespawnables.Remove ( this );
        _RespawnableExitTree ();
    }

    /// <summary>
    /// Function for when our node is ready.
    /// Use the _RespawnableReady function instead when attempting to override.
    /// </summary>
    public sealed override void _Ready () {
        // We don't want our derived classes to accidentally exclude vital code
        // by not calling the base function, so we have the _RespawnableReady function
        // which can safely be overriden with or without the base call.
        animationPlayer = GetNode<AnimationPlayer> ( animationPlayerNodePath );
        startingParent = GetParent ();
        startingPosition = Position;
        startingRotation = Rotation;
        startingScale = Scale;
        startingCollisionMask = CollisionMask;
        animationPlayer.Play ( startingAnimation );
        _RespawnableReady ();
    }

    /// <summary>
    /// Function for resetting our respawnable object to its starting state
    /// </summary>
    public virtual void Respawn () {
        GD.Print ( "Respawning: " + Name );
        // We want our respawnable node to revert to its original parent node
        // as not doing so will resault in gameplay differences if our node
        // was reparented.
        if ( GetParent () != startingParent ) {
            GetParent ().RemoveChild ( this );
            startingParent.AddChild ( this );
            Owner = startingParent;
        }
        isDead = false;
        Position = startingPosition;
        Rotation = startingRotation;
        Scale = startingScale;
        animationPlayer.Play ( startingAnimation );
        CollisionMask = startingCollisionMask;
        SetAsToplevel ( false );
        EmitSignal ( nameof ( on_respawn ) );
    }

    /// <summary>
    /// Respawn all of our respawnable objects
    /// </summary>
    public static void RespawnAll () {
        // We'll be iterating through a temporary copy of our list of respawnables.
        // This is done in order to avoid any errors in our foreach loop caused by
        // modifying the original list while currently in the loop.
        List<Respawnable> tempRespawnables = new List<Respawnable> ();
        tempRespawnables.AddRange ( currentRespawnables );
        foreach ( Respawnable r in tempRespawnables )
            r.Respawn ();
    }

    /// <summary>
    /// Function for when our respawnable node is ready, and is called at the end of the default _Ready function.
    /// Used instead of the default _Ready function to ensure that important code is always executed regardless of
    /// whether or not the base function is called from the derived class.
    /// </summary>
    protected virtual void _RespawnableReady () { }
    /// <summary>
    /// Function for when our respawnable node enters the scene tree, and is called at the end of the default _EnterTree function.
    /// Used instead of the default _EnterTree function to ensure that important code is always executed regardless of
    /// whether or not the base function is called from the derived class.
    /// </summary>
    protected virtual void _RespawnableEnterTree () { }
    /// <summary>
    /// Function for when our respawnable node exits the scene tree, and is called at the end of the default _ExitTree function.
    /// Used instead of the default _ExitTree function to ensure that important code is always executed regardless of
    /// whether or not the base function is called from the derived class.
    /// </summary>
    protected virtual void _RespawnableExitTree () { }
}
