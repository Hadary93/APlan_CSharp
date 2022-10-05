

namespace aplan.database
{

    /// <summary>
    /// Class <c>NetElement</c> models a cartesian coordinate for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class CartesianCoordinate
    {

        /// <value>
        /// Property <c>Id</c> represents the internal id used in database.
        /// </value>
        public int Id { get; set; }

        /// <value>
        /// Property <c>name</c> represents the internal name used in database.
        /// </value>
        public string name { get; set; }

        /// <value>
        /// Property <c>pad</c> represents the pad of respective object from import data.
        /// This property is found normally in mdb format.
        /// </value>
        public string pad { get; set; }

        /// <value>
        /// Property <c>index</c> represents the index of the coordinate which stored in polyline.
        /// This property is found normally in json format.
        /// </value>
        public int index { get; set; }

        /// <value>
        /// Property <c>x</c> represents x value in real geometry coordinate.
        /// </value>
        public double x { get; set; }

        /// <value>
        /// Property <c>y</c> represents y value in real geometry coordinate.
        /// </value>
        public double y { get; set; }

        /// <value>
        /// Property <c>z</c> represents z value in real geometry coordinate.
        /// </value>
        public double z { get; set; }
    }
}
