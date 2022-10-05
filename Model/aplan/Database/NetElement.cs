using System.Collections.Generic;


namespace aplan.database
{
    /// <summary>
    /// Class <c>NetElement</c> models a net element for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class NetElement
    {
        
        /// <value>
        /// Property <c>Id</c> represents the internal id used in database.
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// Property <c>id</c> represents the id of the object.
        /// </value>
        public string id { get; set; }

        /// <value>
        /// Property <c>uuid</c> represents the uuid of eulynx object.
        /// This property helps the program to identify the connection between eulynx object and internal object.
        /// </value>
        public string uuid { get; set; }

        /// <value>
        /// Property <c>startPad</c> represents the start pad of respective object from import data.
        /// This property is used to help identifying exact geo coordinate and matching with other properties.
        /// </value>
        public string startPad { get; set; }

        /// <value>
        /// Property <c>endPad</c> represents the end pad of respective object from import data.
        /// This property is used to help identifying exact geo coordinate and matching with other properties.
        /// </value>
        public string endPad { get; set; }

        /// <value>
        /// Property <c>parseNumber</c> represents the number of which the object belongs to
        /// in odrder to classify the group of geometry features.
        /// This property is found particulary in mdb format.
        /// </value>
        public int parseNumber { get; set; }

        /// <value>
        /// Property <c>name</c> represents the internal name used in database.
        /// </value>
        public string name { get; set; }

        /// <value>
        /// Property <c>name</c> represents the status of the object.
        /// Whether is it active or out of service.
        /// </value>
        public string status { get; set; }

        /// <value>
        /// Property <c>startNodeId</c> represents the start node id of the object.
        /// </value>
        public string startNodeId { get; set; }

        /// <value>
        /// Property <c>startNodeId</c> represents the start node id of the object.
        /// </value>
        public string endNodeId { get; set; }

        /// <value>
        /// Property <c>trackType</c> represents the type of the track.
        /// Whether is it main or side one.
        /// </value>
        public string trackType { get; set; }

        /// <value>
        /// Property <c>length</c> represents the length of the object.
        /// </value>
        public double length { get; set; }

        /// <value>
        /// Property <c>codeDirection</c> represents the direction of the object.
        /// Whether is it normal, reverse, or both.
        /// </value>
        public int codeDirection{ get; set; }

        /// <value>
        /// Property <c>startKm</c> represents the start kilometer of the object.
        /// </value>
        public double? startKm { get; set; }

        /// <value>
        /// Property <c>startKm</c> represents the end kilometer of the object.
        /// </value>
        public double? endKm { get; set; }

        /// <value>
        /// Property <c>startKm</c> represents the geometry type of the object.
        /// This property is found particularly in json format.
        /// </value>
        public string geometryType { get; set; }

        /// <value>
        /// Property <c>startKm</c> represents the geo coordinates of the object.
        /// </value>
        public List<CartesianCoordinate> lineCoordinates { get; set; }

    }
}
