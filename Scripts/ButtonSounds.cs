using Godot;

public class ButtonSounds : Node
{
    private readonly AudioStreamPlayer pressedSound = new AudioStreamPlayer () {
        Stream = GD.Load<AudioStream> ( "res://Sounds/confirm.wav" )
    };
    private readonly AudioStreamPlayer hoverSound = new AudioStreamPlayer () {
        Stream = GD.Load<AudioStream> ( "res://Sounds/beep.wav" ),
        VolumeDb = -20.0f
    };

    private void ConnectButton ( Node node ) {
        if ( node.GetType () != typeof( Button ) )
            return;
        node.Connect ( "pressed", pressedSound, "play" );
        node.Connect ( "focus_entered", hoverSound, "play" );
    }

    private void ConnectParentAndChildButtons ( Node parent ) {
        foreach ( Node node in parent.GetChildren () ) {
            ConnectButton ( node );
            ConnectParentAndChildButtons ( node );
        }
    }

    public override void _Ready () {
        PauseMode = PauseModeEnum.Process;
        AddChild ( pressedSound );
        AddChild ( hoverSound );
        ConnectParentAndChildButtons ( GetTree ().Root );
        GetTree ().Connect ( "node_added", this, nameof ( ConnectButton ) );
    }
}
