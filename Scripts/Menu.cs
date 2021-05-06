using Godot;

public class Menu : Control
{
    [Export] public bool AllowPause;

    [Export] protected NodePath joypadStartNodeNodePath;
    protected static Menu currentMenu { get; private set; }
    protected Control joypadStartNode;
    private Menu previousMenu;

    protected void ChangeScene ( PackedScene scene ) {
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
        } else {
            if ( previousMenu != null )
                previousMenu.CheckIfShouldFocus ();
            currentMenu = previousMenu;
            OnHide ();
        }
    }
}
