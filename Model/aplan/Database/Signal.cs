using System.Collections.Generic;


namespace aplan.database
{

    /// <summary>
    /// Class <c>Signal</c> models a signal for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class Signal
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
        /// Property <c>signalType</c> represents the type of the signal.
        /// This could be varied depends on domain country.
        /// </value>
        public string signalType { get; set; }

        /// <value>
        /// Property <c>signalFunction</c> represents the function of the signal.
        /// This could be varied depends on domain country.
        /// </value>
        public string signalFunction { get; set; }

        /// <value>
        /// Property <c>fixingType</c> represents the type of the signal fixing.
        /// This could be varied depends on domain country.
        /// </value>
        public string fixingType { get; set; }

        /// <value>
        /// Property <c>km</c> represents the kilometer value of the object.
        /// </value>
        public double km { get; set; }

        /// <value>
        /// Property <c>associatedNetElements</c> represents the list of associated element which the object has.
        /// </value>
        public List<AssociatedNetElement> associatedNetElements { get; set; }

        #endregion

        #region eulynx related attributes

        /// <value>
        /// Property <c>signalFrameId</c> represents the id of eulynx signal frame.
        /// </value>
        public string signalFrameId { get; set; }

        /// <value>
        /// Property <c>signalFixingId</c> represents the id of eulynx signal fixing.
        /// </value>
        public string signalFixingId { get; set; }

        /// <value>
        /// Property <c>signalFunctionId</c> represents the id of eulynx signal function.
        /// </value>
        public string signalFunctionId { get; set; }

        /// <value>
        /// Property <c>signalTypeId</c> represents the id of eulynx signal type.
        /// </value>
        public string signalTypeId { get; set; }

        /// <value>
        /// Property <c>signalRsmId</c> represents the id of rsm signal.
        /// </value>
        public string signalRsmId { get; set; }

        #endregion




    }
}
