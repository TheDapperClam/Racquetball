using Godot;

public class Player : Respawnable
{
    private const float DIE_SHAKE_INTENSITY = 3.0f;
    private const float DIE_SHAKE_TIME = 0.1f;
    private const string MOVE_UP_ACTION = "Move_Up";
    private const string MOVE_DOWN_ACTION = "Move_Down";
    private const string MOVE_LEFT_ACTION = "Move_Left";
    private const string MOVE_RIGHT_ACTION = "Move_Right";
    private const string AIM_UP_ACTION = "Aim_Up";
    private const string AIM_DOWN_ACTION = "Aim_Down";
    private const string AIM_LEFT_ACTION = "Aim_Left";
    private const string AIM_RIGHT_ACTION = "Aim_Right";
    private const string SWING_ACTION = "Fire1";
    private const string DIE_ANIMATION = "Die";

    [Signal] public delegate void OnDieBegin ();
    [Signal] public delegate void OnDie ();
    public float MoveSpeed = 60.0f;

    [Export] private readonly int inputId;
    [Export] private readonly NodePath racketNodePath;
    [Export] private readonly NodePath cursorNodePath;
    private Racket racket;
    private Cursor cursor;
    private float aimUp;
    private float aimDown;
    private float aimLeft;
    private float aimRight;
    private float moveUp;
    private float moveDown;
    private float moveLeft;
    private float moveRight;
    private float deadzone;
    private bool joyConnected;
    private bool moveAiming;

    private void CheckJoyConnection ( int device ) {
        bool connected = device - GameMode.Current.MaxPlayers + Input.GetConnectedJoypads ().Count >= 0;
        if ( device == inputId ) {
            joyConnected = connected;
            if ( !moveAiming )
                cursor.OrbitalMode = connected;
        }
    }

    public async void Die ( float horizontalDirection ) {
        if ( isDead )
            return;
        isDead = true;
        EmitSignal ( nameof ( OnDieBegin ) );
        float newDirectionSign = horizontalDirection > 0.0f ? 1.0f : -1.0f;
        float newHorizontalScale = Mathf.Ceil ( Mathf.Abs ( horizontalDirection ) ) * newDirectionSign;
        Scale = new Vector2 ( newHorizontalScale, 1.0f );
        Feedback.Current.ScreenShake ( DIE_SHAKE_INTENSITY, DIE_SHAKE_TIME, newDirectionSign * Mathf.Pi );
        animationPlayer.Play ( DIE_ANIMATION );
        await ToSignal ( animationPlayer, "animation_finished" );
        EmitSignal ( nameof ( OnDie ) );
    }

    public override void _Input ( InputEvent @event ) {
        int device = @event.Device;
        var deviceType = @event.GetType ();
        bool isJoypad = deviceType != typeof ( InputEventKey ) && deviceType != typeof ( InputEventMouseButton );
        if ( isJoypad && Input.GetConnectedJoypads ().Count < GameMode.Current.MaxPlayers )
            device++;
        if ( inputId != device )
            return;

        // We check if the game is paused before swinging so that the player doesn't swing the same frame after the game unpauses
        if ( @event.IsActionPressed ( SWING_ACTION ) && !GetTree ().Paused && !isDead )
            racket.BufferSwing ();

        string aimUpTarget = moveAiming ? MOVE_UP_ACTION : AIM_UP_ACTION;
        string aimDownTarget = moveAiming ? MOVE_DOWN_ACTION : AIM_DOWN_ACTION;
        string aimLeftTarget = moveAiming ? MOVE_LEFT_ACTION : AIM_LEFT_ACTION;
        string aimRightTarget = moveAiming ? MOVE_RIGHT_ACTION : AIM_RIGHT_ACTION;
        if ( @event.IsAction ( aimUpTarget ) )
            aimUp = @event.GetActionStrength ( aimUpTarget );
        if ( @event.IsAction ( aimDownTarget ) )
            aimDown = @event.GetActionStrength ( aimDownTarget );
        if ( @event.IsAction ( aimLeftTarget ) )
            aimLeft = @event.GetActionStrength ( aimLeftTarget );
        if ( @event.IsAction ( aimRightTarget ) )
            aimRight = @event.GetActionStrength ( aimRightTarget );

        if ( @event.IsAction ( MOVE_UP_ACTION ) )
            moveUp = @event.GetActionStrength ( MOVE_UP_ACTION );
        if ( @event.IsAction ( MOVE_DOWN_ACTION ) )
            moveDown = @event.GetActionStrength ( MOVE_DOWN_ACTION );
        if ( @event.IsAction ( MOVE_LEFT_ACTION ) )
            moveLeft = @event.GetActionStrength ( MOVE_LEFT_ACTION );
        if ( @event.IsAction ( MOVE_RIGHT_ACTION ) )
            moveRight = @event.GetActionStrength ( MOVE_RIGHT_ACTION );
    }

    private void JoyConnectionChanged ( int device, bool connected ) {
        CheckJoyConnection ( device );
    }

    public override void _Process ( float delta ) {
        if ( GetTree ().Paused || isDead )
            return;
        Vector2 movement = new Vector2 ( moveRight - moveLeft, moveDown - moveUp );
        Vector2 aimAnalog = new Vector2 ( aimRight - aimLeft, aimDown - aimUp );
        GD.Print ( aimAnalog );
        if ( !cursor.OrbitalMode && inputId == 0 ) {
            Vector2 mousePos = GetGlobalMousePosition ();
            Vector2 mouseDir = mousePos - GlobalPosition;
            racket.Aim ( mouseDir );
        } else if ( aimAnalog.Length () > deadzone )
            racket.Aim ( aimAnalog );
        if ( movement.Length () > deadzone )
            MoveAndSlide ( movement.Normalized () * MoveSpeed );
    }

    public override void Respawn () {
        racket.Cancel ();
        base.Respawn ();
    }

    protected override void _RespawnableReady () {
        moveAiming = OptionsManager.GetProperty<bool> ( string.Format ( "MoveAimingId{0}", inputId ) );
        deadzone = OptionsManager.GetProperty<float> ( "Deadzone" );
        racket = GetNode<Racket> ( racketNodePath );
        cursor = GetNode<Cursor> ( cursorNodePath );
        cursor.InputId = inputId;
        cursor.OrbitalMode = moveAiming;
        Input.Singleton.Connect ( "joy_connection_changed", this, nameof ( JoyConnectionChanged ) );
        CheckJoyConnection ( inputId );
        GD.Print ( Name + " starting animation is " + startingAnimation );
    }
}
