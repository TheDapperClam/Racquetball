using Godot;

public class Player : Respawnable
{
    private const float DIE_SHAKE_INTENSITY = 3.0f;
    private const float DIE_SHAKE_TIME = 0.1f;
    private const string LEFT_STICK_UP_ACTION = "Move_Up";
    private const string LEFT_STICK_DOWN_ACTION = "Move_Down";
    private const string LEFT_STICK_LEFT_ACTION = "Move_Left";
    private const string LEFT_STICK_RIGHT_ACTION = "Move_Right";
    private const string RIGHT_STICK_UP_ACTION = "Aim_Up";
    private const string RIGHT_STICK_DOWN_ACTION = "Aim_Down";
    private const string RIGHT_STICK_LEFT_ACTION = "Aim_Left";
    private const string RIGHT_STICK_RIGHT_ACTION = "Aim_Right";
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
    private string moveUpAction = LEFT_STICK_UP_ACTION;
    private string moveDownAction = LEFT_STICK_DOWN_ACTION;
    private string moveLeftAction = LEFT_STICK_LEFT_ACTION;
    private string moveRightAction = LEFT_STICK_RIGHT_ACTION;
    private string aimUpAction = RIGHT_STICK_UP_ACTION;
    private string aimDownAction = RIGHT_STICK_DOWN_ACTION;
    private string aimLeftAction = RIGHT_STICK_LEFT_ACTION;
    private string aimRightAction = RIGHT_STICK_RIGHT_ACTION;
    private bool joyConnected;
    private bool swapMovement;
    private bool moveAiming;

    private void CheckJoyConnection ( int device ) {
        bool connected = device - GameMode.Current.MaxPlayers + Input.GetConnectedJoypads ().Count >= 0;
        if ( device == inputId ) {
            joyConnected = connected;
            if ( !moveAiming )
                cursor.OrbitalMode = connected;
        }
        UpdateMovementActions ();
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

        if ( @event.IsAction ( aimUpAction ) )
            aimUp = @event.GetActionStrength ( aimUpAction );
        if ( @event.IsAction ( aimDownAction ) )
            aimDown = @event.GetActionStrength ( aimDownAction );
        if ( @event.IsAction ( aimLeftAction ) )
            aimLeft = @event.GetActionStrength ( aimLeftAction );
        if ( @event.IsAction ( aimRightAction ) )
            aimRight = @event.GetActionStrength ( aimRightAction );

        if ( @event.IsAction ( moveUpAction ) )
            moveUp = @event.GetActionStrength ( moveUpAction );
        if ( @event.IsAction ( moveDownAction ) )
            moveDown = @event.GetActionStrength ( moveDownAction );
        if ( @event.IsAction ( moveLeftAction ) )
            moveLeft = @event.GetActionStrength ( moveLeftAction );
        if ( @event.IsAction ( moveRightAction ) )
            moveRight = @event.GetActionStrength ( moveRightAction );
    }

    private void JoyConnectionChanged ( int device, bool connected ) {
        CheckJoyConnection ( device );
    }

    public override void _Process ( float delta ) {
        if ( GetTree ().Paused || isDead )
            return;
        Vector2 movement = new Vector2 ( moveRight - moveLeft, moveDown - moveUp );
        Vector2 aimAnalog = new Vector2 ( aimRight - aimLeft, aimDown - aimUp );
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
        swapMovement = OptionsManager.GetProperty<bool> ( string.Format ( "SwapMoveId{0}", inputId ) );
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

    private void UpdateMovementActions () {
        if ( swapMovement ) {
            moveUpAction = joyConnected ? RIGHT_STICK_UP_ACTION : LEFT_STICK_UP_ACTION;
            moveDownAction = joyConnected ? RIGHT_STICK_DOWN_ACTION : LEFT_STICK_DOWN_ACTION;
            moveLeftAction = joyConnected ? RIGHT_STICK_LEFT_ACTION : LEFT_STICK_LEFT_ACTION;
            moveRightAction = joyConnected ? RIGHT_STICK_RIGHT_ACTION : LEFT_STICK_RIGHT_ACTION;
        }
        if ( moveAiming ) {
            aimUpAction = moveUpAction;
            aimDownAction = moveDownAction;
            aimLeftAction = moveLeftAction;
            aimRightAction = moveRightAction;
        } else if ( swapMovement ) {
            aimUpAction = joyConnected ? LEFT_STICK_UP_ACTION : RIGHT_STICK_UP_ACTION;
            aimDownAction = joyConnected ? LEFT_STICK_DOWN_ACTION : RIGHT_STICK_DOWN_ACTION;
            aimLeftAction = joyConnected ? LEFT_STICK_LEFT_ACTION : RIGHT_STICK_LEFT_ACTION;
            aimRightAction = joyConnected ? LEFT_STICK_RIGHT_ACTION : RIGHT_STICK_RIGHT_ACTION;
        }
    }
}
