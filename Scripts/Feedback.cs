using Godot;

public class Feedback : Node
{
    public static Feedback Current { get; private set; }

    private static readonly RandomNumberGenerator rng = new RandomNumberGenerator ();

    [Export] private readonly NodePath cameraNodePath;
    private Camera2D camera;
    private readonly Timer unpauseTimer = new Timer ();
    private readonly Timer unshakeTimer = new Timer ();

    public void Pause ( float duration ) {
        unpauseTimer.Stop ();
        unpauseTimer.WaitTime = duration;
        unpauseTimer.Start ();
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

    public void ScreenShake ( float intensity, float duration, float overrideAngle = float.MinValue ) {
        unshakeTimer.Stop ();
        unshakeTimer.WaitTime = duration;
        unshakeTimer.Start ();
        rng.Randomize ();
        float angle = overrideAngle == float.MinValue ? rng.RandfRange ( 0.0f, 1.0f ) * Mathf.Pi : overrideAngle;
        Vector2 direction = new Vector2 ( Mathf.Cos ( angle ), Mathf.Sin ( angle ) );
        camera.Offset = direction * intensity;
    }

    private void UnpauseTimerTimeout () {
        if ( !PauseMenu.Current.Paused )
            GetTree ().Paused = false;
    }

    private void UnshakeTimerTimeout () {
        camera.Offset = Vector2.Zero;
    }
}
