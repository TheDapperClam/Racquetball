using Godot;
using System.Collections.Generic;

/// <summary>
/// Class for handling our high score entry menu UI logic.
/// </summary>
public class ScoreEntryMenu : Menu
{
    [Export] private readonly NodePath highScoresNodePath;
    [Export] private readonly NodePath nameLabelNodePath;
    [Export] private readonly NodePath scoreNodePath;
    private HighScores highScores;
    private Label nameLabel;
    private Score score;

    /// <summary>
    /// Function for saving our new high score.
    /// </summary>
    public void Confirm () {
        highScores.Save ( nameLabel.Text );
        Hide ();
    }

    protected override void OnHide () {
        PauseMenu.Current.SetPause ( false, false );
    }

    protected override void OnShow () {
        PauseMenu.Current.SetPause ( true, false );
    }

    protected override void _ReadyMenu () {
        Hide ();
        highScores = GetNode<HighScores> ( highScoresNodePath );
        nameLabel = GetNode<Label> ( nameLabelNodePath );
        score = GetNode<Score> ( scoreNodePath );
    }

    /// <summary>
    /// Function for showing our high score entry menu if a previous score was beaten.
    /// </summary>
    public void ShowIfNewHighScore () {
        List<string> scores = highScores.Load ();
        int lowestScore = (int) HighScores.ParseScore ( scores[ scores.Count - 1 ] )[ 1 ];
        if ( score.Points > lowestScore )
            Show ();
    }
}
