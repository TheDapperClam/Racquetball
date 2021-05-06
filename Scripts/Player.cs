using Godot;

public class Player : KinematicBody2D
{
    private const float AIM_DEADZONE = 0.5f;
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
    [Export] private readonly NodePath animationPlayerNodePath;
    private Racket racket;
    private Cursor cursor;
    private AnimationPlayer animationPlayer;
    private float aimUp;
    private float aimDown;
    private float aimLeft;
    private float aimRight;
    private float moveUp;
    private float moveDown;
    private float moveLeft;
    private float moveRight;
    private bool joyConnected;
    private bool isDead;

    private void CheckJoyConnection ( int device ) {
        bool connected = device - GameMode.Current.MaxPlayers + Input.GetConnectedJoypads ().Count >= 0;
        if ( device == inputId ) {
            joyConnected = connected;
            cursor.OrbitalMode = connected;
        }
    }

    public async void Die ( float horizontalDirection ) {
        isDead = true;
        EmitSignal ( nameof ( OnDieBegin ) );
        animationPlayer.Play ( DIE_ANIMATION );
        float newDirectionSign = horizontalDirection > 0.0f ? 1.0f : -1.0f;
        float newHorizontalScale = Mathf.Ceil ( Mathf.Abs ( horizontalDirection ) ) * newDirectionSign;
        Scale = new Vector2 ( newHorizontalScale, 1.0f );
        Feedback.Current.ScreenShake ( DIE_SHAKE_INTENSITY, DIE_SHAKE_TIME, newDirectionSign * Mathf.Pi );
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

        if ( @event.IsAction ( AIM_UP_ACTION ) )
            aimUp = @event.GetActionStrength ( AIM_UP_ACTION );
        if ( @event.IsAction ( AIM_DOWN_ACTION ) )
            aimDown = @event.GetActionStrength ( AIM_DOWN_ACTION );
        if ( @event.IsAction ( AIM_LEFT_ACTION ) )
            aimLeft = @event.GetActionStrength ( AIM_LEFT_ACTION );
        if ( @event.IsAction ( AIM_RIGHT_ACTION ) )
            aimRight = @event.GetActionStrength ( AIM_RIGHT_ACTION );

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
        Vector2 movement = new Vector2 ( moveRight - moveLeft, moveDown - moveUp ).Normalized ();
        Vector2 aimAnalog = new Vector2 ( aimRight - aimLeft, aimDown - aimUp );
        if ( !joyConnected && inputId == 0 ) {
            Vector2 mousePos = GetGlobalMousePosition ();
            Vector2 mouseDir = mousePos - GlobalPosition;
            racket.Aim ( mouseDir );
        } else if ( aimAnalog.Length () > AIM_DEADZONE )
            racket.Aim ( aimAnalog );
        MoveAndSlide ( movement * MoveSpeed );
    }

    public override void _Ready () {
        racket = GetNode<Racket> ( racketNodePath );
        cursor = GetNode<Cursor> ( cursorNodePath );
        animationPlayer = GetNode<AnimationPlayer> ( animationPlayerNodePath );
        cursor.InputId = inputId;
        Input.Singleton.Connect ( "joy_connection_changed", this, nameof ( JoyConnectionChanged ) );
        CheckJoyConnection ( inputId );
    }
}
