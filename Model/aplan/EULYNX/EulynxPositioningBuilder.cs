using aplan.core;
using aplan.database;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using System.Linq;

using LinearCoordinate = Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate;
using CartesianCoordinate = Models.TopoModels.EULYNX.rsmCommon.CartesianCoordinate;

using static aplan.core.Helper;
using System.Diagnostics;

namespace aplan.eulynx
{
    class EulynxPositioningBuilder
    {

        #region linear

        /// <summary>
        /// Method to create relative positioning system.
        /// It will begin from the lowest number KM of element and end by the highest number of KM
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addLinearPositioningSystem(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            // refer unit
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var km = referUnit(rsmEntities, "kilometer");

            rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };


            // accessing database
            using (var db = database.accessDB()) 
            {
                // get collection
                var collection = db.GetCollection<Mileage>("Mileage").FindAll();
                
                if (collection == null)
                {
                    // create custom linear positioning system
                    LinearPositioningSystem linearPositioningSystem = new LinearPositioningSystem()
                    {
                        id = generateUUID(),
                        name = "Relative Positioning System",
                        hasLinearReferencingMethod = LrMethod.relative
                    };
                    rsmEntities.usesTopography.usesPositioningSystem.Add(linearPositioningSystem);
                    Debug.WriteLine("A custom linear positioning system without measurement has been created");
                }
                else
                {
                    (var lowestMileage, var highestMileage) = findMinMax(db);
                    LinearPositioningSystem linearPositioningSystem = new LinearPositioningSystem()
                    {
                        id = generateUUID(),
                        name = "Relative Positioning System",
                        hasLinearReferencingMethod = LrMethod.relative,
                        startMeasure = new Length { value = (double)lowestMileage, unit = km },
                        endMeasure = new Length { value = (double)highestMileage, unit = km }
                    };

                    rsmEntities.usesTopography.usesPositioningSystem.Add(linearPositioningSystem);
                }
            }
        }


        /// <summary>
        /// Instantiate linear coordinate. This coordinate is based on relative positioning system.
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addLinearCoordinate(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            // refer unit
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var km = referUnit(rsmEntities, "kilometer");

            rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };

            // accessing database
            using (var db = database.accessDB())
            {


                // get collection
                var collection = db.GetCollection<database.LinearCoordinate>("LinearCoordinates").FindAll();

                foreach (var item in collection)
                {
                    var linearCoordinate = new LinearCoordinate()
                    {
                        id = generateUUID(),
                        name = item.name,
                        positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System")
                    };
                    linearCoordinate.measure = new Length { value = item.linearCoordinate, unit = km};

                    if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
                    {
                        rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
                    }
                }
            }
        }

        /// <summary>
        /// Add polyline coordinate along the linear positioning system.
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addPolyLineToLinearPositioningSystem(EulynxDataPrepInterface eulynx, Database database)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<Mileage>("Mileage").FindAll();

                var linearPositioningSystem = rsmEntities.usesTopography.usesPositioningSystem?.First(sys => sys.name == "Relative Positioning System") as LinearPositioningSystem;

                var polyline = new PolyLine();
                foreach (var segment in collection)
                {
                    foreach (var coor in segment.lineCoordinates)
                    {
                        polyline.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + coor.x));
                    }
                }
                var linearLocation = new LinearLocation() { id = generateUUID(), polyLines = { polyline } };

                linearPositioningSystem.isLocatedAt = linearLocation;
            }
        }

        #endregion


        #region geometric

        /// <summary>
        /// Instantiate a grid reference system for the German mapping projection centered on 9 degrees East
        /// </summary>
        /// <see cref="https://epsg.io/31467"/>
        /// <param name="eulynx"></param>
        public static void addGeometricPositioningSystem(EulynxDataPrepInterface eulynx)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };
            var geometricPositioningSystem = new GridReferenceSystem()
            {
                id = generateUUID(),
                name = "Grid Reference System",
                epsgCode = "31467"
            };
            rsmEntities.usesTopography.usesPositioningSystem.Add(geometricPositioningSystem);
        }

        /// <summary>
        /// Instantiate cartesian coordinate. This coordinate is based on geometric positioning system.
        /// (German grid reference system)
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addCartesianCoordinate(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            // refer unit
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<database.CartesianCoordinate>("CartesianCoordinates").FindAll();

                foreach (var item in collection)
                {

                    var cartesianCoordinate = new CartesianCoordinate()
                    {
                        id = generateUUID(),
                        name = item.name,
                        positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System"),
                        x = item.x,
                        y = item.y,
                    };
                    rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(cartesianCoordinate);
                }
            }
        }

        #endregion
 
    }
}



// old code
// these below already being handled in jsonhandler

///// <summary>
///// Instantiate element's cartesian coordinate for linestring type. This coordinate is based on geometric positioning system.
///// (German grid reference system)
///// </summary>
///// <param name="eulynx"></param>
///// <param name="collection"></param>
//public static void addCartesianCoordinateLine(EulynxDataPrepInterfaceExtension eulynx, FeatureCollection collection)
//{
//    if (collectionIsNull(collection))
//        return;

//    var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
//    foreach (Feature feature in collection.Features)
//    {
//        if (!feature.Geometry.Type.Equals(GeoJSON.Net.GeoJSONObjectType.LineString))
//            continue;

//        var lineString = feature.Geometry as GeoJSON.Net.Geometry.LineString;

//        int i = 1;
//        foreach (GeoJSON.Net.Geometry.Position lineStringCoordinate in lineString.Coordinates)
//        {

//            CartesianCoordinate cartesianCoordinate = new CartesianCoordinate()
//            {
//                id = generateUUID(),
//                name = "CartesianCoordinate" + fetchIndividualValue("ID", feature) + "_" + i, // named with id so that later can be found easily for reference purpose
//                positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System"),
//                x = lineStringCoordinate.Longitude,
//                y = lineStringCoordinate.Latitude,
//                //z = height is not added yet
//            };
//            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(cartesianCoordinate);
//            i++;
//        }
//    }
//}


///// <summary>
///// Instantiate element's cartesian coordinate for point type. This coordinate is based on geometric positioning system.
///// (German grid reference system)
///// </summary>
///// <param name="eulynx"></param>
///// <param name="collection"></param>
//public static void addCartesianCoordinatePoint(EulynxDataPrepInterfaceExtension eulynx, FeatureCollection collection)
//{
//    if (collectionIsNull(collection))
//        return;

//    var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
//    foreach (Feature feature in collection.Features)
//    {
//        if (!feature.Geometry.Type.Equals(GeoJSON.Net.GeoJSONObjectType.Point))
//            continue;

//        var point = feature.Geometry as GeoJSON.Net.Geometry.Point;
//        var pointCoordinate = (GeoJSON.Net.Geometry.Position)point.Coordinates;

//        CartesianCoordinate cartesianCoordinate = new CartesianCoordinate()
//        {
//            id = generateUUID(),
//            positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System"),
//            x = pointCoordinate.Longitude,
//            y = pointCoordinate.Latitude,
//            //z = height is not added yet
//            name = "CartesianCoordinate" + fetchIndividualValue("ID", feature) //named with id so that later can be found easily for reference purpose
//        };
//        rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(cartesianCoordinate);

//    }
//}

///// <summary>
///// Instantiate point's linear coordinate. This coordinate is based on absolute positioning system.
///// </summary>
///// <param name="eulynx"></param>
///// <param name="database"></param>
//public static void addLinearCoordinatePoint(EulynxDataPrepInterfaceExtension eulynx, Database database)
//{
//    //if (collectionIsNull(collection))
//    //    return;
//    // rsm container
//    // refer unit
//    var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
//    var km = referUnit(rsmEntities, "kilometer");
//    rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };

//    LinearPositioningSystem usedLinearPositioningSystem = (LinearPositioningSystem)rsmEntities.usesTopography.usesPositioningSystem.Find(item => item.name.Equals("Relative Positioning System"));

//    // accessing database
//    using (var db = database.accessDB())
//    {
//        // get collection
//        var collection = database.accessDB().GetCollection<database.Node>("Nodes").FindAll();

//        foreach (var item in collection)
//        {
//            var linearCoordinate = new Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate()
//            {
//                id = generateUUID(),
//                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                name = "LinearCoordinatePoint" + item.id,
//                measure = item.locationKM,
//                //this new attribute is used to store the application direction from data
//                isLocatedIn = item.codeDirection
//            };
//            if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => item.name.Equals(linearCoordinate.name)))
//            {
//                rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//            }
//        }
//    }
//}





///// <summary>
///// Instantiate line's linear coordinate for topology segment. This coordinate is based on absolute positioning system.
///// </summary>
///// <param name="eulynx"></param>
///// <param name="database"></param>
//public static void addLinearCoordinateLine(EulynxDataPrepInterfaceExtension eulynx, Database database)
//{
//    //if (collectionIsNull(collection))
//    //    return;

//    // rsm container
//    // refer unit
//    var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
//    var km = referUnit(rsmEntities, "kilometer");
//    rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };

//    // accessing database
//    using (var db = database.accessDB())
//    {
//        // get collection
//        var collection = database.accessDB().GetCollection<NetElement>("Nodes").FindAll();

//        foreach (var item in collection)
//        {

//            if (item.codeDirection == 1 || item.codeDirection == 2)
//            {
//                LinearCoordinate linearCoordinate = new LinearCoordinate()
//                {
//                    id = generateUUID(),
//                    name = "LinearCoordinate" + item.,
//                    positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                    measure = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)),
//                    //this new attribute is used to store the kilometer value from data
//                    measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)), unit = km },
//                    //this new attribute is used to store the application direction from data
//                    isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//                };
//                if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//                {
//                    rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//                }

//                // KM bis
//                linearCoordinate = new LinearCoordinate()
//                {
//                    id = generateUUID(),
//                    name = "LinearCoordinate" + hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)),
//                    positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                    measure = hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)),
//                    //this new attribute is used to store the kilometer value from data
//                    measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)) },
//                    //this new attribute is used to store the application direction from data
//                    isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//                };
//                if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//                {
//                    rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);

//                }
//            }
//            else
//            {
//                // KM von
//                LinearCoordinate linearCoordinate = new LinearCoordinate()
//                {
//                    id = generateUUID(),
//                    name = "LinearCoordinateUnknown",
//                    positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                    measure = 0,
//                    // this new attribute is used to store the kilometer value from data
//                    // measureInData = 0,
//                    // this new attribute is used to store the application direction from data
//                    isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//                };
//                if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//                {
//                    rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//                }

//                //KM bis
//                linearCoordinate = new LinearCoordinate()
//                {
//                    id = generateUUID(),
//                    name = "LinearCoordinateUnknown",
//                    positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                    measure = 0,
//                    // this new attribute is used to store the kilometer value from data
//                    //measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)) },
//                    // this new attribute is used to store the application direction from data
//                    isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//                };
//                if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//                {
//                    rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//                }
//            }
//        }
//    }

//    foreach (Feature feature in collection.Features)
//    {
//        if (Convert.ToInt32(fetchIndividualValue("RIKZ", feature)) == 1 || Convert.ToInt32(fetchIndividualValue("RIKZ", feature)) == 2)
//        {
//            // KM von
//            LinearCoordinate linearCoordinate = new LinearCoordinate()
//            {
//                id = generateUUID(),
//                name = "LinearCoordinate" + hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)),
//                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                measure = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)),
//                //this new attribute is used to store the kilometer value from data
//                measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)), unit = km },
//                //this new attribute is used to store the application direction from data
//                isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//            };
//            if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//            {
//                rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//            }

//            // KM bis
//            linearCoordinate = new LinearCoordinate()
//            {
//                id = generateUUID(),
//                name = "LinearCoordinate" + hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)),
//                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                measure = hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)),
//                //this new attribute is used to store the kilometer value from data
//                measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_E_TEXT", feature)) },
//                //this new attribute is used to store the application direction from data
//                isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//            };
//            if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//            {
//                rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);

//            }
//        }
//        else
//        {
//            // KM von
//            LinearCoordinate linearCoordinate = new LinearCoordinate()
//            {
//                id = generateUUID(),
//                name = "LinearCoordinateUnknown",
//                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                measure = 0,
//                // this new attribute is used to store the kilometer value from data
//                // measureInData = 0,
//                // this new attribute is used to store the application direction from data
//                isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//            };
//            if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//            {
//                rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//            }

//            //KM bis
//            linearCoordinate = new LinearCoordinate()
//            {
//                id = generateUUID(),
//                name = "LinearCoordinateUnknown",
//                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//                measure = 0,
//                // this new attribute is used to store the kilometer value from data
//                //measureInData = new Length { value = hm_m2km((string)fetchIndividualValue("KM_A_TEXT", feature)) },
//                // this new attribute is used to store the application direction from data
//                isLocatedIn = Convert.ToInt32(fetchIndividualValue("RIKZ", feature))
//            };
//            if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((LinearCoordinate)item).measure == linearCoordinate.measure))
//            {
//                rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//            }
//        }
//    }
//}


///// <summary>
///// Instantiate line's linear coordinate for topology segment. This coordinate is based on absolute positioning system.
///// specifically for mileage
///// </summary>
///// <param name="eulynx"></param>
///// <param name="collection"></param>
//public static void addLinearCoordinateLineMileage(EulynxDataPrepInterfaceExtension eulynx, FeatureCollection collection)
//{
//    if (collectionIsNull(collection))
//        return;

//    var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
//    rsmEntities.usesTopography ??= new Topography() { id = generateUUID() };
//    var km = referUnit(rsmEntities, "kilometer");
//    foreach (Feature feature in collection.Features)
//    {
//        // KM von
//        var linearCoordinate = new Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate()
//        {
//            id = generateUUID(),
//            name = "LinearCoordinate" + hmToKm_mileage((string)fetchIndividualValue("KM_A_TEXT", feature)),
//            positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//            measure = hmToKm_mileage((string)fetchIndividualValue("KM_A_TEXT", feature)),
//            //this new attribute is used to store the kilometer value from data
//            measureInData = new Length { value = hmToKm_mileage((string)fetchIndividualValue("KM_A_TEXT", feature)), unit = km }
//        };
//        if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate)item).measure == linearCoordinate.measure))
//        {
//            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);
//        }

//        // KM bis
//        linearCoordinate = new Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate()
//        {
//            id = generateUUID(),
//            name = "LinearCoordinate" + hmToKm_mileage((string)fetchIndividualValue("KM_E_TEXT", feature)),
//            positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
//            measure = hmToKm_mileage((string)fetchIndividualValue("KM_E_TEXT", feature)),
//            //this new attribute is used to store the kilometer value from data
//            measureInData = new Length { value = hmToKm_mileage((string)fetchIndividualValue("KM_E_TEXT", feature)) }
//        };
//        if (!rsmEntities.usesTopography.usesPositioningSystemCoordinate.Exists(item => ((Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate)item).measure == linearCoordinate.measure))
//        {
//            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(linearCoordinate);

//        }
//    }
//}
