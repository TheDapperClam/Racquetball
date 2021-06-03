using Godot;

public class Menu : Control
{
    [Signal] public delegate void on_show ();
    [Signal] public delegate void on_hide ();

    [Export] public bool AllowPause;

    [Export] protected NodePath joypadStartNodeNodePath;
    protected static Menu currentMenu { get; private set; }
    protected Control joypadStartNode;
    private Menu previousMenu;

    protected void ChangeScene ( PackedScene scene ) {
        // Since changing to a different scene takes a bit of time, we'll also clean up any garbage that we currently have
        System.GC.Collect ();
        GetTree ().ChangeSceneTo ( scene );
    }

    protected void CheckIfShouldFocus ( bool keepPreviousOwner = false ) {
        if ( !Visible )
            return;
        if ( Input.GetConnectedJoypads ().Count <= 0 )
            return;
        if ( keepPreviousOwner && GetFocusOwner () != null )
            return;
        joypadStartNode.GrabFocus ();
    }

    protected void JoyConnectionChanged ( int device, bool connected ) {
        CheckIfShouldFocus ( true );
    }

    protected virtual void OnHide () {

    }

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
    /// Ready function used for menus to avoid overriding important menu code
    /// </summary>
    protected virtual void _ReadyMenu () {

    }

    public void Quit () {
        GetTree ().Quit ();
    }

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
