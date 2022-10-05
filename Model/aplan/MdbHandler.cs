using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Collections;
using System.Linq;
using LiteDB;
using aplan.database;

using static aplan.core.Helper;

// Linear Coordinate -> database


namespace aplan.core
{
    public static class Globals
    {
        public static int routeNumber;
    }
    class MdbHandler
    {

        private string mdbFilePath;

        #region general

        /// <summary>
        /// Method to set path to file which contains all information of the station.
        /// </summary>
        /// <param name="path">edges path</param>
        public void setMdbFilePath(string path)
        {
            mdbFilePath = path;
        }


        /// <summary>
        /// Method to establish connection with mdb
        /// </summary>
        private OleDbConnection establishConnection()
        {
            string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0; Data Source=" + mdbFilePath;
            var connection = new OleDbConnection(connectionString);
            return connection;
        }


        /// <summary>
        /// Method to handle mdb file content to be fetched into internal database.
        /// </summary>
        /// <param name="database">internal database</param>
        public void mdbToDatabase(Database database)
        {

            // establish connection with mdb
            var connection = establishConnection();

            // accessing internal database
            using (var liteDatabase = database.accessDB())
            {
                // fetch all coordinates
                handleLinearCoordinate(liteDatabase, connection);
                handleCartesianCoordinate(liteDatabase, connection);
                handleHeight(liteDatabase, connection);

                // fetch mileage information
                handleEntwurfselementKilometer(liteDatabase, connection);

                // fetch topology information
                handleGleisknoten(liteDatabase, connection);
                handleGleiskanten(liteDatabase, connection);
                addMissingNetElements(liteDatabase);
                
                // fetch topography information
                handleEntwurfselementLage(liteDatabase, connection);
                handleEntwurfselementHoehe(liteDatabase, connection);
                handleEntwurfselementUeberhoehung(liteDatabase, connection);

                // add missing information
                addNetElementLengthAndCoordinates(liteDatabase, connection);

            }
        }

        #endregion

        #region coordinate

        /// <summary>
        /// Method to handle information about linear coordinates from mdb file.
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleLinearCoordinate(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for linear coordinate
            var linearCoordinateCollection = db.GetCollection<LinearCoordinate>("LinearCoordinates");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC11_PP";

            // command, loop, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // should match with given route number to filter some unwanted information
                            if (Convert.ToInt32(reader["PSTRECKE"]) == Globals.routeNumber)
                            {
                                // create internal database object of linear coordinate
                                // naming the object with its kilometer value -> use later for planning algorithm
                                var linearCoordinate = new LinearCoordinate()
                                {
                                    name = "LinearCoordinate" + valueParser(Convert.ToDouble(reader["STATION"])),
                                    pad = reader["PAD"].ToString(),
                                    linearCoordinate = valueParser(Convert.ToDouble(reader["STATION"])),
                                    unit = "km"
                                };

                                // check if coordinate already exists
                                if (linearCoordinateCollection.Exists(item => item.pad == linearCoordinate.pad))
                                {
                                    // check the program variant
                                    // here the value with GND-Edit programm will be used
                                    // since the value makes more sense and contains kilometer value
                                    if (reader["PPROG"].ToString().Equals("GND-Edit"))
                                    {
                                        var toBeRemoved = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(item => item.pad == linearCoordinate.pad).Id;
                                        db.GetCollection<LinearCoordinate>("LinearCoordinates").Delete(toBeRemoved);
                                        linearCoordinateCollection.Insert(linearCoordinate);
                                    }
                                }
                                else
                                {
                                    linearCoordinateCollection.Insert(linearCoordinate);
                                }
                            }
                            else
                            {
                                // create internal database object of linear coordinate
                                // naming the object with its kilometer value -> use later for planning algorithm
                                var linearCoordinate = new LinearCoordinate()
                                {
                                    name = "LinearCoordinate" + Convert.ToDouble(reader["STATION"]), // PAD?
                                    pad = reader["PAD"].ToString(),
                                    linearCoordinate = Convert.ToDouble(reader["STATION"]),
                                    unit = "m"
                                };

                                // check if coordinate already exists
                                if (linearCoordinateCollection.Exists(item => item.pad == linearCoordinate.pad))
                                {
                                    // check the program variant
                                    // here the value with GND-Edit programm will be used
                                    // since the value makes more sense and contains kilometer value
                                    if (reader["PPROG"].ToString().Equals("GND-Edit"))
                                    {
                                        var toBeRemoved = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(item => item.pad == linearCoordinate.pad).Id;
                                        db.GetCollection<LinearCoordinate>("LinearCoordinates").Delete(toBeRemoved);
                                        linearCoordinateCollection.Insert(linearCoordinate);
                                    }
                                }
                                else
                                {
                                    linearCoordinateCollection.Insert(linearCoordinate);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }
        

        /// <summary>
        /// Method to handle information about cartesian coordinates from mdb file
        /// p.s x and y coordinates are switched to each other
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleCartesianCoordinate(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for geometric coordinate
            var geometricCoordinateCollection = db.GetCollection<CartesianCoordinate>("CartesianCoordinates");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC12_PL";

            // command, loop, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of cartesian coordinate
                            // naming the object with its x value -> use later for planning algorithm
                            var coordinate = new CartesianCoordinate()
                            {
                                name = "CartesianCoordinate" + reader["Y"].ToString(),
                                pad = reader["PAD"].ToString(),
                                index = 0,
                                x = Convert.ToDouble(reader["Y"]),
                                y = Convert.ToDouble(reader["X"])
                            };
                            geometricCoordinateCollection.Insert(coordinate);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }
        
        
        /// <summary>
        /// Method to handle information about heights from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        private void handleHeight(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for height
            var heightCollection = db.GetCollection<Height>("Heights");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC13_PH";

            // command, loop, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of height
                            // naming the object with its pad value -> not really matter since it uses for internal purpose
                            var height = new Height()
                            {
                                name = "Height" + reader["PAD"].ToString(),
                                pad = reader["PAD"].ToString(),
                                height = Convert.ToDouble(reader["H"])
                            };
                            heightCollection.Insert(height);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }

        #endregion

        #region mileage

        /// <summary>
        /// Method to handle information about mileage from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleEntwurfselementKilometer(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for mileage
            var mileageCollection = db.GetCollection<Mileage>("Mileage");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC24_EK";

            // command, loop, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of mileage
                            var mileage = new Mileage()
                            {
                                startPad = reader["PAD1"].ToString(),
                                endPad = reader["PAD2"].ToString(),
                                alignmentTypeCode = Convert.ToInt32(reader["EKTYP"]),
                                alignmentTypeText = (horizontalAlignmentType)Convert.ToInt32(reader["EKTYP"]),
                                length = Convert.ToDouble(reader["EKPAR1"]),
                                initialRadius = Convert.ToDouble(reader["EKPAR2"]),
                                endRadius = Convert.ToDouble(reader["EKPAR3"]),
                                initialAzimuth = Convert.ToDouble(reader["EKARIWI"]),
                                startKM = valueParser(Convert.ToDouble(reader["EKAKM"])),
                                endKM = valueParser(Convert.ToDouble(reader["EKEKM"])),
                            };

                            // extra method to handle geo coordinates in one segment
                            handleCartesianCoordinateLine(db, mileage);

                            mileageCollection.Insert(mileage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }
        
        
        /// <summary>
        /// Extra method to handle geo coordinates in one mileage segment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="mileage">mileage segment</param>
        private void handleCartesianCoordinateLine(LiteDatabase db, Mileage mileage)
        {
            var coordinateList = new List<CartesianCoordinate>();
            var firstCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == mileage.startPad);
            var secondCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == mileage.endPad);
            coordinateList.Add(firstCoordinate);
            coordinateList.Add(secondCoordinate);
            mileage.lineCoordinates = coordinateList;
        }

        #endregion

        #region net element

        /// <summary>
        /// Method to handle information about net horizontalAlignment from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleGleiskanten(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for Gleiskanten
            var netElementCollection = db.GetCollection<NetElement>("NetElements");

            // set query for reading process
            var strSQL = "SELECT * FROM X_ASC33_GS";

            // command, loop, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of net horizontalAlignment
                            // some net elements save its startKm and endKm in meter because of data
                            var netElement = new NetElement()
                            {
                                parseNumber = Convert.ToInt16(reader["ParseLfdNr"]),
                                status = "no information",
                                trackType = "no information", // undefined
                                startNodeId = reader["AKNOTEN"].ToString().Trim(),
                                endNodeId = reader["EKNOTEN"].ToString().Trim(),
                                startPad = db.GetCollection<Node>("Nodes").FindOne(x => x.id == reader["AKNOTEN"].ToString().Trim()).pad,
                                endPad = db.GetCollection<Node>("Nodes").FindOne(x => x.id == reader["EKNOTEN"].ToString().Trim()).pad,
                                startKm = db.GetCollection<Node>("Nodes").FindOne(x => x.id == reader["AKNOTEN"].ToString().Trim()).km,
                                endKm = db.GetCollection<Node>("Nodes").FindOne(x => x.id == reader["EKNOTEN"].ToString().Trim()).km,
                                codeDirection = Convert.ToInt16(reader["STRRIKZ"])
                            };

                            // change code direction 0 to 1 to meet standard along with json format
                            if(netElement.codeDirection == 0)
                            {
                                netElement.codeDirection = 1;
                            }

                            netElementCollection.Insert(netElement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }


        /// <summary>
        /// Method to add net horizontalAlignment length through calculation of alignment segement associated with the object
        /// Method to handle geo coordinates in one net horizontalAlignment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void addNetElementLengthAndCoordinates(LiteDatabase db, OleDbConnection connection)
        {
            // help variables
            var exception = false;

            // find existing net horizontalAlignment in internal database
            var netElementCollection = db.GetCollection<NetElement>("NetElements").FindAll();

            // loop through netElementCollection
            foreach(var netElement in netElementCollection)
            {
                exception = false;

                // set query for reading process
                var findLength = "SELECT * FROM X_ASC33_GS_L WHERE ParseLfdNr LIKE '" + netElement.parseNumber + "'";

                // set length to 0 and empty list of geo coordinates
                var length = 0.0;
                var coordinateList = new List<CartesianCoordinate>();

                // command, loop, read
                using (OleDbCommand command = new OleDbCommand(findLength, connection))
                {
                    try
                    {
                        connection.Open();
                        using (OleDbDataReader reader = command.ExecuteReader())
                        {
                            // toogle exception to true
                            exception = true;

                            // length is added everytime alignment parsenumber match with net horizontalAlignment parsenumber
                            while (reader.Read())
                            {
                                var matchedItem = getHorizontalAlignment(db, reader["L"].ToString());
                                length += matchedItem.length;
                                coordinateList.AddRange(matchedItem.lineCoordinates);
                                exception = false;
                            }

                            // exceptional case for net horizontalAlignment without parsenumber (the one which is missing)
                            // length is calculated by comparing the alignment segment start and end with net horizontalAlignment start and end
                            if (exception)
                            {
                                // exception
                                var border1 = netElement.startKm;
                                var border2 = netElement.endKm;
                                var horizontalAlignmentCollection = db.GetCollection<HorizontalAlignment>("HorizontalAlignments").Find(x => x.unit == "km");
                                foreach(var horizontalAlignment in horizontalAlignmentCollection)
                                {
                                    if(horizontalAlignment.startKM > border1 && horizontalAlignment.startKM < border2)
                                    {
                                        length += horizontalAlignment.length;
                                        coordinateList.AddRange(horizontalAlignment.lineCoordinates);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    connection.Close();
                }
                netElement.length = length;
                netElement.lineCoordinates = coordinateList;
                db.GetCollection<NetElement>("NetElements").Update(netElement);
            }
        }


        /// <summary>
        /// Method to help getting correct horizontal alignment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="pad">punkt addresse</param>
        private static HorizontalAlignment getHorizontalAlignment(LiteDatabase db, string pad)
        {
            var horizontalAlignments = db.GetCollection<HorizontalAlignment>("HorizontalAlignments").FindAll();

            foreach(var horizontalAlignment in horizontalAlignments)
            {
                if(horizontalAlignment.startPad == pad || horizontalAlignment.endPad == pad)
                {
                    return horizontalAlignment;
                }
            }
            return null;
        }


        /// <summary>
        /// Method to add missing net horizontalAlignment based on nodes data
        /// Proceed after handling node information
        /// </summary>
        /// <param name="db">internal database</param>
        public void addMissingNetElements(LiteDatabase db)
        {
            // call database netElementCollection of net elements and nodes
            var netElementCollection = db.GetCollection<NetElement>("NetElements");
            var nodeCollectionAll = db.GetCollection<Node>("Nodes").FindAll();

            // loop through node netElementCollection
            foreach (var node in nodeCollectionAll)
            {
                // check if a node linked with another node that already exists or not
                if (!db.GetCollection<Node>("Nodes").Exists(n => n.name == node.originNodeID))
                {
                    // create internal database object of net horizontalAlignment
                    // in this case the missing net horizontalAlignment is considered as main track with code direction 1
                    var netElement = new NetElement()
                    {
                        id = "",
                        status = "no information",
                        trackType = "no information", // undefined
                        codeDirection = 1
                    };

                    // sorting node and linear coordinate netElementCollection
                    var sortedNodeCollection = db.GetCollection<Node>("Nodes").Find(n => n.codeDirection == 1).OrderBy(n => n.km);
                    var sortedLinearCoordinateCollection = db.GetCollection<LinearCoordinate>("LinearCoordinates").Find(lc => lc.unit == "km").OrderBy(lc => lc.linearCoordinate);

                    // assigning pad, node ids, and kilometer value
                    if (node.km <= sortedNodeCollection.First().km)
                    {
                        var pad = sortedLinearCoordinateCollection.First().pad;
                        netElement.startPad = pad;
                        netElement.endPad = node.pad;
                        netElement.startNodeId = node.originNodeID;
                        netElement.endNodeId = node.id;
                        netElement.startKm = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(lc => lc.pad == pad).linearCoordinate;
                        netElement.endKm = node.km;
                    }
                    else if (node.km >= sortedNodeCollection.Last().km)
                    {
                        var pad = sortedLinearCoordinateCollection.Last().pad;
                        netElement.startPad = node.pad;
                        netElement.endPad = sortedLinearCoordinateCollection.Last().pad;
                        netElement.startNodeId = node.id;
                        netElement.endNodeId = node.originNodeID;
                        netElement.startKm = node.km;
                        netElement.endKm = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(lc => lc.pad == pad).linearCoordinate;
                    }
                    netElementCollection.Insert(netElement);
                }
            }
        }

        #endregion

        #region node

        /// <summary>
        /// Method to handle information about node from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleGleisknoten(LiteDatabase db, OleDbConnection connection)
        {
            // create database netElementCollection for Gleisknoten
            var collection = db.GetCollection<Node>("Nodes");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC31_KN";

            // loop, command, and read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of node
                            // id and name has the same value since it is not clear enough yet
                            var node = new Node()
                            {
                                id = reader["KNOTEN"].ToString(),
                                name = reader["KNOTEN"].ToString(),
                                parseNumber = Convert.ToInt16(reader["ParseLfdNr"]),
                                pad = reader["PAD"].ToString(),
                                originNodeID = reader["KN0"].ToString()
                            };

                            // assigning node direction information for planning purpose
                            // help to identify which route the node belongs to
                            // p.s in this case is either 1 and 0
                            if(db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == node.pad).unit == "km")
                            {
                                node.codeDirection = 1;
                            }
                            else if(db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == node.pad).unit == "m")
                            {
                                node.codeDirection = 0;
                            }

                            // assigning km in km and geo coordinate
                            node.km = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == node.pad).linearCoordinate;
                            node.coordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == node.pad);
                            
                            // assigning which object the node represented by
                            if (reader["KNBE"].ToString() == "Gleisende")
                            {
                                node.nodeType = 2;
                            }
                            else
                            {
                                node.nodeType = 1;
                            }

                            // handle destination nodes
                            node.destinationNodeID = new ArrayList();
                            for (int i = 1; i < 3; i++)
                            {
                                node.destinationNodeID.Add(reader["KN" + i].ToString());
                            }

                            collection.Insert(node);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }

        #endregion

        #region horizontal alignment

        /// <summary>
        /// Method to handle information about horizontal alignment from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleEntwurfselementLage(LiteDatabase db, OleDbConnection connection)
        {
            // create netElementCollection of horizontal alignment
            var collection = db.GetCollection<HorizontalAlignment>("HorizontalAlignments");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC21_EL";

            // loop, command, read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of horizontal alignment
                            var horizontalAlignment = new HorizontalAlignment()
                            {
                                startPad = reader["PAD1"].ToString(),
                                endPad = reader["PAD2"].ToString(),
                                alignmentTypeCode = Convert.ToInt32(reader["ELTYP"]),
                                alignmentTypeText = (horizontalAlignmentType)Convert.ToInt32(reader["ELTYP"]),
                                length = Convert.ToDouble(reader["ELPAR1"]),
                                initialRadius = Convert.ToDouble(reader["ELPAR2"]),
                                endRadius = Convert.ToDouble(reader["ELPAR3"]),
                                initialAzimuth = Convert.ToDouble(reader["ELARIWI"]),
                                startKM = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).linearCoordinate,
                                endKM = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD2"].ToString()).linearCoordinate,
                                unit = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).unit
                            };

                            // assigning code direction information for planning purpose
                            // help to identify which route the alignment belongs to
                            // p.s in this case is either 1 and 0
                            if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == horizontalAlignment.startPad).unit == "km")
                            {
                                horizontalAlignment.codeDirection = 1;
                            }
                            else if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == horizontalAlignment.startPad).unit == "m")
                            {
                                horizontalAlignment.codeDirection = 0;
                            }

                            // handle geo coordinates belong to alignment
                            handleCartesianCoordinateLine(db, horizontalAlignment);

                            collection.Insert(horizontalAlignment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }


        /// <summary>
        /// Extra method to handle geo coordinates in one alignment segment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="horizontalAlignment">mileage segment</param>
        private void handleCartesianCoordinateLine(LiteDatabase db, HorizontalAlignment horizontalAlignment)
        {
            var coordinateList = new List<CartesianCoordinate>();
            var firstCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == horizontalAlignment.startPad);
            var secondCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == horizontalAlignment.endPad);
            coordinateList.Add(firstCoordinate);
            coordinateList.Add(secondCoordinate);
            horizontalAlignment.lineCoordinates = coordinateList;
        }

        #endregion

        #region vertical alignment

        /// <summary>
        /// Method to handle information about vertical alignment from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleEntwurfselementHoehe(LiteDatabase db, OleDbConnection connection)
        {
            // create netElementCollection of vertical alignment
            var collection = db.GetCollection<VerticalAlignment>("VerticalAlignments");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC22_EH";

            // loop, command, read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of vertical alignment
                            var verticalAlignment = new VerticalAlignment()
                            {
                                id = "",
                                startPad = reader["PAD1"].ToString(),
                                endPad = reader["PAD2"].ToString(),
                                codeDirection = 99,
                                alignmentTypeCode = Convert.ToInt32(reader["EHTYP"]),
                                alignmentTypeText = (verticalAlignmentType)Convert.ToInt32(reader["EHTYP"]),
                                length = Convert.ToDouble(reader["EHPAR1"]),
                                initialSlope = Convert.ToDouble(reader["EHPAR2"]),
                                endSlope = Convert.ToDouble(reader["EHPAR3"]),
                                initialHeight = db.GetCollection<Height>("Heights").FindOne(x => x.pad == reader["PAD1"].ToString()).height,
                                endHeight = db.GetCollection<Height>("Heights").FindOne(x => x.pad == reader["PAD2"].ToString()).height,
                                startKM = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).linearCoordinate,
                                endKM = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD2"].ToString()).linearCoordinate,
                                unit = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).unit
                            };

                            // assigning code direction information for planning purpose
                            // help to identify which route the alignment belongs to
                            // p.s in this case is either 1 and 0
                            if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == verticalAlignment.startPad).unit == "km")
                            {
                                verticalAlignment.codeDirection = 1;
                            }
                            else if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == verticalAlignment.endPad).unit == "m")
                            {
                                verticalAlignment.codeDirection = 0;
                            }

                            // handle geo coordinates belong to alignment
                            handleCartesianCoordinateLine(db, verticalAlignment);

                            collection.Insert(verticalAlignment);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }


        /// <summary>
        /// Extra method to handle geo coordinates in one alignment segment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="verticalAlignment">mileage segment</param>
        private void handleCartesianCoordinateLine(LiteDatabase db, VerticalAlignment verticalAlignment)
        {
            var coordinateList = new List<CartesianCoordinate>();
            var firstCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == verticalAlignment.startPad);
            var secondCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == verticalAlignment.endPad);
            coordinateList.Add(firstCoordinate);
            coordinateList.Add(secondCoordinate);
            verticalAlignment.lineCoordinates = coordinateList;
        }

        #endregion

        #region alignment cant

        /// <summary>
        /// Method to handle information about alignment cant from mdb file
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="connection">established connection with mdb</param>
        public void handleEntwurfselementUeberhoehung(LiteDatabase db, OleDbConnection connection)
        {
            // create netElementCollection of cant alignment
            var collection = db.GetCollection<AlignmentCant>("AlignmentCants");

            // set query for reading process
            string strSQL = "SELECT * FROM X_ASC23_EU";

            // loop, command, read
            using (OleDbCommand command = new OleDbCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // create internal database object of alignment cant
                            var alignmentCant = new AlignmentCant()
                            {
                                startPad = reader["PAD1"].ToString(),
                                endPad = reader["PAD2"].ToString(),
                                alignmentTypeCode = Convert.ToInt32(reader["EUTYP"]),
                                alignmentTypeText = (alignmentCantType)Convert.ToInt32(reader["EUTYP"]),
                                length = Convert.ToDouble(reader["EUPAR1"]),
                                initialSuperelevation = Convert.ToDouble(reader["EUPAR2"]),
                                endSuperelevation = Convert.ToDouble(reader["EUPAR3"]),
                                startKm = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).linearCoordinate,
                                endKm = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD2"].ToString()).linearCoordinate,
                                unit = db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == reader["PAD1"].ToString()).unit
                            };

                            // assigning code direction information for planning purpose
                            // help to identify which route the alignment belongs to
                            // p.s in this case is either 1 and 0
                            if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == alignmentCant.startPad).unit == "km")
                            {
                                alignmentCant.codeDirection = 1;
                            }
                            else if (db.GetCollection<LinearCoordinate>("LinearCoordinates").FindOne(x => x.pad == alignmentCant.startPad).unit == "m")
                            {
                                alignmentCant.codeDirection = 0;
                            }

                            // handle geo coordinates belong to alignment
                            handleCartesianCoordinateLine(db, alignmentCant);

                            collection.Insert(alignmentCant);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                connection.Close();
            }
        }


        /// <summary>
        /// Extra method to handle geo coordinates in one alignment segment
        /// </summary>
        /// <param name="db">internal database</param>
        /// <param name="alignmentCant">mileage segment</param>
        private void handleCartesianCoordinateLine(LiteDatabase db, AlignmentCant alignmentCant)
        {
            var coordinateList = new List<CartesianCoordinate>();
            var firstCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == alignmentCant.startPad);
            var secondCoordinate = db.GetCollection<CartesianCoordinate>("CartesianCoordinates").FindOne(x => x.pad == alignmentCant.endPad);
            coordinateList.Add(firstCoordinate);
            coordinateList.Add(secondCoordinate);
            alignmentCant.lineCoordinates = coordinateList;
        }

        #endregion

    }
}