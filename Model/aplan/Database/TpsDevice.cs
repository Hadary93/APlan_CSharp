using System.Collections.Generic;


namespace aplan.database
{

    /// <summary>
    /// Class <c>TpsDevice</c> models a tps device for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class TpsDevice
    {

        /// <value>
        /// Property <c>Id</c> represents the internal id used in database.
        /// </value>
        public int Id { get; set; }

        #region rsm related attributes

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
        /// Property <c>deviceType</c> represents the tps device type.
        /// </value>
        public string deviceType { get; set; }

        /// <value>
        /// Property <c>deviceFunction</c> represents the tps device function.
        /// </value>
        public string deviceFunction { get; set; }

        /// <value>
        /// Property <c>km</c> represents kilometer value of the object.
        /// </value>
        public double km { get; set; }

        /// <value>
        /// Property <c>associatedNetElements</c> represents the list of associated element which the object has.
        /// </value>
        public List<AssociatedNetElement> associatedNetElements { get; set; }

        #endregion

        // eulynx related attributes
    }
}
