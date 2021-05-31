using Godot;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

public class OptionsManager : Menu
{
    private const string DIRECTORY = "{0}/Racquetball/";
    private const string FILENAME = "Options.json";
    private const string BUS_MASTER = "Master";
    private const string RESOLUTIONS = "640x480,800x600,960x720,1024x768,1280x960,1400x1050,1440x1080,1600x1200,1856x1392,1920x1440,2048x1536,1280x800,1440x900,1680x1050,1920x1200,2560x1600,1024x576,1152x648,1280x720,1366x768,1600x900,1920x1080,2560x1440,3840x2160";

    private readonly string saveDirectory = string.Format ( DIRECTORY, System.Environment.GetFolderPath ( System.Environment.SpecialFolder.ApplicationData ) );
    [Export] private readonly NodePath resolutionOptionButtonNodePath;
    private OptionButton resolutionOptionButton;
    private string saveFile;

    private static readonly Dictionary<string, OptionsProperty> properties = new Dictionary<string, OptionsProperty> () {
        { "Fullscreen", new OptionsProperty ( "Fullscreen", "Fullscreen Checkbox", "True" ) },
        { "Resolution", new OptionsProperty ( "Resolution", "Resolution Option Button", "0" ) },
        { "VolumeMaster", new OptionsProperty ( "VolumeMaster", "Master Volume Slider", "0" ) }
    };

    public static void Apply () {
        OS.WindowFullscreen = GetProperty<bool> ( "Fullscreen" );
        if ( !OS.WindowFullscreen )
            OS.WindowSize = GetResolution ( GetProperty<int> ( "Resolution" ) );
        AudioServer.SetBusVolumeDb ( AudioServer.GetBusIndex ( BUS_MASTER ), GetProperty<int> ( "VolumeMaster" ) );
    }

    public static T GetProperty<T> ( string name ) {
        if ( !properties.ContainsKey ( name ) )
            return default;
        return (T) System.Convert.ChangeType ( properties[ name ].Value, typeof ( T ) );
    }

    private static Vector2 GetResolution ( int index ) {
        string[] resolutionList = RESOLUTIONS.Split ( ',' );
        string[] resolution = resolutionList[ index ].Split ( 'x' );
        return new Vector2 ( float.Parse ( resolution[ 0 ] ), float.Parse ( resolution[ 1 ] ) );
    }

    private void Load () {
        if ( !System.IO.File.Exists ( saveFile ) )
            return;
        string json = System.IO.File.ReadAllText ( saveFile );
        OptionsProperty[] loadedProperties = JsonConvert.DeserializeObject<OptionsProperty[]> ( json );
        foreach ( OptionsProperty p in loadedProperties ) {
            var controllingNode = GetNode ( p.ControllingNodePath );
            switch ( controllingNode ) {
                case CheckBox checkBox:
                    checkBox.Pressed = bool.Parse ( p.Value );
                    break;
                case Slider slider:
                    slider.Value = int.Parse ( p.Value );
                    break;
                case OptionButton optionButton:
                    optionButton.Select ( int.Parse ( p.Value ) );
                    break;
            }
            SetProperty ( p.Value, p.Name, p.ControllingNodePath );
        }
    }

    private void LoadResolutions () {
        string[] resolutionList = RESOLUTIONS.Split ( ',' );
        foreach ( string resolution in resolutionList )
            resolutionOptionButton.AddItem ( resolution );
    }

    protected override void OnShow () {
        Load ();
    }

    protected override void _ReadyMenu () {
        resolutionOptionButton = GetNode<OptionButton> ( resolutionOptionButtonNodePath );
        saveFile = System.IO.Path.Combine ( saveDirectory, FILENAME );
        LoadResolutions ();
        Load ();
        Apply ();
    }

    public void SetProperty ( string value, string name, string controllingNodePath ) {
        if ( !properties.ContainsKey ( name ) )
            properties.Add ( name, default );
        properties[ name ] = new OptionsProperty ( name, controllingNodePath, value );
    }

    public void Save () {
        if ( !System.IO.Directory.Exists ( saveDirectory ) )
            System.IO.Directory.CreateDirectory ( saveDirectory );
        System.IO.File.WriteAllText ( saveFile, JsonConvert.SerializeObject ( properties.Values.ToArray (), Formatting.Indented ) );
        Apply ();
    }
}
