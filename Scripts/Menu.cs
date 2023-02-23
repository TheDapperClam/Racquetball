using Godot;

/// <summary>
/// Class for our UI menus.
/// </summary>
public class Menu : Control
{
    [Signal] public delegate void on_show ();
    [Signal] public delegate void on_hide ();

    [Export] public bool AllowPause;

    [Export] protected NodePath joypadStartNodeNodePath;
    protected static Menu currentMenu { get; private set; }
    protected Control joypadStartNode;
    private Menu previousMenu;

    /// <summary>
    /// Function for collecting garbage, and then changing the scene.
    /// </summary>
    /// <param name="scene"></param>
    protected void ChangeScene ( PackedScene scene ) {
        System.GC.Collect ();
        GetTree ().ChangeSceneTo ( scene );
    }

    /// <summary>
    /// Function for checking if this menu should be focused, and then doing so if yes.
    /// </summary>
    /// <param name="keepPreviousOwner"></param>
    protected void CheckIfShouldFocus ( bool keepPreviousOwner = false ) {
        if ( !Visible )
            return;
        if ( Input.GetConnectedJoypads ().Count <= 0 )
            return;
        if ( keepPreviousOwner && GetFocusOwner () != null )
            return;
        joypadStartNode.GrabFocus ();
    }

    /// <summary>
    /// Function for when a gamepad is connected or disconnected.
    /// </summary>
    /// <param name="device"></param>
    /// <param name="connected"></param>
    protected void JoyConnectionChanged ( int device, bool connected ) {
        CheckIfShouldFocus ( true );
    }

    /// <summary>
    /// Function for when our menu is hidden.
    /// </summary>
    protected virtual void OnHide () {

    }

    /// <summary>
    /// Function for when our menu is shown.
    /// </summary>
    protected virtual void OnShow () {

    }

    public override void _ExitTree () {
        if ( currentMenu == this )
            currentMenu = null;
    }

    public sealed override void _Ready () {
        joypadStartNode = GetNode<Control> ( joypadStartNodeNodePath );
        Connect ( "visibility_changed", this, nameof ( VisibilityChanged ) );
        Input.Singleton.Connect ( "joy_connection_changed", this, nameof ( JoyConnectionChanged ) );
        _ReadyMenu ();
    }

    /// <summary>
    /// Ready function used for menus to avoid overriding important menu code.
    /// </summary>
    protected virtual void _ReadyMenu () {

    }

    /// <summary>
    /// Function for quitting the game.
    /// </summary>
    public void Quit () {
        GetTree ().Quit ();
    }

    /// <summary>
    /// Function for when the visability of our menu changes
    /// </summary>
    protected void VisibilityChanged () {
        if ( Visible ) {
            previousMenu = currentMenu;
            currentMenu = this;
            CheckIfShouldFocus ();
            OnShow ();
            EmitSignal ( nameof ( on_show ) );
        } else {
            if ( previousMenu != null )
                previousMenu.CheckIfShouldFocus ();
            currentMenu = previousMenu;
            OnHide ();
            EmitSignal ( nameof ( on_hide ) );
        }
    }
}
