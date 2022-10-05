using Models.TopoModels.EULYNX.generic;
using System;
using aplan.core;

namespace aplan.eulynx
{
    public sealed class EulynxService : IModelService
    {

        //instance
        private static EulynxService eulynxService;

        //singleton constructor
        private EulynxService() { }

        //singleton method
        /// <summary>
        /// Get eulynx service instance
        /// </summary>
        /// <param name="paths"></param>
        public static EulynxService getInstance()
        
        {
            if (eulynxService == null)
            {
                eulynxService = new EulynxService();
            }
            return eulynxService;
        }

        /// <summary>
        /// Handle input of different format
        /// ha = horizontal alignment
        /// va = vertical alignment
        /// ca = cant alignment
        /// </summary>
        /// <param name="format"></param>
        public void inputHandling(
            string format, Database database,
            string mileage, string edge, string node, string ha, string va, string ca,
            string mdb)
        {
            //input handling
            switch (format)
            {
                case ".json":

                    // @"..\..\..\resources\Kiel
                    // @"..\..\..\resources\Meggen(old)
                    JsonHandler jsonHandler = new();
                    // because of integration with qt we need to place path in main
                    jsonHandler.setMileageFilePath(mileage);
                    jsonHandler.setEdgesFilePath(edge);
                    jsonHandler.setNodesFilePath(node);
                    jsonHandler.setHorizontalAlignmentsFilePath(ha);
                    jsonHandler.setVerticalAlignmentsFilePath(va);
                    jsonHandler.setCantAlignmentsFilePath(ca);
                    jsonHandler.jsonToDatabase(database);

                    break;
                case ".mdb":
                    MdbHandler mdbHandler = new();
                    // because of integration with qt we need to place path in main
                    mdbHandler.setMdbFilePath(mdb);
                    mdbHandler.mdbToDatabase(database);

                    break;
            }
        }

        /// <summary>
        /// Instantiate objects based on data
        /// </summary>
        /// <param name="database"></param>
        /// <param name="country"></param>
        public object objectsCreation(string country, Database database)
        {
            //object creator
            switch (country)
            {
                case "de":
                    DbEulynxObjectsBuilder builder = new();
                    EulynxDirector director = EulynxDirector.getInstance();
                    director.setBuilder(builder);
                    director.constructRsmEntitiesJson(database);
                    return builder.getEulynxDP();
                    break;
                case "fr":
                    break;
                    return default;
            }
            throw new NotImplementedException();
        }

        /// <summary>
        /// Write an eulynx xml file based on available eulynx objects
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="station"></param>
        /// <param name="outputPath"></param>
        public void serialization(EulynxDataPrepInterface eulynx, string station, string outputPath)
        {
            EulynxSerializer serializer = EulynxSerializer.getInstance();
            serializer.serialize(eulynx, station, outputPath);
        }
        /// <summary>
        /// Instantiate eulynx objects based on imported eulynx xml
        /// </summary>
        /// <param name="path">Path to eulynx xml file</param>
        public EulynxDataPrepInterface deserialization(string path)
        {
            EulynxSerializer serializer = EulynxSerializer.getInstance();
            
            return serializer.deserialize<EulynxDataPrepInterface>(path);
        }

        /// <summary>
        /// Plan infrastructure automatically using german rulebooks
        /// </summary>
        /// <param name="eulynx"></param>
        public void plan(EulynxDataPrepInterface eulynx, Database database, bool etcs)
        {
            EulynxPlanner planner = EulynxPlanner.getInstance();

            if (etcs)
            {
                planner.planCCS(eulynx, database);
                planner.planETCS(eulynx, database);
                planner.printCCSInConsole(eulynx);
                planner.printETCSIInConsole(eulynx);
            }
            else
            {
                planner.planCCS(eulynx, database);
                planner.printCCSInConsole(eulynx);
            }

            

        }

        /// <summary>
        /// Validate the generated eulynx xml
        /// </summary>
        public void validation()
        {
            throw new NotImplementedException();
        }
  
    }
}
