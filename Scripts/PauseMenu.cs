using Godot;

/// <summary>
/// Class for handling our pause menu UI logic.
/// </summary>
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

    /// <summary>
    /// Function for returning to the main menu.
    /// </summary>
    public void LoadMainMenu () {
        SetPause ( false, false );
        ChangeScene ( mainMenuScene );
    }

    protected override void _ReadyMenu () {
        Current = this;
        Hide ();
    }

    /// <summary>
    /// Function for setting the game's pause state
    /// </summary>
    /// <param name="paused"></param>
    /// <param name="visible"></param>
    public void SetPause ( bool paused, bool visible ) {
        Paused = paused;
        Visible = visible;
        GetTree ().Paused = paused;
        EmitSignal ( nameof ( pause_changed ), paused );
    }
}
