using Godot;

public class GameMode : Node
{
    public static GameMode Current { get; private set; }
    [Export] public int MaxPlayers { get; private set; }

    public override void _EnterTree () {
        Current = this;
    }

    public override void _ExitTree () {
        Current = null;
    }

    public async void Restart () {
        await ToSignal ( GetTree (), "idle_frame" );
        if ( PauseMenu.Current.Paused )
            await ToSignal ( PauseMenu.Current, "pause_changed" );
        Respawnable.RespawnAll ();
    }
}
