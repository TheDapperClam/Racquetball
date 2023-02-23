using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Class for managing the high scores for our game.
/// </summary>
public class HighScores : Node
{
    private const string HIGH_SCORE_FORMAT = "{0} - {1}";
    private const string DEFAULT_INITIALS = "___";
    private const int DEFAULT_SCORE = 0;
    private const int MAX_HIGH_SCORES = 5;

    private static string baseDirectory = "{0}/Racquetball/";
    [Export] private readonly NodePath scoreNodePath;
    [Export] private readonly string filename;
    private Score score;

    /// <summary>
    /// Function for creating the directory of our save file.
    /// </summary>
    private static void CreateDirectory () {
        baseDirectory = string.Format ( baseDirectory, System.Environment.GetFolderPath ( System.Environment.SpecialFolder.ApplicationData ) );
        if ( System.IO.Directory.Exists ( baseDirectory ) )
            return;
        System.IO.Directory.CreateDirectory ( baseDirectory );
    }

    /// <summary>
    /// Function for loading our previously loaded save file.
    /// </summary>
    /// <returns>List of scores</returns>
    public List<string> Load () {
        return Load ( filename );
    }

    /// <summary>
    /// Function for loading our save file
    /// </summary>
    /// <param name="filename"></param>
    /// <returns>List of scores</returns>
    public static List<string> Load ( string filename ) {
        CreateDirectory ();
        string path = System.IO.Path.Combine ( baseDirectory, filename );
        string defaultHighScore = string.Format ( HIGH_SCORE_FORMAT, DEFAULT_INITIALS, DEFAULT_SCORE );
        List<string> highScores = Enumerable.Repeat ( defaultHighScore, MAX_HIGH_SCORES ).ToList ();
        if ( !System.IO.File.Exists ( path ) )
            return highScores;
        using ( StreamReader reader = new StreamReader ( path ) ) {
            int index = -1;
            while ( !reader.EndOfStream ) {
                index++;
                string line = reader.ReadLine ();
                if ( index >= highScores.Count )
                    // Our save file contains more scores than we though, so extend the list.
                    highScores.Add ( line );
                else
                    highScores[ index ] = line;
            }
            return highScores;
        }
    }

    /// <summary>
    /// Parse a score in string format.
    /// </summary>
    /// <param name="score"></param>
    /// <returns>Array of score data</returns>
    public static Array ParseScore ( string score ) {
        string[] data = score.Split ( " - " );
        return new Array () { data[ 0 ], int.Parse ( data[ 1 ] ) };
    }

    public override void _Ready () {
        score = GetNodeOrNull<Score> ( scoreNodePath );
    }

    /// <summary>
    /// Function for adding our current score to the list of high scores.
    /// </summary>
    /// <param name="initials"></param>
    public void Save ( string initials = DEFAULT_INITIALS ) {
        if ( score == null )
            return;
        CreateDirectory ();
        string path = System.IO.Path.Combine ( baseDirectory, filename );
        List<string> highScores = Load ( path );
        Score scoreNode = GetNodeOrNull<Score> ( scoreNodePath );
        int insertionPoint = -1;
        if ( scoreNode == null )
            return;
        // We want to insert our new score based on where it ranks compared to high scores.
        for ( int i = highScores.Count - 1; i >= 0; i-- ) {
            Array scoreData = ParseScore ( highScores[ i ] );
            int oldScore = (int) scoreData[ 1 ];
            if ( scoreNode.Points > oldScore )
                insertionPoint = i;
            else
                break;
        }
        if ( insertionPoint < 0 )
            // The new score hasn't beaten any current ones.
            return;
        highScores.Insert ( insertionPoint, string.Format ( HIGH_SCORE_FORMAT, initials, scoreNode.Points ) );
        // We want to remove any scores that are now out or range of our top scores.
        highScores.RemoveRange ( MAX_HIGH_SCORES, highScores.Count - MAX_HIGH_SCORES );
        using ( StreamWriter writer = new StreamWriter ( path ) ) {
            foreach ( string newHighScore in highScores )
                writer.WriteLine ( newHighScore );
        }
    }
}
