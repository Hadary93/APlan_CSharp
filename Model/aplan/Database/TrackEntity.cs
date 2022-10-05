

namespace aplan.database
{

    /// <summary>
    /// Class <c>TrackEntity</c> models a track entity for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class TrackEntity
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
        /// Property <c>trackEntityObject</c> represents the track entity object.
        /// </value>
        public string trackEntityObject { get; set; }

        /// <value>
        /// Property <c>trackEntityObjectElement</c> represents the track entity object element.
        /// </value>
        public string trackEntityObjectElement { get; set; }

        /// <value>
        /// Property <c>km</c> represents kilometer value of the object.
        /// </value>
        public double km { get; set; }

        // eulynx related attributes
        
    }
}
