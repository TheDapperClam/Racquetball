using Godot;
using Godot.Collections;

/// <summary>
/// Class for our on screen keyboard.
/// </summary>
public class Keyboard : GridContainer
{
    [Export] private readonly NodePath nameEntryNodePath;
    [Export] private Vector2 keySize = new Vector2 ( 64.0f, 64.0f );
    private NameEntry nameEntry;
    private Button[,] buttons;

    /// <summary>
    /// Function for connecting our keyboard keys for navigation.
    /// </summary>
    private void ConnectButtons () {
        for ( int y = 0; y < buttons.GetLength ( 0 ); y++ ) {
            for ( int x = 0; x < buttons.GetLength ( 1 ); x++ ) {
                Button currentButton = GetButton ( x, y );
                Button topNeighbour = GetButton ( x, y - 1 );
                Button bottomNeightbour = GetButton (x, y + 1 );
                Button leftNeighbour = GetButton ( x - 1, y );
                Button rightNeighbour = GetButton ( x + 1, y );
                if ( topNeighbour != null )
                    currentButton.FocusNeighbourTop = topNeighbour.GetPath ();
                if ( bottomNeightbour != null )
                    currentButton.FocusNeighbourBottom = bottomNeightbour.GetPath ();
                if ( leftNeighbour != null )
                    currentButton.FocusNeighbourLeft = leftNeighbour.GetPath ();
                if ( rightNeighbour != null )
                    currentButton.FocusNeighbourRight = rightNeighbour.GetPath ();
            }
        }
    }

    /// <summary>
    /// Function for when our keyboard has gained focus.
    /// </summary>
    private void FocusEntered () {
        // Once the entire keyboard has gained focus, focus on the first key.
        GetButton ( 0, 0 ).GrabFocus ();
    }

    /// <summary>
    /// Function for getting a keyboard key based on its index.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private Button GetButton ( int x, int y ) {
        if ( x < 0 || x >= buttons.GetLength ( 1 ) || y < 0 || y >= buttons.GetLength ( 0 ) )
            return null;
        return buttons[ y, x ];
    }

    public override void _Ready () {
        Connect ( "focus_entered", this, nameof ( FocusEntered ) );
        FocusMode = FocusModeEnum.All;
        KeyboardKey[] keys = KeyboardLayouts.Default;
        buttons = new Button[ Mathf.CeilToInt ( keys.Length / Columns ), Columns ];
        nameEntry = GetNode<NameEntry> ( nameEntryNodePath );
        // Now we fill our keyboard with our list of keys.
        for ( int i = 0; i < keys.Length; i++ ) {
            KeyboardKey currentKey = keys[ i ];
            Button newButton = new Button {
                RectMinSize = keySize,
                Text = currentKey.Text,
                Name = currentKey.Text + " Key",
            };
            buttons[ i / Columns, i % Columns ] = newButton;
            newButton.Shortcut = new ShortCut () { Shortcut = new InputEventKey () { Scancode = currentKey.Hotkey } };
            newButton.Connect ( "pressed", nameEntry, nameof ( nameEntry.Enter ), new Array { currentKey.Text } );
            AddChild ( newButton );
        }
        ConnectButtons ();
    }
}
