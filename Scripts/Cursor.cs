using Godot;

/// <summary>
/// Class for our racket aiming cursor.
/// </summary>
public class Cursor : Node2D
{
    public bool OrbitalMode;
    public int InputId;

    [Export] private readonly NodePath aimNodeNodePath;
    private Node2D aimNode;
    private Vector2 orbitalPosition;

    /// <summary>
    /// Function for when our game's pause state is changed.
    /// </summary>
    /// <param name="paused"></param>
    private void PauseChanged ( bool paused ) {
        Input.SetMouseMode ( paused ? Input.MouseMode.Visible : Input.MouseMode.Hidden );
    }

    public override void _EnterTree () {
        Input.SetMouseMode ( Input.MouseMode.Hidden );
    }

    public override void _ExitTree () {
        Input.SetMouseMode ( Input.MouseMode.Visible );
    }

    public override void _Process ( float delta ) {
        if ( !OrbitalMode && InputId == 0 ) {
            // Orbital mode is disabled, so we want our custom cursor to just follow the mouse cursor.
            Vector2 mousePos = GetGlobalMousePosition ();
            GlobalPosition = mousePos;
        } else
            // Orbital mode is enabled, so have it orbit around the player
            Position = orbitalPosition;
        if ( aimNode != null )
            // Have our cursor look at the player, so that the arrow points in the direction that the player is aiming. 
            LookAt ( aimNode.GlobalPosition );
    }

    public override void _Ready () {
        aimNode = GetNodeOrNull<Node2D> ( aimNodeNodePath );
        orbitalPosition = Position;
        PauseMenu.Current.Connect ( "pause_changed", this, nameof ( PauseChanged ) );
    }
}
