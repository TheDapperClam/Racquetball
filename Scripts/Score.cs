using Godot;

public class Score : Label
{
    public int Points { get; private set; }

    public void AddPoints ( int amount ) {
        Points += amount;
        Text = Points.ToString ();
    }
}
