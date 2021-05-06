using Godot;

public class PauseMenu : Menu
{
    private const string PAUSE_ACTION = "Pause";

    public static PauseMenu Current { get; private set; }
    public bool Paused { get; private set; }
    [Signal] public delegate void pause_changed ( bool paused );

    [Export] private readonly PackedScene mainMenuScene;

    public override void _Input ( InputEvent @event ) {
        if ( @event.IsActionPressed ( PAUSE_ACTION ) && ( currentMenu == null || currentMenu.AllowPause ) )
            SetPause ( !Paused, !Paused );
    }

    public void LoadMainMenu () {
        SetPause ( false, false );
        ChangeScene ( mainMenuScene );
    }

    protected override void _ReadyMenu () {
        Current = this;
        Hide ();
    }

    public void SetPause ( bool paused, bool visible ) {
        Paused = paused;
        Visible = visible;
        GetTree ().Paused = paused;
        EmitSignal ( nameof ( pause_changed ), paused );
    }
}
