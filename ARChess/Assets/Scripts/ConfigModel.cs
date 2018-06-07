using System;

public enum Selection
{
    VirtualButton, Time 
};

public class ConfigModel
{
    public static SharpChess.Model.Player.PlayerColourNames _player = SharpChess.Model.PlayerWhite.PlayerColourNames.White;
    public static int _difficulty = 0;
    public static bool _sound = true;
    public static bool _ar = true;
    public static bool _showLegalMoves = true;
    public static Selection _selection = Selection.VirtualButton;

    public ConfigModel()
    {
    }
};
