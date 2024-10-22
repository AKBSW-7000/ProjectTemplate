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

    public bool set_error(Error err, ErrorModule em, ErrorAction ea, string moreDetails = "")
    {
        set_station((int)em);
        return set_error((int)err, moreDetails, (int)ea);
    }
}
