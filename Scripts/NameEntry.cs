using Godot;
using System.Text;

public class NameEntry : Label
{
    private const int LENGTH = 3;
    private const char UNDO_CHAR = '<';
    private const char BLANK_CHAR = '_';

    private readonly StringBuilder sb = new StringBuilder ( new string ( BLANK_CHAR, LENGTH ), LENGTH );
    private int enterPosition;

    public void Enter ( string character ) {
        char newChar = character[ 0 ];
        bool undoEntered = newChar == UNDO_CHAR;
        if ( undoEntered && enterPosition > 0 ) {
            enterPosition--;
            sb[ enterPosition ] = BLANK_CHAR;
        } else if ( !undoEntered && enterPosition < LENGTH ) {
            sb[ enterPosition ] = newChar;
            enterPosition++;
        }
        Text = sb.ToString ();
    }

    public override void _Ready () {
        Text = sb.ToString ();
    }
}
