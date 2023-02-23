using Godot;

/// <summary>
/// Class used for gameplay feedback
/// </summary>
public class Feedback : Node
{
    public static Feedback Current { get; private set; }

    private static readonly RandomNumberGenerator rng = new RandomNumberGenerator ();

    [Export] private readonly NodePath cameraNodePath;
    private Camera2D camera;
    private readonly Timer unpauseTimer = new Timer ();
    private readonly Timer unshakeTimer = new Timer ();

    /// <summary>
    /// Function for freezing the game for a given time.
    /// </summary>
    /// <param name="duration"></param>
    public void Pause ( float duration ) {
        StartTimer ( unpauseTimer, duration );
        GetTree ().Paused = true;
    }

    public override void _Ready () {
        Current = this;
        PauseMode = PauseModeEnum.Process;
        camera = GetNode<Camera2D> ( cameraNodePath );
        unpauseTimer.Connect ( "timeout", this, nameof ( UnpauseTimerTimeout ) );
        unshakeTimer.Connect ( "timeout", this, nameof ( UnshakeTimerTimeout ) );
        AddChild ( unpauseTimer );
        AddChild ( unshakeTimer );
    }

    /// <summary>
    /// Function used for jolting the screen in a direction.
    /// </summary>
    /// <param name="intensity"></param>
    /// <param name="duration"></param>
    /// <param name="overrideAngle"></param>
    public void ScreenShake ( float intensity, float duration, float overrideAngle = float.MinValue ) {
        StartTimer ( unshakeTimer, duration );
        rng.Randomize ();
        float angle = overrideAngle == float.MinValue ? rng.RandfRange ( 0.0f, 1.0f ) * Mathf.Pi : overrideAngle;
        Vector2 direction = new Vector2 ( Mathf.Cos ( angle ), Mathf.Sin ( angle ) );
        camera.Offset = direction * intensity;
    }

    /// <summary>
    /// Function for setting up and starting a given timer.
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="duration"></param>
    private void StartTimer ( Timer timer, float duration ) {
        timer.Stop ();
        timer.WaitTime = duration;
        timer.Start ();
    }

    /// <summary>
    /// Function for when our unpause timer goes off.
    /// </summary>
    private void UnpauseTimerTimeout () {
        if ( !PauseMenu.Current.Paused )
            GetTree ().Paused = false;
    }

    /// <summary>
    /// Function for when our unshake timer goes off.
    /// </summary>
    private void UnshakeTimerTimeout () {
        camera.Offset = Vector2.Zero;
    }
}
