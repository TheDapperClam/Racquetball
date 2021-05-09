using Godot;

public class Score : Label
{
    public int Points { get; private set; }

    public void AddPoints ( int amount ) {
        Points += Mathf.Max ( amount, 0 );
        UpdateText ();
    }

    public void Reset () {
        Points = 0;
        UpdateText ();
    }

    private void UpdateText () {
        Text = Points.ToString ();
    }
}
