using aplan.core;

namespace aplan.eulynx
{
    class EulynxDirector
    {
        //instance
        private static EulynxDirector eulynxDirector;
        private EulynxObjectsBuilder builder;

        //singleton constructor
        private EulynxDirector() { }

        //singleton method
        /// <summary>
        /// Get eulynx director instance
        /// </summary>
        public static EulynxDirector getInstance()
        {
            if (eulynxDirector == null)
            {
                eulynxDirector = new EulynxDirector();
            }
            return eulynxDirector;
        }

        /// <summary>
        /// Set eulynx object builder
        /// </summary>
        public void setBuilder(EulynxObjectsBuilder builder)
        {
            this.builder = builder;
        }

        /// <summary>
        /// Construct rsm entities for json format
        /// </summary>
        /// <param name="database"></param>
        public void constructRsmEntitiesJson(Database database)
        {
            builder.buildContainer();
            builder.buildGeneralObjects();
            builder.buildPositioningObjects(database);
            builder.buildTopologyObjects(database);
            builder.buildTopographyObjects(database);
            builder.buildInfrastructureObjects(database);
        }
    }
}
