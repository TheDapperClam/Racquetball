using Godot;

public class Ball : Respawnable
{
    private const float MINIMUM_BOUNCE_SPEED = 1.0f;
    private const string COLLIDE_ANIMATION = "Squish";
    private const int SERVED_MASK = 3;
    private const int PLAYER_DEAD_MASK = 1;
    private const float BOUNCE_STEP_ANGLE = Mathf.Pi / 2.0f;

    [Signal] public delegate void OnBodyEntered ();
    public bool Served { get; private set; }
    public Vector2 Velocity;

    [Export] private readonly NodePath cameraNodePath;
    [Export] private readonly float bounceScreenShakeMultiplier = 0.1f;
    private Camera2D camera;

    public override void _Process ( float delta ) {
        if ( !Served )
            return;
        KinematicCollision2D collision = MoveAndCollide ( Velocity * delta * 100.0f );
        if ( collision != null ) {
            EmitSignal ( nameof ( OnBodyEntered ) );
            SetDirection ( Velocity.Bounce ( collision.Normal ).Normalized () );
            Rotation = Mathf.Stepify ( collision.Normal.Angle (), BOUNCE_STEP_ANGLE );
            Feedback.Current.ScreenShake ( Velocity.Length () * bounceScreenShakeMultiplier, 0.1f, collision.Normal.Angle () );
            animationPlayer.Play ( COLLIDE_ANIMATION );
            if ( collision.Collider.GetType () == typeof ( Player ) ) {
                CollisionMask = PLAYER_DEAD_MASK;
                ( (Player) collision.Collider ).Die ( collision.Normal.x );
            }
        }
    }

    public override void Respawn () {
        base.Respawn ();
        Velocity = Vector2.Zero;
        Served = false;
    }

    protected override void _RespawnableReady () {
        camera = GetNode<Camera2D> ( cameraNodePath );
    }

    public void Serve () {
        CollisionMask = SERVED_MASK;
        Served = true;
    }

    public void SetDirection ( Vector2 direction, float addedSpeed = 0.0f ) {
        Serve ();
        Translate ( direction );
        Velocity = direction * ( Mathf.Max ( Velocity.Length () + addedSpeed, MINIMUM_BOUNCE_SPEED ) );
    }
}
