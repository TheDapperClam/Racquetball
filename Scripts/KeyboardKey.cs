public struct KeyboardKey
{
    public string Text;
    public uint Hotkey;

    public KeyboardKey ( string text, uint hotkey ) {
        Text = text;
        Hotkey = hotkey;
    }
}
