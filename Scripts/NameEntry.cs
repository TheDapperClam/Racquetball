using Godot;
using System.Text;

/// <summary>
/// Class for our high score name entry label.
/// </summary>
public class NameEntry : Label
{
    private const int LENGTH = 3;
    private const char UNDO_CHAR = '<';
    private const char BLANK_CHAR = '_';

    private readonly StringBuilder sb = new StringBuilder ();
    private int enterPosition;

    /// <summary>
    /// Function for adjusting our entered name.
    /// </summary>
    /// <param name="character"></param>
    public void Enter ( string character ) {
        char newChar = character[ 0 ];
        bool undoEntered = newChar == UNDO_CHAR;
        if ( undoEntered && enterPosition > 0 ) {
            // The undo character has been entered, so delete the previous character.
            enterPosition--;
            sb[ enterPosition ] = BLANK_CHAR;
        } else if ( !undoEntered && enterPosition < LENGTH ) {
            sb[ enterPosition ] = newChar;
            enterPosition++;
        }
        Text = sb.ToString ();
    }

    public override void _Ready () {
        Reset ();
        Connect ( "visibility_changed", this, nameof ( Reset ) );
    }

    /// <summary>
    /// Function for resetting our name to empty.
    /// </summary>
    private void Reset () {
        enterPosition = 0;
        sb.Clear ();
        sb.Append ( BLANK_CHAR, LENGTH );
        Text = sb.ToString ();
    }
}
