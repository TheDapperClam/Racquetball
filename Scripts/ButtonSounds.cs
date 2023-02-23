using Godot;

/// <summary>
/// Class for handling sound effects for our UI buttons.
/// </summary>
public class ButtonSounds : Node
{
    private readonly AudioStreamPlayer pressedSound = new AudioStreamPlayer () {
        Stream = GD.Load<AudioStream> ( "res://Sounds/confirm.wav" )
    };
    private readonly AudioStreamPlayer hoverSound = new AudioStreamPlayer () {
        Stream = GD.Load<AudioStream> ( "res://Sounds/beep.wav" ),
        VolumeDb = -20.0f
    };

    /// <summary>
    /// Function for connecting a button to our sound effects.
    /// </summary>
    /// <param name="node"></param>
    private void ConnectButton ( Node node ) {
        if ( node.GetType () != typeof( Button ) )
            return;
        node.Connect ( "pressed", pressedSound, "play" );
        node.Connect ( "focus_entered", hoverSound, "play" );
    }

    /// <summary>
    /// Function for connecting a parent button and its child buttons to our sound effects.
    /// </summary>
    /// <param name="parent"></param>
    private void ConnectParentAndChildButtons ( Node parent ) {
        foreach ( Node node in parent.GetChildren () ) {
            ConnectButton ( node );
            ConnectParentAndChildButtons ( node );
        }
    }

    public override void _Ready () {
        // We want our sounds to play even when the game is paused.
        PauseMode = PauseModeEnum.Process;
        AddChild ( pressedSound );
        AddChild ( hoverSound );
        // We want to go through the entire scene tree, and connect every UI button to our sound effects.
        ConnectParentAndChildButtons ( GetTree ().Root );
        // Connect any new buttons added to the scene to our sound effects.
        GetTree ().Connect ( "node_added", this, nameof ( ConnectButton ) );
    }
}
