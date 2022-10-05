

namespace aplan.database
{

    /// <summary>
    /// Class <c>AssociatedNetElement</c> models an associated net element for internal used within the project.
    /// This is modeled based on the needs of keeping imported information,
    /// which later will be used for calculation and mapping.
    /// </summary>
    class AssociatedNetElement
    {

        /// <value>
        /// Property <c>uuid</c> represents the uuid of eulynx object.
        /// This property helps the program to identify the connection between eulynx object and internal object.
        /// </value>
        public string uuid { get; set; }

        /// <value>
        /// Property <c>netElementId</c> represents uuid of net element which being associated.
        /// </value>
        public string netElementId { get; set; }

        /// <value>
        /// Property <c>appliesInDirection</c> represents application direction of the object which associates to this net element.
        /// </value>
        public int appliesInDirection { get; set; }

        /// <value>
        /// Property <c>intrinsicValue</c> represents intrinsic coordinate of an object along the net element.
        /// </value>
        public double intrinsicValue { get; set; }

        /// <value>
        /// Property <c>lateralSide</c> represents side whereas an object is located.
        /// </value>
        public string lateralSide { get; set; }
    }
}
