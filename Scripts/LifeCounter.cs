using Godot;

public class LifeCounter : TextureRect
{
    [Signal] public delegate void out_of_life ();

    [Export] private int currentLife = 3;
    private Vector2 defaultPosition;

    private void CalculateScale () {
        Vector2 texSize = Texture.GetSize ();
        Vector2 newScale = texSize * currentLife * Vector2.Right;
        newScale.x = Mathf.Max ( newScale.x, texSize.x );
        newScale.y = Mathf.Max ( newScale.y, texSize.y );
        Vector2 newPosition = defaultPosition - texSize * RectScale * ( currentLife - 1 ) * Vector2.Right / 2.0f;
        RectSize = newScale;
        RectPosition = newPosition;
    }

    public void ModifyLife ( int amount ) {
        currentLife = Mathf.Max ( 0, currentLife + amount );
        bool alive = currentLife > 0;
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
