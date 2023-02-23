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

    /// <summary>
    /// Dictionary for all of our options, and their default values.
    /// </summary>
    private static readonly Dictionary<string, OptionsProperty> properties = new Dictionary<string, OptionsProperty> () {
        { "Fullscreen", new OptionsProperty ( "Fullscreen Checkbox", "True" ) },
        { "Resolution", new OptionsProperty ( "Resolution Option Button", "0" ) },
        { "VolumeMaster", new OptionsProperty ( "Master Volume Slider", "0" ) },
        { "MoveAimingId0", new OptionsProperty ( "Move Aiming P1 Checkbox", "False" ) },
        { "MoveAimingId1", new OptionsProperty ( "Move Aiming P2 Checkbox", "False" ) },
        { "SwapMoveId0", new OptionsProperty ( "Swap Move P1 Checkbox", "False" ) },
        { "SwapMoveId1", new OptionsProperty ( "Swap Move P2 Checkbox", "False" ) },
        { "Deadzone", new OptionsProperty ( "Deadzone Slider", "0.1" ) }
    };
    private static readonly Dictionary<string, OptionsProperty> tempProperties = new Dictionary<string, OptionsProperty> ();
    /// <summary>
    /// Update game settings based on the currently saved properties
    /// </summary>
    public static void ApplySettings () {
        OS.WindowFullscreen = GetProperty<bool> ( "Fullscreen" );
        if ( !OS.WindowFullscreen )
            OS.WindowSize = GetResolution ( GetProperty<int> ( "Resolution" ) );
        AudioServer.SetBusVolumeDb ( AudioServer.GetBusIndex ( BUS_MASTER ), GetProperty<int> ( "VolumeMaster" ) );
    }
    /// <summary>
    /// Saves all queued properties set by SetProperty to the main properties dictionary
    /// </summary>
    private static void ApplyTempProperties () {
        foreach ( var p in tempProperties ) {
            if ( !properties.ContainsKey ( p.Key ) )
                properties.Add ( p.Key, default );
            properties[ p.Key ] = p.Value;
        }
        tempProperties.Clear ();
    }

    public static OptionsProperty[] GetCurrentProperties () {
        return properties.Values.ToArray ();
    }
    /// <summary>
    /// Returns a saved property with a given type. Returns the default value of a given type if the specified property is not present
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="name"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Load all properties stored on file
    /// </summary>
    private void Load () {
        if ( !System.IO.File.Exists ( saveFile ) )
            return;
        string json = System.IO.File.ReadAllText ( saveFile );
        OptionsProperty[] loadedProperties = JsonConvert.DeserializeObject<OptionsProperty[]> ( json );
        foreach ( OptionsProperty p in loadedProperties )
            SetProperty ( p.Value, p.Name, p.ControllingNodePath );
        ApplyTempProperties ();
    }
    /// <summary>
    /// Load all resolutions from the RESOLUTIONS string into a given option button 
    /// </summary>
    private void LoadResolutions ( OptionButton optionButton ) {
        string[] resolutionList = RESOLUTIONS.Split ( ',' );
        foreach ( string resolution in resolutionList )
            optionButton.AddItem ( resolution );
    }

    protected override void OnShow () {
        UpdateControllingNodes ();
    }

    protected override void _ReadyMenu () {
        resolutionOptionButton = GetNode<OptionButton> ( resolutionOptionButtonNodePath );
        saveFile = System.IO.Path.Combine ( saveDirectory, FILENAME );
        LoadResolutions ( resolutionOptionButton );
        Load ();
        ApplySettings ();
    }
    /// <summary>
    /// Queues a property to be applied when calling the ApplyTempProperties method
    /// </summary>
    /// <param name="value"></param>
    /// <param name="name"></param>
    /// <param name="controllingNodePath"></param>
    public void SetProperty ( string value, string name, string controllingNodePath ) {
        if ( !tempProperties.ContainsKey ( name ) )
            tempProperties.Add ( name, default );
        tempProperties[ name ] = new OptionsProperty ( controllingNodePath, value );
    }
    /// <summary>
    /// Save the current properties dictionary to a file
    /// </summary>
    public void Save () {
        ApplyTempProperties ();
        if ( !System.IO.Directory.Exists ( saveDirectory ) )
            System.IO.Directory.CreateDirectory ( saveDirectory );
        // We don't specify names for default properties to make it easier to avoid typos, so we'll just use their dictionary key as their name instead
        List<OptionsProperty> propertiesToSave = new List<OptionsProperty> ();
        foreach ( var p in properties )
            propertiesToSave.Add ( new OptionsProperty ( p.Key, p.Value.ControllingNodePath, p.Value.Value ) );
        string json = JsonConvert.SerializeObject ( propertiesToSave, Formatting.Indented );
        System.IO.File.WriteAllText ( saveFile, json );
        ApplySettings ();
    }
    /// <summary>
    /// Updates all nodes used for controlling properties to their saved value
    /// </summary>
    private void UpdateControllingNodes () {
        foreach ( OptionsProperty p in GetCurrentProperties () ) {
            var controllingNode = GetNodeOrNull ( p.ControllingNodePath );
            if ( controllingNode != null ) {
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
            }
        }
    }
}
