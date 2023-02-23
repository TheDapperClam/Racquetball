using Godot;

/// <summary>
/// Class for storing, and managing our game score.
/// </summary>
public class Score : Label
{
    public int Points { get; private set; }

    /// <summary>
    /// Function for adding points to our current score.
    /// </summary>
    /// <param name="amount"></param>
    public void AddPoints ( int amount ) {
        Points += Mathf.Max ( amount, 0 );
        UpdateText ();
    }

    /// <summary>
    /// Function for resetting our score to 0.
    /// </summary>
    public void Reset () {
        Points = 0;
        UpdateText ();
    }

    /// <summary>
    /// Function for updating our UI label to reflect our current score.
    /// </summary>
    private void UpdateText () {
        Text = Points.ToString ();
    }
}
