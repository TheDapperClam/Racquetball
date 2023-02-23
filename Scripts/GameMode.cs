using Godot;
using Godot.Collections;

/// <summary>
/// Class for setting game mode rules.
/// </summary>
public class GameMode : Node
{
    public static GameMode Current { get; private set; }
    [Export] public int MaxPlayers { get; private set; }

    [Export] private readonly Array<NodePath> lifeCounterNodePaths = new Array<NodePath> ();
    private LifeCounter[] lifeCounters;
    private readonly Timer reloadTimer = new Timer ();

    public override void _EnterTree () {
        Current = this;
    }

    public override void _ExitTree () {
        Current = null;
    }

    public override void _Ready () {
        AddChild ( reloadTimer );
        // Set up our life counters if any nodes were given.
        lifeCounters = new LifeCounter[ lifeCounterNodePaths.Count ];
        for ( int i = 0; i < lifeCounters.Length; i++ )
            lifeCounters[ i ] = GetNode<LifeCounter> ( lifeCounterNodePaths[ i ] );
    }

    /// <summary>
    /// Function for reloading the current scene after a delay.
    /// </summary>
    /// <param name="delay"></param>
    public async void Reload ( float delay ) {
        reloadTimer.WaitTime = delay;
        reloadTimer.Start ();
        await ToSignal ( reloadTimer, "timeout" );
        GetTree ().ReloadCurrentScene ();
    }

    /// <summary>
    /// Function for starting a round
    /// </summary>
    /// <param name="ignoreLifeCounters"></param>
    public async void Restart ( bool ignoreLifeCounters = true ) {
        // Wait until the end of the frame before restarting.
        await ToSignal ( GetTree (), "idle_frame" );
        if ( PauseMenu.Current.Paused )
            await ToSignal ( PauseMenu.Current, "pause_changed" );
        if ( !ignoreLifeCounters ) {
            // If we aren't ignoring any life counters, we don't want to start another round
            // if one player has run our of life.
            foreach ( LifeCounter lc in lifeCounters ) {
                if ( lc.CurrentLife <= 0 )
                    return;
            }
        }
        Respawnable.RespawnAll ();
    }
}
