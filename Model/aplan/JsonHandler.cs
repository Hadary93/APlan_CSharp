using GeoJSON.Net.Feature;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LiteDB;
using aplan.database;

using static aplan.core.Helper;

namespace aplan.core
{
    class JsonHandler : InputHandler
    {
        
        private string mileageFilePath;
        private string edgesFilePath;
        private string nodesFilePath;
        private string horizontalAlignmentsFilePath;
        private string verticalAlignmentsFilePath;
        private string cantAlignmentsFilePath;

        #region general

        /// <summary>
        /// Method to convert information inside json file into feature collection.
        /// </summary>
        /// <param name="path">path to json file</param>
        public FeatureCollection jsonToFeatureCollection(string path)
        {
            string file;
            FeatureCollection collection;
            try { file = File.ReadAllText(path); } catch { file = null; };
            try { collection = JsonConvert.DeserializeObject<FeatureCollection>(file); } catch { collection = null; };
            return collection;
        }

        
        /// <summary>
        /// Method to fetch information as individual value from a feature.
        /// </summary>
        /// <param name="feature">current feature</param>
        /// <param name="key">key name</param>
        public static object fetchIndividualValue(string key, Feature feature)
        {
            if (feature == null || feature.Properties == null || !feature.Properties.TryGetValue(key, out object output))
                throw new ApplicationException($"Failed to read {key} from feature property {feature.Id}");
            if (output == null)
                output = 0;
            return output;
        }


        /// <summary>
        /// Method to set path to file which contain mileage information.
        /// </summary>
        /// <param name="path">mileage path</param>
        public void setMileageFilePath(string path)
        {
            mileageFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain edges information.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setEdgesFilePath(string path)
        {
            edgesFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain nodes information.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setNodesFilePath(string path)
        {
            nodesFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain horizontal alignments information.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setHorizontalAlignmentsFilePath(string path)
        {
            horizontalAlignmentsFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain vertical alignments information.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setVerticalAlignmentsFilePath(string path)
        {
            verticalAlignmentsFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain cant alignments information.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setCantAlignmentsFilePath(string path)
        {
            cantAlignmentsFilePath = path;
        }


        /// <summary>
        /// Method to set path to file which contain cant alignments information.
        /// </summary>
        /// <param name="desc">description of the alignment</param>
        private int germanAlignmentTypeToInt(string desc)
        {
            switch (desc)
            {
                case "Gerade":
                    return 0;
                    break;
                case "Kreis":
                    return 1;
                case "Klothoide":
                    return 2;
                case "Blosskurve":
                    return 4;
                default:
                    return 99;
            }
        }


        #endregion


        #region Deutsche Bahn


        /// <summary>
        /// Method to fetch all of json information to internal database
        /// </summary>
        /// <param name="database">internal database</param>
        public void jsonToDatabase(Database database)
        {
            // accessing database
            using (var liteDatabase = database.accessDB())
            {
                handleEntwurfselementKilometer(mileageFilePath, liteDatabase);
                handleGleiskanten(edgesFilePath, liteDatabase);
                handleGleisknoten(nodesFilePath, liteDatabase);
                handleEntwurfselementLage(horizontalAlignmentsFilePath, liteDatabase);
                handleEntwurfselementHoehe(verticalAlignmentsFilePath, liteDatabase);
                handleEntwurfselementUeberhoehung(cantAlignmentsFilePath, liteDatabase);
            }
        }

        /// <summary>
        /// Method to handle json mileage information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleEntwurfselementKilometer(string path, LiteDatabase db)
        {
            // read json file and turn it into collection
            FeatureCollection collectionKM = jsonToFeatureCollection(path);
            if (collectionIsNull(collectionKM))
                return;

            // create collection for mileage
            var mileageCollection = db.GetCollection<Mileage>("Mileage");

            foreach (Feature feature in collectionKM.Features)
            {
                var mileage = new Mileage()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    alignmentTypeCode = germanAlignmentTypeToInt(fetchIndividualValue("ELTYP", feature).ToString()),
                    alignmentTypeText = (horizontalAlignmentType)germanAlignmentTypeToInt(fetchIndividualValue("ELTYP", feature).ToString()),
                    length = Convert.ToDouble(fetchIndividualValue("PARAM1", feature)),
                    initialRadius = Convert.ToDouble(fetchIndividualValue("PARAM2", feature)),
                    endRadius = Convert.ToDouble(fetchIndividualValue("PARAM3", feature)),
                    startKM = hmToKm_mileage(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                    endKM = hmToKm_mileage(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                };
                
                // instantiate coordinate objects
                handleLinearCoordinateMileage(db, feature);
                mileage.lineCoordinates = handleCartesianCoordinateLine(db, feature);

                mileageCollection.Insert(mileage);
            }
        }

        /// <summary>
        /// Method to handle linear coordinate information of mileage file
        /// </summary>
        /// <param name="feature">feature of json</param>
        /// <param name="db">internal database</param>
        private static void handleLinearCoordinateMileage(LiteDatabase db, Feature feature)
        {
            // get collection
            var linearCoordinateCollection = db.GetCollection<LinearCoordinate>("LinearCoordinates");

            // linear coordinate
            // start kilometer
            var linearCoordinate = new LinearCoordinate()
            {
                name = "LinearCoordinate" + hmToKm_mileage(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                linearCoordinate = hmToKm_mileage(fetchIndividualValue("KM_A_TEXT", feature).ToString())
            };
            if (!linearCoordinateCollection.Exists(item => item.name.Equals(linearCoordinate.name)))
            {
                linearCoordinateCollection.Insert(linearCoordinate);
            }

            // end kilometer
            linearCoordinate = new LinearCoordinate()
            {
                name = "LinearCoordinate" + hmToKm_mileage(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                linearCoordinate = hmToKm_mileage(fetchIndividualValue("KM_E_TEXT", feature).ToString())
            };
            if (!linearCoordinateCollection.Exists(item => item.name.Equals(linearCoordinate.name)))
            {
                linearCoordinateCollection.Insert(linearCoordinate);
            }
        }

        /// <summary>
        /// Method to handle linear coordinate information of file which is linear element
        /// </summary>
        /// <param name="feature">feature of json</param>
        /// <param name="db">internal database</param>
        private static void handleLinearCoordinateLine(LiteDatabase db, Feature feature)
        {
            // get collection
            var linearCoordinateCollection = db.GetCollection<LinearCoordinate>("LinearCoordinates");

            // linear coordinate
            // start kilometer
            var linearCoordinate = new LinearCoordinate()
            {
                name = "LinearCoordinate" + hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                linearCoordinate = hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString())
            };
            if (!linearCoordinateCollection.Exists(item => item.name.Equals(linearCoordinate.name)))
            {
                linearCoordinateCollection.Insert(linearCoordinate);
            }

            // end kilometer
            linearCoordinate = new LinearCoordinate()
            {
                name = "LinearCoordinate" + hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                linearCoordinate = hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString())
            };
            if (!linearCoordinateCollection.Exists(item => item.name.Equals(linearCoordinate.name)))
            {
                linearCoordinateCollection.Insert(linearCoordinate);
            }
        }

        /// <summary>
        /// Method to handle linear coordinate information of file which is non-linear element (point)
        /// </summary>
        /// <param name="feature">feature of json</param>
        /// <param name="db">internal database</param>
        private static void handleLinearCoordinatePoint(LiteDatabase db, Feature feature)
        {
            // get collection
            var linearCoordinateCollection = db.GetCollection<LinearCoordinate>("LinearCoordinates");

            // linear coordinate
            var linearCoordinate = new LinearCoordinate()
            {
                name = "LinearCoordinate" + hmToKm(fetchIndividualValue("KM_TEXT", feature).ToString()),
                linearCoordinate = hmToKm(fetchIndividualValue("KM_TEXT", feature).ToString())
            };
            if (!linearCoordinateCollection.Exists(item => item.name.Equals(linearCoordinate.name)))
            {
                linearCoordinateCollection.Insert(linearCoordinate);
            }
        }

        /// <summary>
        /// Method to handle cartesian coordinate information of file which is linear element
        /// </summary>
        /// <param name="feature">feature of json</param>
        /// <param name="db">internal database</param>
        private static List<CartesianCoordinate> handleCartesianCoordinateLine(LiteDatabase db, Feature feature)
        {
            // get collection
            var geometricCoordinateCollection = db.GetCollection<CartesianCoordinate>("CartesianCoordinates");

            // geometric coordinate
            var lineString = feature.Geometry as GeoJSON.Net.Geometry.LineString;
            var coordinateList = new List<CartesianCoordinate>();
            int i = 0;

            foreach (GeoJSON.Net.Geometry.Position lineStringCoordinate in lineString.Coordinates)
            {
                var coordinate = new CartesianCoordinate()
                {
                    name = "CartesianCoordinate" + lineStringCoordinate.Longitude,
                    index = i,
                    x = lineStringCoordinate.Longitude,
                    y = lineStringCoordinate.Latitude
                };
                coordinateList.Add(coordinate);
                geometricCoordinateCollection.Insert(coordinate);
                i++;
            }

            return coordinateList;
        }

        /// <summary>
        /// Method to handle cartesian coordinate information of file which is non-linear element (point)
        /// </summary>
        /// <param name="feature">feature of json</param>
        /// <param name="db">internal database</param>
        private static CartesianCoordinate handleCartesianCoordinatePoint(LiteDatabase db, Feature feature)
        {
            // get collection
            var geometricCoordinateCollection = db.GetCollection<CartesianCoordinate>("CartesianCoordinates");

            // cartesian coordinate
            var point = feature.Geometry as GeoJSON.Net.Geometry.Point;
            var pointCoordinate = (GeoJSON.Net.Geometry.Position)point.Coordinates;

            var coordinate = new CartesianCoordinate()
            {
                name = "CartesianCoordinate" + fetchIndividualValue("ID", feature).ToString(),
                index = 0,
                x = pointCoordinate.Longitude,
                y = pointCoordinate.Latitude
            };
            geometricCoordinateCollection.Insert(coordinate);

            return coordinate;
        }

        /// <summary>
        /// Method to handle json edge information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleGleiskanten(string path, LiteDatabase db)
        {

            // read json file and turn it into collection
            FeatureCollection collectionNE = jsonToFeatureCollection(path);

            // check collection contents
            if (collectionIsNull(collectionNE))
                return;

            // create database collection for Gleiskanten
            var netElementCollection = db.GetCollection<NetElement>("NetElements");

            foreach (Feature feature in collectionNE.Features)
            {
                if (!feature.Geometry.Type.Equals(GeoJSON.Net.GeoJSONObjectType.LineString))
                    continue;
                
                var netElement = new NetElement()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    status = fetchIndividualValue("STATUS", feature).ToString(),
                    startNodeId = fetchIndividualValue("KN_ID_V", feature).ToString(),
                    endNodeId = fetchIndividualValue("KN_ID_B", feature).ToString(),
                    length = Convert.ToDouble(fetchIndividualValue("LAENGE_ENT", feature))/1000, //convert to m
                    codeDirection = Convert.ToInt32(fetchIndividualValue("RIKZ", feature)),
                    startKm = hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                    endKm = hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                    geometryType = feature.Geometry.Type.ToString()
                };

                // instantiate coordinate objects
                handleLinearCoordinateLine(db, feature);
                netElement.lineCoordinates = handleCartesianCoordinateLine(db, feature);

                netElementCollection.Insert(netElement);
            }
        }

        /// <summary>
        /// Method to handle json node information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleGleisknoten(string path, LiteDatabase db)
        {

            // read json file and turn it into collection
            FeatureCollection collectionNO = jsonToFeatureCollection(path);
            if (collectionIsNull(collectionNO))
                return;

            // create collection for Gleiskanten
            var collection = db.GetCollection<Node>("Nodes");
            
            foreach (Feature feature in collectionNO.Features)
            {
                if (!feature.Geometry.Type.Equals(GeoJSON.Net.GeoJSONObjectType.Point))
                    continue;

                var node = new Node()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    codeDirection = Convert.ToInt32(fetchIndividualValue("RIKZ", feature)),
                    originNodeID = fetchIndividualValue("KN_ID_AN", feature).ToString(),
                    km = hmToKm(fetchIndividualValue("KM_TEXT", feature).ToString()),
                    nodeType = Convert.ToInt32(fetchIndividualValue("KN_TYP", feature)),
                    geometryType = feature.Geometry.Type.ToString()
                };

                // handle destination nodes
                node.destinationNodeID = new ArrayList();
                for (int i = 0; i < 3; i++)
                {
                    node.destinationNodeID.Add(fetchIndividualValue("KN_ID_AB" + (i + 1), feature).ToString());
                }

                // instantiate coordinate objects
                handleLinearCoordinatePoint(db, feature);
                node.coordinate = handleCartesianCoordinatePoint(db, feature);
                
                collection.Insert(node);
            }
        }

        /// <summary>
        /// Method to handle json horizontal alignment information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleEntwurfselementLage(string path, LiteDatabase db)
        {
            // read json file and turn it into collection
            FeatureCollection collectionHA = jsonToFeatureCollection(path);
            if (collectionIsNull(collectionHA))
                return;

            // create collection for Gleiskanten
            var collection = db.GetCollection<HorizontalAlignment>("HorizontalAlignments");

            foreach (Feature feature in collectionHA.Features)
            {
                var horizontalAlignment = new HorizontalAlignment()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    codeDirection = Convert.ToInt32(fetchIndividualValue("RIKZ", feature)),
                    alignmentTypeCode = Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    alignmentTypeText = (horizontalAlignmentType)Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    length = Convert.ToDouble(fetchIndividualValue("PARAM1", feature)),
                    initialRadius = Convert.ToDouble(fetchIndividualValue("PARAM2", feature)),
                    endRadius = Convert.ToDouble(fetchIndividualValue("PARAM3", feature)),
                    initialAzimuth = Convert.ToDouble(fetchIndividualValue("WINKEL_AN", feature)),
                    startKM = hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                    endKM = hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                };

                // instantiate coordinate objects
                handleLinearCoordinateLine(db, feature);
                horizontalAlignment.lineCoordinates = handleCartesianCoordinateLine(db, feature);

                collection.Insert(horizontalAlignment);
            }
        }

        /// <summary>
        /// Method to handle json vertical alignment information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleEntwurfselementHoehe(string path, LiteDatabase db)
        {
            // read json file and turn it into collection
            FeatureCollection collectionVA = jsonToFeatureCollection(path);
            if (collectionIsNull(collectionVA))
                return;

            // create collection for Gleiskanten
            var collection = db.GetCollection<VerticalAlignment>("VerticalAlignments");

            foreach (Feature feature in collectionVA.Features)
            {
                var verticalAlignment = new VerticalAlignment()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    codeDirection = Convert.ToInt32(fetchIndividualValue("RIKZ", feature)),
                    alignmentTypeCode = Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    alignmentTypeText = (verticalAlignmentType)Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    length = Convert.ToDouble(fetchIndividualValue("PARAM1", feature)),
                    initialSlope = Convert.ToDouble(fetchIndividualValue("PARAM2", feature)),
                    endSlope = Convert.ToDouble(fetchIndividualValue("PARAM3", feature)),
                    initialHeight = Convert.ToDouble(fetchIndividualValue("HOEHE_A_R", feature)),
                    endHeight = Convert.ToDouble(fetchIndividualValue("HOEHE_E_R", feature)),
                    startKM = hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                    endKM = hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                };

                // instantiate coordinate objects
                handleLinearCoordinateLine(db, feature);
                verticalAlignment.lineCoordinates = handleCartesianCoordinateLine(db, feature);

                collection.Insert(verticalAlignment);
            }
        }

        /// <summary>
        /// Method to handle json cant alignment information
        /// </summary>
        /// <param name="path">path to particular json file</param>
        /// <param name="db">internal database</param>
        public void handleEntwurfselementUeberhoehung(string path, LiteDatabase db)
        {
            // read json file and turn it into collection
            FeatureCollection collectionCA = jsonToFeatureCollection(path);
            if (collectionIsNull(collectionCA))
                return;

            // create collection for Gleiskanten
            var collection = db.GetCollection<AlignmentCant>("AlignmentCants");

            foreach (Feature feature in collectionCA.Features)
            {
                var alignmentCant = new AlignmentCant()
                {
                    id = fetchIndividualValue("ID", feature).ToString(),
                    codeDirection = Convert.ToInt32(fetchIndividualValue("RIKZ", feature)),
                    alignmentTypeCode = Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    alignmentTypeText = (alignmentCantType)Convert.ToInt32(fetchIndividualValue("ELTYP", feature)),
                    length = Convert.ToDouble(fetchIndividualValue("PARAM1", feature)),
                    initialSuperelevation = Convert.ToDouble(fetchIndividualValue("PARAM2", feature)),
                    endSuperelevation = Convert.ToDouble(fetchIndividualValue("PARAM3", feature)),
                    startKm = hmToKm(fetchIndividualValue("KM_A_TEXT", feature).ToString()),
                    endKm = hmToKm(fetchIndividualValue("KM_E_TEXT", feature).ToString()),
                };

                // instantiate coordinate objects
                handleLinearCoordinateLine(db, feature);
                alignmentCant.lineCoordinates = handleCartesianCoordinateLine(db, feature);

                collection.Insert(alignmentCant);
            }
        }


        #endregion

    }
}
