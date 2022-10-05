using System.Collections;

namespace aplan.database
{

    /// <summary>
    /// Class <c>NetElement</c> models a node for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class Node
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
        /// Property <c>name</c> represents the internal name used in database.
        /// </value>
        public string name { get; set; }

        /// <value>
        /// Property <c>pad</c> represents the pad of respective object from import data.
        /// This property is used to help identifying exact geo coordinate and matching with other properties.
        /// </value>
        public string pad { get; set; }

        /// <value>
        /// Property <c>parseNumber</c> represents the number of which the object belongs to
        /// in odrder to classify the group of geometry features.
        /// This property is found particulary in mdb format.
        /// </value>
        public int parseNumber { get; set; }

        /// <value>
        /// Property <c>codeDirection</c> represents the direction of the object.
        /// Whether is it normal, reverse, or both.
        /// </value>
        public int codeDirection { get; set; }

        /// <value>
        /// Property <c>originNodeID</c> represents the id of previous node which is connected to the object.
        /// </value>
        public string originNodeID { get; set; }

        /// <value>
        /// Property <c>destinationNodeID</c> represents the list of destination node's id which is connected to the object.
        /// </value>
        public ArrayList destinationNodeID { get; set; }

        /// <value>
        /// Property <c>km</c> represents the kilometer value of the object.
        /// </value>
        public double? km { get; set; }

        /// <value>
        /// Property <c>nodeType</c> represents the type of object.
        /// Whether it is switch or bufferstop
        /// </value>
        public int nodeType { get; set; }

        /// <value>
        /// Property <c>geometryType</c> represents the geometry type of the object.
        /// This property is found particularly in json format.
        /// </value>
        public string geometryType { get; set; } // json specifi attribute (?)

        /// <value>
        /// Property <c>coordinate</c> represents the geo coordinate of the object.
        /// </value>
        public CartesianCoordinate coordinate { get; set; }

    }
}
