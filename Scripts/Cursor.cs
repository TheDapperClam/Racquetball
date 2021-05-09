using Godot;

public class Cursor : Node2D
{
    public bool OrbitalMode;
    public int InputId;

    [Export] private readonly NodePath aimNodeNodePath;
    private Node2D aimNode;
    private Vector2 orbitalPosition;

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
            Vector2 mousePos = GetGlobalMousePosition ();
            GlobalPosition = mousePos;
        } else
            Position = orbitalPosition;
        if ( aimNode != null )
            LookAt ( aimNode.GlobalPosition );
    }

    public override void _Ready () {
        aimNode = GetNodeOrNull<Node2D> ( aimNodeNodePath );
        orbitalPosition = Position;
        PauseMenu.Current.Connect ( "pause_changed", this, nameof ( PauseChanged ) );
    }
}
