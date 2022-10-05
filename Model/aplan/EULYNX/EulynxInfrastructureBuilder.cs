using GeoJSON.Net.Feature;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.rsmTrack;
using System.Linq;
using aplan.core;

using db = Models.TopoModels.EULYNX.db;
using sig = Models.TopoModels.EULYNX.sig;
using AssociatedNetElement = Models.TopoModels.EULYNX.rsmCommon.AssociatedNetElement;

using static aplan.core.Helper;
using static aplan.core.JsonHandler;

namespace aplan.eulynx
{
    class EulynxInfrastructureBuilder
    {


        /// <summary>
        /// Method to define vehicle stops ("Prellbock") in the station from dataset.
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addVehicleStops(EulynxDataPrepInterface eulynx, Database database)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            int numeration = 1;

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collectionTE = db.GetCollection<database.TrackEntity>("TrackEntities");
                var collectionNO = db.GetCollection<database.Node>("Nodes").FindAll();
                var collectionNE = db.GetCollection<database.NetElement>("NetElements").FindAll();

                // iterate through collection
                foreach (var item in collectionNO)
                {
                    if (item.nodeType == 2)
                    {
                        // instantiate spot loctation
                        var spotLocation = new SpotLocation(){ id = generateUUID() };

                        foreach (var netElement in collectionNE)
                        {
                            if (item.id.Equals(netElement.startNodeId))
                            {
                                // instantiate intrinsic coordinate to be referred
                                var intrinsicCoordinate = new IntrinsicCoordinate() 
                                { 
                                    id = generateUUID(), 
                                    value = 0,
                                    coordinates = 
                                    {
                                        referCoordinate(rsmEntities, "LinearCoordinate" + item.km),
                                        referCoordinate(rsmEntities, "CartesianCoordinate" + item.coordinate.x)
                                    }
                                };
                                
                                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);

                                // instantiate associated net element
                                var associatedNetElement = new AssociatedNetElement();
                                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id}); // need coordinate? though it is the same in the km
                                associatedNetElement.isLocatedToSide = LateralSide.centre;
                                associatedNetElement.netElement = new tElementWithIDref { @ref = netElement.uuid };
                                associatedNetElement.appliesInDirection = ApplicationDirection.normal; // guess, it should be compared with rikz?
                                spotLocation.associatedNetElements.Add(associatedNetElement);
                            }
                            else if (item.id.Equals(netElement.endNodeId))
                            {
                                // instantiate intrinsic coordinate to be referred
                                var intrinsicCoordinate = new IntrinsicCoordinate()
                                {
                                    id = generateUUID(),
                                    value = 1,
                                    coordinates =
                                    {
                                        referCoordinate(rsmEntities, "LinearCoordinate" + item.km),
                                        referCoordinate(rsmEntities, "CartesianCoordinate" + item.coordinate.x)
                                    }
                                };

                                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);

                                // instantiate associated net element
                                var associatedNetElement = new AssociatedNetElement();
                                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id }); // need coordinate? though it is the same in the km
                                associatedNetElement.isLocatedToSide = LateralSide.centre;
                                associatedNetElement.netElement = new tElementWithIDref { @ref = netElement.uuid };
                                associatedNetElement.appliesInDirection = ApplicationDirection.normal; // guess, it should be compared with rikz?
                                spotLocation.associatedNetElements.Add(associatedNetElement);
                            }
                        }
                        rsmEntities.usesLocation.Add(spotLocation);

                        // RSM vehicle stop
                        var rsmVehicleStop = new VehicleStop
                        {
                            id = generateUUID(),
                            name = "VehicleStop" + numeration++,
                            locations =
                        {
                            new tElementWithIDref { @ref = spotLocation.id }
                        }
                        };
                        rsmEntities.ownsVehicleStop.Add(rsmVehicleStop);

                        // EULYNX vehicle stop mirrors the RSM vehicle stop
                        var euVehicleStop = new sig.VehicleStop
                        {
                            id = generateUUID(),
                            hasConfiguration = new Configuration
                            {
                                hasConfigurationProperty = {new db.Designation
                            {
                                id = generateUUID(),
                                localName = "Prellbock"
                            } }
                            },
                            refersToRsmVehicleStop = new tElementWithIDref { @ref = rsmVehicleStop.id }
                        };

                        var trackEntitiy = new database.TrackEntity()
                        {
                            uuid = rsmVehicleStop.id,
                            name = "trackEntity" + numeration,
                            trackEntityObject = "vehicle_stop",
                            trackEntityObjectElement = ""
                        };

                        collectionTE.Insert(trackEntitiy);

                        // dataprep

                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        /// <summary>
        /// Method to define point in the station from dataset.
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addTurnouts(EulynxDataPrepInterface eulynx, Database database)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            int numeration = 1;

            using (var db = database.accessDB())
            {
                // get collection
                var collectionTE = db.GetCollection<database.TrackEntity>("TrackEntities");
                var collectionNO = db.GetCollection<database.Node>("Nodes").FindAll();
                var collectionNE = db.GetCollection<database.NetElement>("NetElements").FindAll();
                

                foreach (var item in collectionNO)
                {
                    if(item.nodeType == 1)
                    {
                        // instantiate spot km
                        var spotLocation = new SpotLocation(){ id = generateUUID() };

                        foreach (var netElement in collectionNE)
                        {
                            if (item.id.Equals(netElement.startNodeId))
                            {
                                // instantiate intrinsic coordinate to be referred
                                var intrinsicCoordinate = new IntrinsicCoordinate()
                                {
                                    id = generateUUID(),
                                    value = 0,
                                    coordinates =
                                    {
                                        referCoordinate(rsmEntities, "LinearCoordinate" + item.km),
                                        referCoordinate(rsmEntities, "CartesianCoordinate" + item.coordinate.x)
                                    }
                                };
                                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);

                                var associatedNetElement = new AssociatedNetElement();
                                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id}); // need coordinate? though it is the same in the km
                                associatedNetElement.isLocatedToSide = LateralSide.centre;
                                associatedNetElement.netElement = new tElementWithIDref { @ref = netElement.uuid };
                                associatedNetElement.appliesInDirection = ApplicationDirection.undefined; // guess, it should be compared with rikz?
                                spotLocation.associatedNetElements.Add(associatedNetElement);
                            }
                            else if (item.id.Equals(netElement.endNodeId))
                            {
                                // instantiate intrinsic coordinate to be referred
                                var intrinsicCoordinate = new IntrinsicCoordinate()
                                {
                                    id = generateUUID(),
                                    value = 1,
                                    coordinates =
                                    {
                                        referCoordinate(rsmEntities, "LinearCoordinate" + item.km),
                                        referCoordinate(rsmEntities, "CartesianCoordinate" + item.coordinate.x)
                                    }
                                };
                                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);

                                var associatedNetElement = new AssociatedNetElement();
                                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id}); // need coordinate? though it is the same in the km
                                associatedNetElement.isLocatedToSide = LateralSide.centre;
                                associatedNetElement.netElement = new tElementWithIDref { @ref = netElement.uuid };
                                associatedNetElement.appliesInDirection = ApplicationDirection.undefined; // guess, it should be compared with rikz?
                                spotLocation.associatedNetElements.Add(associatedNetElement);
                            }
                        }

                        rsmEntities.usesLocation.Add(spotLocation);

                        
                        var turnout = new Turnout()
                        {
                            id = generateUUID(),
                            name = "Point" + numeration,
                            longname = "switch_tip",
                            orientation = TurnoutOrientation.Other, //the orientation is not define in dataset
                            locations =
                            {
                                new tElementWithIDref { @ref = spotLocation.id}
                            }

                        };

                        var trackEntity = new database.TrackEntity()
                        {
                            uuid = turnout.id,
                            name = "junctionEntity" + numeration,
                            trackEntityObject = "switch",
                            trackEntityObjectElement = "switch_tip"
                        };

                        collectionTE.Insert(trackEntity);

                        //turnout.handles.Add(new tElementWithIDref { @ref = "*function is under development*" });
                        //findPositionedRelations(rsmEntities, turnout, feature);
                        // false, should spot km
                        turnout.origin = new tElementWithIDref { @ref = spotLocation.id };
                        //turnout.turnoutArea => no clue yet
                        rsmEntities.ownsPoint.Add(turnout);
                        numeration++;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


        /* need to be fix*/
        /// <summary>
        /// Method to define point in the station from dataset.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="feature"></param>
        /// <param name="turnout"></param>
        public static void findPositionedRelations(RsmEntities rsmEntities, Turnout turnout, Feature feature)
        {
            var id = fetchIndividualValue("ID", feature).ToString();
            foreach (PositionedRelation positionedRelation in rsmEntities.usesTrackTopology.usesPositionedRelation)
            {
                foreach (string nodeID in positionedRelation.nodesID)
                {
                    if (id.Equals(nodeID))
                    {
                        turnout.handles.Add(new tElementWithIDref { @ref = positionedRelation.id });
                    }
                    else
                    {
                        continue;
                    }
                }
            }
        }


    }
}
