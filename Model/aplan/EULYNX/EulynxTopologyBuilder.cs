using aplan.core;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using System.Linq;
using LiteDB;

using static aplan.core.Helper;

namespace aplan.eulynx
{
    class EulynxTopologyBuilder
    {


        /// <summary>
        /// Method to associate intrinsic coordinates of cartesian coordinates in one object.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="item"></param>
        /// /// <param name="db"></param>
        private static AssociatedPositioning addCartesianAssociatedPositioning(LiteDatabase db, database.NetElement item, RsmEntities rsmEntities)
        {
            // geometric associated positioning
            //var associatedPositioning = new AssociatedPositioning();
            //var cartesianCoordinateCollection = db.GetCollection<database.CartesianCoordinate>("CartesianCoordinates").Find(x => x.name.Equals("CartesianCoordinate" + item.lineCoordinates[0]));
            //double total = cartesianCoordinateCollection.Count();
            //double increment = 1 / total;
            //double value = 0;
            //foreach (var coordinate in cartesianCoordinateCollection)
            //{
            //    // the intrinsic coordinates are wrong, still need to be fixed
            //    // because it is not always comes up with the same gap
            //    var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID()};
            //    intrinsicCoordinate.value = value;
            //    intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, coordinate.name + "_" + coordinate.index));
            //    associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
            //    value += increment;
            //}
            //return associatedPositioning;
            
            var associatedPositioning = new AssociatedPositioning();
            var total = item.lineCoordinates.Count;
            double increment = 1.0 / total;
            double value = 0;
            for (int i = 0; i < total; i++)
            {
                // the intrinsic coordinates are wrong, still need to be fixed
                // because it is not always comes up with the same gap
                var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };
                intrinsicCoordinate.value = value;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + item.lineCoordinates[i].x));
                associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
                value += increment;
            }
            return associatedPositioning;
        }


        /// <summary>
        /// Method to associate intrinsic coordinates of linear coordinates in one object.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="item"></param>
        private static AssociatedPositioning addLinearAssociatedPositioning(database.NetElement item, RsmEntities rsmEntities)
        {
            var associatedPositioning = new AssociatedPositioning();
            var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID()};
            if (item.codeDirection == 1 || item.codeDirection == 2)
            {
                intrinsicCoordinate.id = generateUUID();
                intrinsicCoordinate.value = 0;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKm));
                associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };
                intrinsicCoordinate.id = generateUUID();
                intrinsicCoordinate.value = 1;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKm));
                associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
            }
            else
            {
                intrinsicCoordinate.id = generateUUID();
                intrinsicCoordinate.value = 0;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKm));
                associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };
                intrinsicCoordinate.id = generateUUID();
                intrinsicCoordinate.value = 1;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKm));
                associatedPositioning.intrinsicCoordinates.Add(intrinsicCoordinate);
            }

            return associatedPositioning;
        }


        /// <summary>
        /// Method to define all linear element appears in the station
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="database"></param>
        public static void addLinearElement(EulynxDataPrepInterface eulynx, Database database)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var m = referUnit(rsmEntities, "meter");
            rsmEntities.usesTrackTopology ??= new Topology() { id = generateUUID() };
            int numeration = 1;

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<database.NetElement>("NetElements").FindAll();
                if (collectionIsNull(collection))
                   return;

                foreach (var item in collection)
                {

                    if(item.codeDirection == 4)
                    {
                        var linearElement = new LinearElementWithLength()
                        {
                            id = generateUUID(),
                            name = "Side-" + numeration,
                            //these attributes below are created in purpose to build custom function
                            //localID = item.id,
                            //fromNodeID = item.startNodeId,
                            //toNodeID = item.endNodeId,
                            //direction = item.codeDirection
                        };
                        //linear associated positioning 
                        linearElement.associatedPositioning.Add(addLinearAssociatedPositioning(item, rsmEntities));

                        // geometric associated positioning
                        linearElement.associatedPositioning.Add(addCartesianAssociatedPositioning(db, item, rsmEntities));

                        linearElement.elementLength = new ElementLength { id = generateUUID(), quantiy = { new Length { value = item.length, unit = m } } };

                        rsmEntities.usesTrackTopology.usesNetElement.Add(linearElement);

                        //update uuid
                        item.uuid = linearElement.id;
                    }
                    else
                    {
                        var linearElement = new LinearElementWithLength
                        {
                            id = generateUUID(),
                            name = "Main-" + numeration,
                            //these attributes below are created in purpose to build custom function
                            //localID = item.id,
                            //fromNodeID = item.startNodeId,
                            //toNodeID = item.endNodeId,
                            //direction = item.codeDirection
                            //isMainTrack = true
                        };

                        //linear associated positioning 
                        linearElement.associatedPositioning.Add(addLinearAssociatedPositioning(item, rsmEntities));

                        // geometric associated positioning
                        linearElement.associatedPositioning.Add(addCartesianAssociatedPositioning(db, item, rsmEntities));

                        linearElement.elementLength = new ElementLength { id = generateUUID(), quantiy = { new Length { value = item.length, unit = m } } };

                        rsmEntities.usesTrackTopology.usesNetElement.Add(linearElement);

                        //update uuid
                        item.uuid = linearElement.id;
                    }
                    numeration++;
                    
                    // update item in database
                    db.GetCollection<database.NetElement>("NetElements").Update(item);
                }
            }     
        }


        /// <summary>
        /// Method to add all possible relations between linear elements.
        /// </summary>
        /// <param name="eulynx"></param>
        public static void addRelations(EulynxDataPrepInterface eulynx, Database database)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            string fromNodeID, toNodeID, relationName;

            using (var db = database.accessDB())
            {
                // get collection
                var collection1 = db.GetCollection<database.NetElement>("NetElements").FindAll();
                var collection2 = db.GetCollection<database.NetElement>("NetElements").FindAll();
                if (collectionIsNull(collection1))
                    return;


                foreach (var item1 in collection1)
                {
                    fromNodeID = item1.endNodeId;
                    toNodeID = item1.startNodeId;

                    foreach (var item2 in collection2)
                    {
                        if (fromNodeID.Equals(item2.startNodeId))
                        {
                            PositionedRelation positionedRelation = new PositionedRelation()
                            {
                                id = generateUUID(),
                                name = item1.Id + "--" + item2.Id,
                                positionOnA = Usage.end,
                                positionOnB = Usage.start,
                                elementA = new tElementWithIDref { @ref = item1.uuid },
                                elementB = new tElementWithIDref { @ref = item2.uuid },
                                navigability = Navigability.Both
                            };
                            positionedRelation.nodesID.Add(fromNodeID);
                            if (!rsmEntities.usesTrackTopology.usesPositionedRelation.Exists(item => item.name.Equals(positionedRelation.name)))
                            {
                                rsmEntities.usesTrackTopology.usesPositionedRelation.Add(positionedRelation);
                            }
                        }
                        else if (toNodeID.Equals(item2.endNodeId))
                        {
                            PositionedRelation positionedRelation = new PositionedRelation()
                            {
                                id = generateUUID(),
                                name = item2.Id + "--" + item1.Id,
                                positionOnA = Usage.end,
                                positionOnB = Usage.start,
                                elementA = new tElementWithIDref { @ref = item2.uuid },
                                elementB = new tElementWithIDref { @ref = item1.uuid },
                                navigability = Navigability.Both
                            };
                            positionedRelation.nodesID.Add(toNodeID);
                            if (!rsmEntities.usesTrackTopology.usesPositionedRelation.Exists(item => item.name.Equals(positionedRelation.name)))
                            {
                                rsmEntities.usesTrackTopology.usesPositionedRelation.Add(positionedRelation);
                            }
                        }
                    }

                }
            }
        }


    }
}
