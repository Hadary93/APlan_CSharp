using Models.TopoModels.EULYNX.generic;

using static aplan.core.Helper;
using aplan.core;

namespace aplan.eulynx
{
    class DbEulynxObjectsBuilder : EulynxObjectsBuilder
    {

        /// <summary>
        /// Get eulynx container
        /// </summary>
        public EulynxDataPrepInterface getEulynxDP()
        {
            return eulynx;
        }

        /// <summary>
        /// Build eulynx container along with its rsm entities and signalling entities
        /// </summary>
        public override void buildContainer()
        {
            eulynx = new EulynxDataPrepInterface { id = generateUUID() };
            RsmEntities rsmEntities = new RsmEntities { id = generateUUID() };
            DataPrepEntities dataPrepEntities = new DataPrepEntities() { id = generateUUID() };
            eulynx.hasDataContainer.Add(new DataContainer
            {
                id = generateUUID(),
                ownsRsmEntities = rsmEntities,
                ownsDataPrepEntities = dataPrepEntities
            });

        }

        /// <summary>
        /// Build general objects
        /// </summary>
        public override void buildGeneralObjects()
        {
            EulynxCommonBuilder.addUnit(eulynx, "meter", "m");
            EulynxCommonBuilder.addUnit(eulynx, "kilometer", "km");
            EulynxCommonBuilder.addUnit(eulynx, "per-mille", "‰");
            EulynxCommonBuilder.addUnit(eulynx, "gon", "gon");
        }

        /// <summary>
        /// Build objects that related to positioning topic
        /// </summary>
        /// <param name="database"></param>
        public override void buildPositioningObjects(Database database)
        {

            // instantiate linear coordinates appeared in the data
            EulynxPositioningBuilder.addLinearPositioningSystem(eulynx, database);
            EulynxPositioningBuilder.addLinearCoordinate(eulynx, database);

            // instantiate cartesian coordinates appeared in the data
            EulynxPositioningBuilder.addGeometricPositioningSystem(eulynx);
            EulynxPositioningBuilder.addCartesianCoordinate(eulynx, database);

            // add polyline to linear positioning system
            EulynxPositioningBuilder.addPolyLineToLinearPositioningSystem(eulynx, database);


        }

        /// <summary>
        /// Build objects that related to topology
        /// </summary>
        /// <param name="db"></param>
        public override void buildTopologyObjects(Database db)
        {
            // instantiate topology aspects appeared in the data
            EulynxTopologyBuilder.addLinearElement(eulynx, db);
            EulynxTopologyBuilder.addRelations(eulynx, db);
            //EulynxTopologyBuilder.addMileage(eulynx, paths["mileage"]);
            // missing linearelement? -> handled before mapping, once all of net element store to database

            //Console.WriteLine("linear elements and its relation have been generated as an object . . .");

        }

        /// <summary>
        /// Build objects that related to topology
        /// </summary>
        /// <param name="db"></param>
        public override void buildTopographyObjects(Database db)
        {
            // instantiate topography aspects appeared in the data
            EulynxTopographyBuilder.addHorizontalAlignment(eulynx, db);
            EulynxTopographyBuilder.addVerticalAlignment(eulynx, db);
            EulynxTopographyBuilder.addAlignmentCant(eulynx, db);
            EulynxTopographyBuilder.addAlignmentCurve(eulynx, db);

            //Console.WriteLine("alignments have been generated as an object . . .");

        }

        /// <summary>
        /// Build objects that related to topology
        /// </summary>
        /// <param name="db"></param>
        public override void buildInfrastructureObjects(Database db)
        {
            // instantiate infrastructure aspects appeared in the data
            EulynxInfrastructureBuilder.addTurnouts(eulynx, db);
            EulynxInfrastructureBuilder.addVehicleStops(eulynx, db);

            //Console.WriteLine("net entities have been generated as an object . . .");

        }


    }
}
