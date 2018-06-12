using System;

namespace Util
{
    //Autor: Hugo C. Machado
    public enum ButtonEnum
    {
        NewGame, LoadGame, SaveGame, Undo, Redo, File1, File2, File3, Menu
    };

    public class HelperButtonEnum
    {
        public static string ButtonEnumToString(ButtonEnum e)
        {
            switch(e)
            {
                case ButtonEnum.NewGame: 
                    return "NewGame";
    
                case ButtonEnum.LoadGame:
                    return "LoadGame";
    
                case ButtonEnum.SaveGame:
                    return "SaveGame";
    
                case ButtonEnum.Undo:
                    return "Undo";
    
                case ButtonEnum.Redo:
                    return "Redo";
    
                case ButtonEnum.File1:
                    return "File1";
    
                case ButtonEnum.File2:
                    return "File2";
    
                case ButtonEnum.File3:
                    return "File3";

                case ButtonEnum.Menu:
                    return "Menu";
    
                default:
                    return "";
            }
        }
    };
}

