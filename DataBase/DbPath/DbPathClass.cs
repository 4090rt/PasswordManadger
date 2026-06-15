namespace Messangers.SQLite.DbPath
{
    public class DbPathClass
    {
        public string dbpath()
        {
            string projectDirectory = System.IO.Directory.GetCurrentDirectory();
            string dbPath = System.IO.Path.Combine(projectDirectory, "DataBase.db");
            return dbPath;
        }
    }
}
