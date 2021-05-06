using Godot;

public class Ball : KinematicBody2D
{
    private const float MINIMUM_BOUNCE_SPEED = 1.0f;

    [Signal] public delegate void OnBodyEntered ();
    [Export] public bool Served { get; private set; }
    public Vector2 Velocity;

    [Export] private readonly NodePath collisionShapeNodePath;
    [Export] private readonly NodePath cameraNodePath;
    [Export] private readonly float bounceScreenShakeMultiplier = 0.1f;
    private CollisionShape2D collisionShape;
    private Camera2D camera;

    public override void _Process ( float delta ) {
        KinematicCollision2D collision = MoveAndCollide ( Velocity * delta * 100.0f );
        if ( collision != null ) {
            EmitSignal ( nameof ( OnBodyEntered ) );
            SetDirection ( Velocity.Bounce ( collision.Normal ).Normalized () );
            Rotation = collision.Normal.Angle ();
            Feedback.Current.ScreenShake ( Velocity.Length () * bounceScreenShakeMultiplier, 0.1f, collision.Normal.Angle () );
            if ( collision.Collider.GetType () == typeof ( Player ) )
                ( (Player) collision.Collider ).Die ( collision.Normal.x );
        }
    }

    public override void _Ready () {
        collisionShape = GetNode<CollisionShape2D> ( collisionShapeNodePath );
        camera = GetNode<Camera2D> ( cameraNodePath );
        if ( GetParent ().GetType () == typeof ( Player ) )
            SetCollision ( false );
    }

    public void Serve ( Vector2 position ) {
        Node newParent = GetTree ().CurrentScene;
        GetParent ().RemoveChild ( this );
        newParent.AddChild ( this );
        Owner = newParent;
        GlobalPosition = position;
        SetCollision ( true );
    }

    private void SetCollision ( bool collide ) {
        collisionShape.Disabled = !collide;
    }

    public void SetDirection ( Vector2 direction, float addedSpeed = 0.0f ) {
        Translate ( direction );
        Velocity = direction * ( Mathf.Max ( Velocity.Length () + addedSpeed, MINIMUM_BOUNCE_SPEED ) );
        Served = true;
    }
}
