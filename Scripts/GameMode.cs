using Godot;
using Godot.Collections;

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
        lifeCounters = new LifeCounter[ lifeCounterNodePaths.Count ];
        for ( int i = 0; i < lifeCounters.Length; i++ )
            lifeCounters[ i ] = GetNode<LifeCounter> ( lifeCounterNodePaths[ i ] );
    }

    public async void Reload ( float delay ) {
        reloadTimer.WaitTime = delay;
        reloadTimer.Start ();
        await ToSignal ( reloadTimer, "timeout" );
        GetTree ().ReloadCurrentScene ();
    }

    public async void Restart ( bool ignoreLifeCounters = true ) {
        await ToSignal ( GetTree (), "idle_frame" );
        if ( PauseMenu.Current.Paused )
            await ToSignal ( PauseMenu.Current, "pause_changed" );
        if ( !ignoreLifeCounters ) {
            foreach ( LifeCounter lc in lifeCounters ) {
                if ( lc.CurrentLife <= 0 )
                    return;
            }
        }
        Respawnable.RespawnAll ();
    }
}
