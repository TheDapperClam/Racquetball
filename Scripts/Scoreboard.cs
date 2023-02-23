using Godot;
using System.Collections.Generic;

/// <summary>
/// Class for handling our scoreboard menu UI logic.
/// </summary>
public class Scoreboard : Menu
{
    [Export] private readonly NodePath labelNodePath;
    [Export] private readonly string FileName;
    private Label label;

    /// <summary>
    /// Function for loading, and showing our saved list of high scores.
    /// </summary>
    /// <param name="filename"></param>
    private void Populate ( string filename ) {
        label.Text = "";
        List<string> scores = HighScores.Load ( filename );
        for ( int i = 0; i < scores.Count; i++ ) {
            label.Text += scores[ i ];
            if ( i < scores.Count )
                label.Text += '\n';
        }
    }

    protected override void _ReadyMenu () {
        Hide ();
        label = GetNode<Label> ( labelNodePath );
        Populate ( FileName );
    }
}
