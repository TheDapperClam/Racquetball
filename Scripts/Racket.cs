using Godot;

public class Racket : Area2D
{
    private const float ANGLE_OFFSET = Mathf.Pi * -0.5f;
    private const string SWING_RIGHT_ANIMATION = "SwingTo_Right";
    private const string SWING_LEFT_ANIMATION = "SwingTo_Left";

    [Signal] public delegate void OnBallServed ();
    [Signal] public delegate void OnBallHit ();

    [Export] private readonly NodePath animationPlayerNodePath;
    [Export] private readonly NodePath ballNodePath;
    [Export] private readonly NodePath raycastNodePath;
    [Export] private readonly NodePath cameraNodePath;
    [Export] private readonly float swingCooldown = 0.2f;
    [Export] private readonly float serveOffset = 12.0f;
    [Export] private readonly float swingShakeIntensity = 0.5f;
    [Export] private readonly float hitPauseTime = 0.1f;
    [Export] private readonly float ballHitPower = 0.025f;
    private AnimationPlayer animationPlayer;
    private Ball ballToServe;
    private RayCast2D raycast;
    private Camera2D camera;
    private bool isSwinging;
    private bool reverseSwing;
    private bool ballHit;
    private bool cancelSwing;
    private float swingTime;
    private Vector2 swingDirection = Vector2.Down;

    /// <summary>
    /// Function for aiming our racket in a given direction.
    /// </summary>
    /// <param name="direction"></param>
    public void Aim ( Vector2 direction ) {
        Rotation = direction.Angle () + ANGLE_OFFSET;
        swingDirection = direction.Normalized ();
    }

    /// <summary>
    /// Function for when a physics body enters our rackets hitbox.
    /// </summary>
    /// <param name="body"></param>
    private void BodyEntered ( Node body ) {
        if ( !cancelSwing && !ballHit && body.GetType () == typeof ( Ball ) ) {
            ballHit = true;
            EmitSignal ( ( (Ball) body ).Served ? nameof ( OnBallHit ) : nameof ( OnBallServed ) );
            Feedback.Current.Pause ( hitPauseTime );
            ( (Ball) body ).SetDirection ( swingDirection, ballHitPower );
        }
    }

    /// <summary>
    /// Function for when we want to buffer a swing input.
    /// </summary>
    public void BufferSwing () {
        if ( GetTime () > swingTime ) {
            ballHit = false;
            isSwinging = true;
        }
    }

    /// <summary>
    /// Function for when we want to cancel a swing.
    /// </summary>
    public void Cancel () {
        cancelSwing = true;
        ballHit = false;
    }

    /// <summary>
    /// Function for getting the current time in seconds.
    /// </summary>
    /// <returns></returns>
    private float GetTime () {
        float time = OS.GetTicksMsec () / 1000.0f;
        return time;
    }

    public override void _Process ( float delta ) {
        Swing ();
    }

    public override void _Ready () {
        animationPlayer = GetNode<AnimationPlayer> ( animationPlayerNodePath );
        ballToServe = GetNodeOrNull<Ball> ( ballNodePath );
        raycast = GetNode<RayCast2D> ( raycastNodePath );
        camera = GetNode<Camera2D> ( cameraNodePath );
        raycast.CastTo = Vector2.Down * serveOffset;
        raycast.Enabled = true;
        animationPlayer.PlaybackSpeed = 1.0f / swingCooldown;
        Monitoring = false;
    }

    /// <summary>
    /// Function for swinging our racket.
    /// </summary>
    private void Swing () {
        if ( !isSwinging )
            return;
        if ( ballToServe != null && !ballToServe.Served ) {
            Vector2 servePoint = raycast.IsColliding () ? raycast.GetCollisionPoint () : GlobalPosition + swingDirection * serveOffset;
            // If the ball node is our child, we don't want its movement to be affected by our movement.
            ballToServe.SetAsToplevel ( true );
            ballToServe.GlobalPosition = servePoint;
        }
        float time = GetTime ();
        animationPlayer.Play ( reverseSwing ? SWING_LEFT_ANIMATION : SWING_RIGHT_ANIMATION );
        reverseSwing = !reverseSwing;
        swingTime = time + swingCooldown;
        Feedback.Current.ScreenShake ( swingShakeIntensity, 0.1f );
        isSwinging = false;
        cancelSwing = false;
    }
}
