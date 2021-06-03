public struct OptionsProperty
{
    public string Name;
    public string ControllingNodePath;
    public string Value;

    public OptionsProperty ( string name, string controllingNodePath, string value ) {
        Name = name;
        ControllingNodePath = controllingNodePath;
        Value = value;
    }

    public OptionsProperty ( string controllingNodePath, string value ) {
        Name = null;
        ControllingNodePath = controllingNodePath;
        Value = value;
    }
}
