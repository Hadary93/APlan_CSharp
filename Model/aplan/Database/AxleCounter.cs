using System.Collections.Generic;


namespace aplan.database
{

    /// <summary>
    /// Class <c>AxleCounter</c> models an axle counter for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class AxleCounter
    {

        /// <value>
        /// Property <c>Id</c> represents the internal id used in database.
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// Property <c>uuid</c> represents the uuid of eulynx object.
        /// This property helps the program to identify the connection between eulynx object and internal object.
        /// </value>
        public string uuid { get; set; }

        /// <value>
        /// Property <c>name</c> represents the internal name used in database.
        /// </value>
        public string name { get; set; }

        /// <value>
        /// Property <c>longname</c> represents the internal longname used in database.
        /// </value>
        public string longname { get; set; }

        /// <value>
        /// Property <c>km</c> represents kilometer value of the object.
        /// </value>
        public double km { get; set; }

        /// <value>
        /// Property <c>associatedNetElements</c> represents the list of associated element which the object has.
        /// </value>
        public List<AssociatedNetElement> associatedNetElements { get; set; }
    }
}
