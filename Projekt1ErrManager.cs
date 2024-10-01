using AKBUtilities;

namespace Projekt1;

public class Projekt1ErrManager:ErrorManager
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="FilePath">json filePath for errors, errorActions and errorStations</param>
    public Projekt1ErrManager(string FilePath):base()
    {
        set_dictionaries(FilePath);
    }
}
