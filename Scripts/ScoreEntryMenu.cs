using Godot;
using System.Collections.Generic;

public class ScoreEntryMenu : Menu
{
    [Export] private readonly NodePath highScoresNodePath;
    [Export] private readonly NodePath nameLabelNodePath;
    [Export] private readonly NodePath scoreNodePath;
    private HighScores highScores;
    private Label nameLabel;
    private Score score;

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

    public void ShowIfNewHighScore () {
        List<string> scores = highScores.Load ();
        int lowestScore = (int) HighScores.ParseScore ( scores[ scores.Count - 1 ] )[ 1 ];
        if ( score.Points > lowestScore )
            Show ();
    }
}
