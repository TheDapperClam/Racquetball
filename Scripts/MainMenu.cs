using Godot;

/// <summary>
/// Class for handling our main menu UI logic.
/// </summary>
public class MainMenu : Menu
{
    [Export] private readonly PackedScene singlePlayerScene;
    [Export] private readonly PackedScene battleScene;

    /// <summary>
    /// Function for loading the multiplayer battle mode.
    /// </summary>
    public void LoadBattleMode () {
        ChangeScene ( battleScene );
    }

    /// <summary>
    /// Function for loading the single player mode.
    /// </summary>
    public void LoadSinglePlayerMode () {
        ChangeScene ( singlePlayerScene );
    }
}
