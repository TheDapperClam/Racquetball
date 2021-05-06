using Godot;

public class MainMenu : Menu
{
    [Export] private readonly PackedScene singlePlayerScene;
    [Export] private readonly PackedScene battleScene;

    public void LoadBattleMode () {
        ChangeScene ( battleScene );
    }

    public void LoadSinglePlayerMode () {
        ChangeScene ( singlePlayerScene );
    }
}
