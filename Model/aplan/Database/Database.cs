using LiteDB;

namespace aplan.core
{

    /// <summary>
    /// Class <c>Database</c> models an internal database of APlan.
    /// </summary>
    public class Database
    {

        // instance
        private static Database databaseInstance;

        // singleton constructor
        private Database() { }

        // singleton method
        /// <summary>
        /// Get eulynx service instance
        /// </summary>
        /// <param name="paths"></param>
        public static Database getInstance()
        {
            if (databaseInstance == null)
            {
                databaseInstance = new Database();
            }
            return databaseInstance;
        }


        /// <summary>
        /// Method to access internal database.
        /// </summary>
        public LiteDatabase accessDB()
        {
            return new LiteDatabase(@"..\..\..\resources\APlan.db");
        }


        /// <summary>
        /// Method to clear all records in internal database.
        /// </summary>
        /// <param name="database">internal database</param>
        public void clearDB(LiteDatabase database)
        {
            using (database)
            {
                //database.DropCollection("NetElements");
                //database.DropCollection("Nodes");
                //database.DropCollection("HorizontalAlignments");
                //database.DropCollection("VerticalAlignments");
                //database.DropCollection("AlignmentCants");
                //database.DropCollection("Mileage");
                //all collection
                //database.Dispose();
              
            }
        }

    }
}
