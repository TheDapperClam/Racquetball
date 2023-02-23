using Godot;

/// <summary>
/// Class for handling a player's health
/// </summary>
public class LifeCounter : TextureRect
{
    [Signal] public delegate void out_of_life ();
    [Export] public int CurrentLife { get; private set; } = 3;

    private Vector2 defaultPosition;

    /// <summary>
    /// Function for calculating and setting the clipping rectangle of the life counter.
    /// </summary>
    private void CalculateScale () {
        Vector2 texSize = Texture.GetSize ();
        Vector2 newScale = texSize * CurrentLife * Vector2.Right;
        newScale.x = Mathf.Max ( newScale.x, texSize.x );
        newScale.y = Mathf.Max ( newScale.y, texSize.y );
        // We want the life counter to remain centered relative to its starting position and current life count.
        Vector2 newPosition = defaultPosition - texSize * RectScale * ( CurrentLife - 1 ) * Vector2.Right / 2.0f;
        RectSize = newScale;
        RectPosition = newPosition;
    }

    /// <summary>
    /// Function for adjusting the current life of the counter
    /// </summary>
    /// <param name="amount"></param>
    public void ModifyLife ( int amount ) {
        CurrentLife = Mathf.Max ( 0, CurrentLife + amount );
        bool alive = CurrentLife > 0;
        Visible = alive;
        CalculateScale ();
        if ( !alive )
            EmitSignal ( nameof ( out_of_life ) );
    }

    public override void _Ready () {
        defaultPosition = RectPosition;
        CalculateScale ();
    }
}
