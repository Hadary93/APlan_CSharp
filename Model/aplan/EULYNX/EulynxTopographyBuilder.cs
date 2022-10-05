using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using System.Linq;
using aplan.core;
using aplan.database;
using LiteDB;

using AlignmentCantSegment = Models.TopoModels.EULYNX.rsmCommon.AlignmentCantSegment;
using HorizontalAlignmentSegment = Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment;
using VerticalAlignmentSegment = Models.TopoModels.EULYNX.rsmCommon.VerticalAlignmentSegment;
using LinearCoordinate = Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate;
using ElevationAndInclination = Models.TopoModels.EULYNX.rsmCommon.ElevationAndInclination;
using Cant = Models.TopoModels.EULYNX.rsmCommon.Cant;
using AssociatedNetElement = Models.TopoModels.EULYNX.rsmCommon.AssociatedNetElement;

using static aplan.core.Helper;


namespace aplan.eulynx
{

    class EulynxTopographyBuilder
    {


        /// <summary>
        /// Method to add horizontal alignments.
        /// </summary>
        /// <param name="eulynx">Eulynx container</param>
        /// <param name="database">Internal database</param>
        public static void addHorizontalAlignment(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var m = referUnit(rsmEntities, "meter");
            var km = referUnit(rsmEntities, "kilometer");
            var gon = referUnit(rsmEntities, "gon");

            // accessing database
            using (var db = database.accessDB())
            {

                // get collection
                var collection = db.GetCollection<database.HorizontalAlignment>("HorizontalAlignments").FindAll();

                // iterate through database
                foreach (var item in collection)
                {
                    // get alignment type
                    var type = item.alignmentTypeText;

                    switch (type)
                    {
                        case horizontalAlignmentType.Line:
                            var horizontalSegmentLine = new HorizontalSegmentLine
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialAzimuth = new Angle { value = item.initialAzimuth, unit = gon },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes 
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };


                            //create two polyline

                            //linear line
                            horizontalSegmentLine.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentLine.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(horizontalSegmentLine, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(horizontalSegmentLine.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentLine);
                            break;
                        case horizontalAlignmentType.Arc:
                            var horizontalSegmentArc = new HorizontalSegmentArc()
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                radius = new Length { value = item.initialRadius, unit = m },
                                initialAzimuth = new Angle { value = item.initialAzimuth, unit = gon },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            
                            //create two polyline

                            //linear line
                            horizontalSegmentArc.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentArc.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(horizontalSegmentArc, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(horizontalSegmentArc.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentArc);
                            break;
                        case horizontalAlignmentType.Clothoid:
                            var horizontalSegmentClothoid = new HorizontalSegmentTransition
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialRadius = new Length { value = item.initialRadius, unit = m },
                                finalRadius = new Length { value = item.endRadius, unit = m },
                                type = item.alignmentTypeText.ToString(),
                                initialAzimuth = new Angle { value = item.initialAzimuth, unit = gon },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentClothoid.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentClothoid.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(horizontalSegmentClothoid, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(horizontalSegmentClothoid.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentClothoid);
                            break;
                        case horizontalAlignmentType.Bloss_curve:
                            HorizontalSegmentTransition horizontalSegmentBlossCurve = new HorizontalSegmentTransition
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialRadius = new Length { value = item.initialRadius, unit = m },
                                finalRadius = new Length { value = item.endRadius, unit = m },
                                type = item.alignmentTypeText.ToString(),
                                initialAzimuth = new Angle { value = item.initialAzimuth, unit = gon },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentBlossCurve.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentBlossCurve.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(horizontalSegmentBlossCurve, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(horizontalSegmentBlossCurve.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentBlossCurve);
                            break;
                        default:
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// Method to associate linear km with any netelements.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database horizontal alignment segment</param>
        /// <param name="segment">Rsm horizontal alignment segment</param>
        private static void addAssociatedNetElements(HorizontalAlignmentSegment segment, RsmEntities rsmEntities, database.HorizontalAlignment item, LiteDatabase database)
        {
            //help variables
            string initialSegmentRef, endSegmentRef;
            double initialSegmentValue, endSegmentValue;
            LinearElementWithLength currentLinearElement;
            Length length;
            var direction = item.codeDirection;

            //create objects
            var associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
            var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };


            //add application direction according to RIKZ
            if (direction == 1) { associatedNetElement.appliesInDirection = ApplicationDirection.normal; }
            else if (direction == 2) { associatedNetElement.appliesInDirection = ApplicationDirection.reverse; }
            else { associatedNetElement.appliesInDirection = ApplicationDirection.undefined; }

            //first linear element
            currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
            associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

            //intrinsic coordinate calculation
            initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
            endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
            initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
            endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
            length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
            intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

            //find coordinate and add as bound
            string name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
            intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
            rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
            associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });


            //add first associated net element
            segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);

            //check direction


            //if segment still belongs to same net element
            if (segment.endSegment.value >= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value
                && segment.endSegment.value <= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value)
            {
                //find new intrinsci coordinate
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };
                intrinsicCoordinate.value = (segment.endSegment.value - initialSegmentValue) / length.value;
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));

                //add new intrinsic coordinate to previous associated net element
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                segment.hasLinearLocation.associatedNetElements[0].bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });
            }
            //if segment belongs to other net element
            else
            {
                //create new objects
                associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };

                //next linear element
                currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
                associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

                //intrinsic coordinate calculation
                initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
                endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
                length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
                intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

                //find the coordinate
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

                //add new associated net element
                segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);

            }
        }

        /// <summary>
        /// Method to add linear polyline for horizontal alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database horizontal alignment segment</param>
        private static PolyLine addLinearPolyLine(RsmEntities rsmEntities, database.HorizontalAlignment item)
        {
            //linear line
            PolyLine linearPolyLine = new PolyLine();
            linearPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System");
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKM));
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKM));
            return linearPolyLine;
        }

        /// <summary>
        /// Method to add cartesian polyline for horizontal alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database horizontal alignment segment</param>
        /// <param name="db">APlan internal lite database</param>
        private static PolyLine addCartesianPolyLine(RsmEntities rsmEntities, LiteDatabase db, database.HorizontalAlignment item)
        {
            //geo line
            PolyLine cartesianPolyLine = new PolyLine();
            cartesianPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System");

            foreach (var coor in item.lineCoordinates)
            {
                cartesianPolyLine.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + coor.x));
            }
            return cartesianPolyLine;
        }

        /// <summary>
        /// Method to find linear element associated to particular horizontal alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="segment">Rsm horizontal alignment segment</param>
        /// <param name="direction">Direction of horizontal alginment segment</param>
        /// <param name="database">APlan internal lite database</param>
        private static LinearElementWithLength findLinearElement(HorizontalAlignmentSegment segment, RsmEntities rsmEntities, int direction, LiteDatabase database)
        {
            // get collection
            var collection = database.GetCollection<database.NetElement>("NetElements").FindAll();
            if (collectionIsNull(collection))
                return null;

            foreach (var item in collection)
            {
                //check the direction
                if (item.codeDirection == direction)
                {
                    //check if the initial segment is between the start and end of one edge
                    if (segment.initialSegment.value >= item.startKm && segment.initialSegment.value <= item.endKm)
                    {
                        return (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id.Equals(item.uuid));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Method to add vertical alignments.
        /// </summary>
        /// <param name="eulynx">Eulynx container</param>
        /// <param name="database">Internal database</param>
        public static void addVerticalAlignment(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var m = referUnit(rsmEntities, "meter");
            var km = referUnit(rsmEntities, "kilometer");
            var promille = referUnit(rsmEntities, "per-mille");

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<database.VerticalAlignment>("VerticalAlignments").FindAll();

                // iterate through database
                foreach (var item in collection)
                {
                    // get alignment type
                    var type = item.alignmentTypeText;

                    switch (type)
                    {
                        case verticalAlignmentType.Line:
                            var verticalSegmentLine = new VerticalSegmentLine
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                type = item.alignmentTypeText.ToString(),
                                // these are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //Elevation
                            addElevation(rsmEntities, item.initialSlope);
                            addElevation(rsmEntities, item.endSlope);

                            verticalSegmentLine.initialElevation = referElevation(rsmEntities, item.initialSlope);
                            verticalSegmentLine.finalElevation = referElevation(rsmEntities, item.endSlope);

                            //create two polyline

                            //linear line
                            verticalSegmentLine.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            verticalSegmentLine.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(verticalSegmentLine, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(verticalSegmentLine.hasLinearLocation);
                            rsmEntities.usesTopography.usesVerticalAlignmentSegment.Add(verticalSegmentLine);
                            break;
                        case verticalAlignmentType.Arc:
                            var verticalSegmentArc = new VerticalSegmentArc
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                type = item.alignmentTypeText.ToString(),
                                // these are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //Elevation
                            addElevation(rsmEntities, item.initialSlope);
                            addElevation(rsmEntities, item.endSlope);

                            verticalSegmentArc.initialElevation = referElevation(rsmEntities, item.initialSlope);
                            verticalSegmentArc.finalElevation = referElevation(rsmEntities, item.endSlope);

                            //create two polyline

                            //linear line
                            verticalSegmentArc.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            verticalSegmentArc.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(verticalSegmentArc, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(verticalSegmentArc.hasLinearLocation);
                            rsmEntities.usesTopography.usesVerticalAlignmentSegment.Add(verticalSegmentArc);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Method to add elevation.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="elevationValue">Elevation value</param>
        private static void addElevation(RsmEntities rsmEntities, double elevationValue)
        {
            var promille = referUnit(rsmEntities, "per-mille");
            var elevation = new ElevationAndInclination();
            elevation.id = generateUUID();
            elevation.inclination = new Gradient { value = elevationValue, unit = promille };
            if (!rsmEntities.usesTopography.usesElevationAndInclination.Exists(x => x.inclination.value == elevation.inclination.value))
                rsmEntities.usesTopography.usesElevationAndInclination.Add(elevation);
        }

        /// <summary>
        /// Method to associate linear km with any netelements.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database vertical alignment segment</param>
        /// <param name="segment">Rsm vertical alignment segment</param>
        private static void addAssociatedNetElements(VerticalAlignmentSegment segment, RsmEntities rsmEntities, database.VerticalAlignment item, LiteDatabase database)
        {
            //help variables
            string initialSegmentRef, endSegmentRef;
            double initialSegmentValue, endSegmentValue;
            LinearElementWithLength currentLinearElement;
            Length length;
            var direction = item.codeDirection;

            //create objects
            var associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
            var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };

            //add application direction according to RIKZ
            if (direction == 1) { associatedNetElement.appliesInDirection = ApplicationDirection.normal; }
            else if (direction == 2) { associatedNetElement.appliesInDirection = ApplicationDirection.reverse; }
            else { associatedNetElement.appliesInDirection = ApplicationDirection.undefined; }

            //first linear element
            currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
            associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

            //intrinsic coordinate calculation
            initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
            endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
            initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
            endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
            length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
            intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

            //find coordinate and add as bound
            string name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
            intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
            rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
            associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

            //add first associated net element
            segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);


            //if segment still belongs to same net element
            if (segment.endSegment.value >= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value
                && segment.endSegment.value <= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value)
            {
                //find new intrinsci coordinate
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() }; ;
                intrinsicCoordinate.value = (segment.endSegment.value - initialSegmentValue) / length.value;
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));

                //add new intrinsic coordinate to previous associated net element
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                segment.hasLinearLocation.associatedNetElements[0].bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });
            }
            //if segment belongs to other net element
            else
            {
                //create new objects
                associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };

                //next linear element
                currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
                associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

                //intrinsic coordinate calculation
                initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
                endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
                length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
                intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

                //find the coordinate
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

                //add new associated net element
                segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);

            }


        }

        /// <summary>
        /// Method to add linear polyline for vertical alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database vertical alignment segment</param>
        private static PolyLine addLinearPolyLine(RsmEntities rsmEntities, database.VerticalAlignment item)
        {
            //linear line
            PolyLine linearPolyLine = new PolyLine();
            linearPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System");
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKM));
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKM));
            return linearPolyLine;
        }

        /// <summary>
        /// Method to add cartesian polyline for vertical alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database vertical alignment segment</param>
        /// <param name="db">APlan internal lite database</param>
        private static PolyLine addCartesianPolyLine(RsmEntities rsmEntities, LiteDatabase db, database.VerticalAlignment item)
        {
            //geo line
            PolyLine cartesianPolyLine = new PolyLine();
            cartesianPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System");

            foreach(var coor in item.lineCoordinates)
            {
                cartesianPolyLine.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + coor.x));
            }
            return cartesianPolyLine;
        }

        /// <summary>
        /// Method to find linear element associated to particular vertical alignment segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="segment">Rsm horizontal alignment segment</param>
        /// <param name="direction">Direction of horizontal alginment segment</param>
        /// <param name="database">APlan internal lite database</param>
        private static LinearElementWithLength findLinearElement(VerticalAlignmentSegment segment, RsmEntities rsmEntities, int direction, LiteDatabase database)
        {
            // get collection
            var collection = database.GetCollection<database.NetElement>("NetElements").FindAll();
            if (collectionIsNull(collection))
                return null;

            foreach (var item in collection)
            {
                //check the direction
                if (item.codeDirection == direction)
                {
                    //check if the initial segment is between the start and end of one edge
                    if (segment.initialSegment.value >= item.startKm && segment.initialSegment.value <= item.endKm)
                    {
                        return (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id.Equals(item.uuid));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Method to add alignment cants.
        /// </summary>
        /// <param name="eulynx">Eulynx container</param>
        /// <param name="database">Internal database</param>
        public static void addAlignmentCant(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var km = referUnit(rsmEntities, "kilometer");

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<database.AlignmentCant>("AlignmentCants").FindAll();

                // iterate through database
                foreach (var item in collection)
                {
                    // get alignment type
                    var type = item.alignmentTypeText;
                    
                    switch (type)
                    {
                        case alignmentCantType.Line:
                            var segmentCantLine = new SegmentCantLine()
                            {
                                id = generateUUID(),
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // these are extra attributes
                                initialSegment = new Length { value = item.startKm, unit = km },
                                endSegment = new Length { value = item.endKm, unit = km }
                            };

                            // Cant
                            var cantLine = new Cant { id = generateUUID(), superelevation = new Length { value = item.initialSuperelevation } };
                            rsmEntities.usesTopography.usesCant.Add(cantLine);

                            segmentCantLine.hasInitialCant = new tElementWithIDref { @ref = cantLine.id };
                            

                            // create two polyline

                            //linear line
                            segmentCantLine.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            segmentCantLine.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(segmentCantLine, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(segmentCantLine.hasLinearLocation);
                            rsmEntities.usesTopography.usesAlignmentCantSegment.Add(segmentCantLine);
                            break;
                        case alignmentCantType.Linear_slope:
                            var segmentCantTransitionLinear = new SegmentCantTransition()
                            {
                                id = generateUUID(),
                                transitionShape = new TransitionShape { id = generateUUID(), isOfTransitionType = TransitionTypes.linear_slope },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // these are extra attributes
                                initialSegment = new Length { value = item.startKm, unit = km },
                                endSegment = new Length { value = item.endKm, unit = km }
                            };

                            // Cant
                            // start
                            var cantLinear = new Cant { id = generateUUID(), superelevation = new Length { value = item.initialSuperelevation } };
                            rsmEntities.usesTopography.usesCant.Add(cantLinear);
                            segmentCantTransitionLinear.hasInitialCant = new tElementWithIDref { @ref = cantLinear.id };

                            //end
                            cantLinear = new Cant { id = generateUUID(), superelevation = new Length { value = item.endSuperelevation } };
                            rsmEntities.usesTopography.usesCant.Add(cantLinear);
                            segmentCantTransitionLinear.hasInitialCant = new tElementWithIDref { @ref = cantLinear.id };



                            // create two polyline

                            //linear line
                            segmentCantTransitionLinear.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            segmentCantTransitionLinear.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(segmentCantTransitionLinear, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(segmentCantTransitionLinear.hasLinearLocation);
                            rsmEntities.usesTopography.usesAlignmentCantSegment.Add(segmentCantTransitionLinear);
                            break;
                        case alignmentCantType.Bloss_slope:
                            var segmentCantTransitionBlossRampe = new SegmentCantTransition()
                            {
                                id = generateUUID(),
                                transitionShape = new TransitionShape { id = generateUUID(), isOfTransitionType = TransitionTypes.bloss_slope },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // these are extra attributes
                                initialSegment = new Length { value = item.startKm, unit = km },
                                endSegment = new Length { value = item.endKm, unit = km }
                            };

                            //  Cant

                            // start
                            var cantBlossrampe = new Cant { id = generateUUID(), superelevation = new Length { value = item.initialSuperelevation } };
                            rsmEntities.usesTopography.usesCant.Add(cantBlossrampe);
                            segmentCantTransitionBlossRampe.hasInitialCant = new tElementWithIDref { @ref = cantBlossrampe.id };

                            //end
                            cantBlossrampe = new Cant { id = generateUUID(), superelevation = new Length { value = item.endSuperelevation } };
                            rsmEntities.usesTopography.usesCant.Add(cantBlossrampe);
                            segmentCantTransitionBlossRampe.hasInitialCant = new tElementWithIDref { @ref = cantBlossrampe.id };

                            // create two polyline

                            //linear line
                            segmentCantTransitionBlossRampe.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            segmentCantTransitionBlossRampe.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            if (item.codeDirection != 0)
                                addAssociatedNetElements(segmentCantTransitionBlossRampe, rsmEntities, item, db);

                            rsmEntities.usesLocation.Add(segmentCantTransitionBlossRampe.hasLinearLocation);
                            rsmEntities.usesTopography.usesAlignmentCantSegment.Add(segmentCantTransitionBlossRampe);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        
        /// <summary>
        /// Method to associate linear km with any netelements.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="item"></param>
        /// <param name="segment"></param>
        private static void addAssociatedNetElements(AlignmentCantSegment segment, RsmEntities rsmEntities, database.AlignmentCant item, LiteDatabase database)
        {
            //help variables
            string initialSegmentRef, endSegmentRef;
            double initialSegmentValue, endSegmentValue;
            LinearElementWithLength currentLinearElement;
            Length length;
            var direction = item.codeDirection;

            //create objects
            var associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
            var intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };

            //add application direction according to RIKZ
            if (direction == 1) { associatedNetElement.appliesInDirection = ApplicationDirection.normal; }
            else if (direction == 2) { associatedNetElement.appliesInDirection = ApplicationDirection.reverse; }
            else { associatedNetElement.appliesInDirection = ApplicationDirection.undefined; }

            //first linear element
            currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
            associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

            //intrinsic coordinate calculation
            initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
            endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
            initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
            endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
            length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
            intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

            //find coordinate and add as bound
            string name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
            intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
            rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
            associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

            //add first associated net element
            segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);


            //if segment still belongs to same net element
            if (segment.endSegment.value >= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value
                && segment.endSegment.value <= ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value)
            {
                //find new intrinsci coordinate
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() }; ;
                intrinsicCoordinate.value = (segment.endSegment.value - initialSegmentValue) / length.value;
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));

                //add new intrinsic coordinate to previous associated net element
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                segment.hasLinearLocation.associatedNetElements[0].bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });
            }
            //if segment belongs to other net element
            else
            {
                //create new objects
                associatedNetElement = new AssociatedNetElement { isLocatedToSide = LateralSide.centre };
                intrinsicCoordinate = new IntrinsicCoordinate { id = generateUUID() };

                //next linear element
                currentLinearElement = findLinearElement(segment, rsmEntities, direction, database);
                associatedNetElement.netElement = new tElementWithIDref { @ref = currentLinearElement.id };

                //intrinsic coordinate calculation
                initialSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                endSegmentRef = currentLinearElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                initialSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialSegmentRef))).measure.value;
                endSegmentValue = (double)((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endSegmentRef))).measure.value;
                length = (Length)currentLinearElement.elementLength.quantiy.Find(item => item is Length);
                intrinsicCoordinate.value = (segment.initialSegment.value - initialSegmentValue) / length.value;

                //find the coordinate
                name = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => ((LinearCoordinate)item).measure.value == segment.initialSegment.value).name;
                intrinsicCoordinate.coordinates.Add(referCoordinate(rsmEntities, name));
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
                associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

                //add new associated net element
                segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);

            }
        }


        /// <summary>
        /// Method to add linear polyline for alignment cant segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database alignment cant segment</param>
        private static PolyLine addLinearPolyLine(RsmEntities rsmEntities, database.AlignmentCant item)
        {
            //linear line
            PolyLine linearPolyLine = new PolyLine();
            linearPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System");
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKm));
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKm));
            return linearPolyLine;
        }

        /// <summary>
        /// Method to add cartesian polyline for alignment cant segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database alignment cant segment</param>
        /// <param name="db">APlan internal lite database</param>
        private static PolyLine addCartesianPolyLine(RsmEntities rsmEntities, LiteDatabase db, database.AlignmentCant item)
        {
            //geo line
            PolyLine cartesianPolyLine = new PolyLine();
            cartesianPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System");

            foreach (var coor in item.lineCoordinates)
            {
                cartesianPolyLine.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + coor.x));
            }
            return cartesianPolyLine;
        }

        /// <summary>
        /// Method to find linear element associated to particular alignment cant segment.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="segment">Rsm alignment cant segment</param>
        /// <param name="direction">Direction of horizontal alginment segment</param>
        /// <param name="database">APlan internal lite database</param>
        private static LinearElementWithLength findLinearElement(AlignmentCantSegment segment, RsmEntities rsmEntities, int direction, LiteDatabase database)
        {
            // get collection
            var collection = database.GetCollection<database.NetElement>("NetElements").FindAll();
            if (collectionIsNull(collection))
                return null;

            foreach (var item in collection)
            {
                //check the direction
                if (item.codeDirection == direction)
                {
                    //check if the initial segment is between the start and end of one edge
                    if (segment.initialSegment.value >= item.startKm && segment.initialSegment.value <= item.endKm)
                    {
                        return (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id.Equals(item.uuid));
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Method to add alignment curve.
        /// </summary>
        /// <param name="eulynx">Eulynx container</param>
        /// <param name="database">Internal database</param>
        public static void addAlignmentCurve(EulynxDataPrepInterface eulynx, Database database)
        {
            // rsm container
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var m = referUnit(rsmEntities, "meter");
            var km = referUnit(rsmEntities, "kilometer");

            // accessing database
            using (var db = database.accessDB())
            {
                // get collection
                var collection = db.GetCollection<database.Mileage>("Mileage").FindAll();

                // iterate through database
                foreach (var item in collection)
                {
                    // get alignment type
                    var type = item.alignmentTypeText;

                    switch (type)
                    {
                        case horizontalAlignmentType.Line:
                            var horizontalSegmentLine = new HorizontalSegmentLine
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes 
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline
                            
                            //linear line
                            horizontalSegmentLine.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentLine.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // think about mdb case where it doesn't has code direction
                            // add associated net element
                            //if (item.codeDirection != 0)
                            //    addAssociatedNetElements(horizontalSegmentLine, rsmEntities, item);

                            rsmEntities.usesLocation.Add(horizontalSegmentLine.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentLine);
                            break;
                        case horizontalAlignmentType.Arc:
                            var horizontalSegmentArc = new HorizontalSegmentArc()
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                radius = new Length { value = item.initialRadius, unit = m },
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentArc.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentArc.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // associated net element need to bes olved
                            //addAssociatedNetElementsMileage(horizontalSegmentArc, rsmEntities);

                            rsmEntities.usesLocation.Add(horizontalSegmentArc.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentArc);
                            break;
                        case horizontalAlignmentType.Clothoid:
                            var horizontalSegmentClothoid = new HorizontalSegmentTransition
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialRadius = new Length { value = item.initialRadius, unit = m },
                                finalRadius = new Length { value = item.endRadius, unit = m },
                                type = item.alignmentTypeText.ToString(),
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentClothoid.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentClothoid.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // ass net element need to be solved
                            //addAssociatedNetElementsMileage(horizontalSegmentClothoid, rsmEntities);

                            rsmEntities.usesLocation.Add(horizontalSegmentClothoid.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentClothoid);
                            break;
                        case horizontalAlignmentType.Bloss_curve:
                            HorizontalSegmentTransition horizontalSegmentBlossCurve = new HorizontalSegmentTransition
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialRadius = new Length { value = item.initialRadius, unit = m },
                                finalRadius = new Length { value = item.endRadius, unit = m },
                                type = item.alignmentTypeText.ToString(),
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentBlossCurve.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentBlossCurve.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // ass net element need to be solved
                            //addAssociatedNetElementsMileage(horizontalSegmentBlossCurve, rsmEntities);

                            rsmEntities.usesLocation.Add(horizontalSegmentBlossCurve.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentBlossCurve);
                            break;
                        default:
                            HorizontalSegmentTransition horizontalSegmentDefault = new HorizontalSegmentTransition
                            {
                                id = generateUUID(),
                                length = new Length { value = item.length, unit = m },
                                initialRadius = new Length { value = item.initialRadius, unit = m },
                                finalRadius = new Length { value = item.endRadius, unit = m },
                                type = item.alignmentTypeText.ToString(),
                                hasLinearLocation = new LinearLocation { id = generateUUID() },
                                // These are extra attributes
                                localID = item.id,
                                initialSegment = new Length { value = item.startKM, unit = km },
                                endSegment = new Length { value = item.endKM, unit = km }
                            };

                            //create two polyline

                            //linear line
                            horizontalSegmentDefault.hasLinearLocation.polyLines.Add(addLinearPolyLine(rsmEntities, item));

                            //geo line
                            horizontalSegmentDefault.hasLinearLocation.polyLines.Add(addCartesianPolyLine(rsmEntities, db, item));

                            // ass net element need to be solved
                            //addAssociatedNetElementsMileage(horizontalSegmentDefault, rsmEntities);

                            rsmEntities.usesLocation.Add(horizontalSegmentDefault.hasLinearLocation);
                            rsmEntities.usesTopography.usesHorizontalAlignmentSegment.Add(horizontalSegmentDefault);
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Method to add linear polyline for mileage.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database mileage</param>
        private static PolyLine addLinearPolyLine(RsmEntities rsmEntities, database.Mileage item)
        {
            //linear line
            PolyLine linearPolyLine = new PolyLine();
            linearPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System");
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.startKM));
            linearPolyLine.coordinates.Add(referCoordinate(rsmEntities, "LinearCoordinate" + item.endKM));
            return linearPolyLine;
        }

        /// <summary>
        /// Method to add cartesian polyline for mileage.
        /// </summary>
        /// <param name="rsmEntities">Rsm entities container</param>
        /// <param name="item">APlan database mileage</param>
        /// <param name="db">APlan internal lite database</param>
        private static PolyLine addCartesianPolyLine(RsmEntities rsmEntities, LiteDatabase db, database.Mileage item)
        {
            //geo line
            PolyLine cartesianPolyLine = new PolyLine();
            cartesianPolyLine.positioningSystem = referPositioningSystem(rsmEntities, "Grid Reference System");

            //var cartesianCoordinateCollection = db.GetCollection<database.CartesianCoordinate>("GeometricCoordinates").Find(x => x.name.Equals("CartesianCoordinate" + item.id));
            //int index = 1;
            //foreach (var coordinate in cartesianCoordinateCollection)
            //{
            //    cartesianPolyLine.coordinates.Add(referCoordinate(rsmEntities, coordinate.name + "_" + coordinate.index));
            //    index++;
            //}

            foreach (var coor in item.lineCoordinates)
            {
                cartesianPolyLine.coordinates.Add(referCoordinate(rsmEntities, "CartesianCoordinate" + coor.x));
            }
            return cartesianPolyLine;
        }


        /// <summary>
        /// Method to associate linear km with any netelements.
        /// specifically for mileage
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="feature"></param>
        /// <param name="segment"></param>
        private static void addAssociatedNetElementsMileage(HorizontalAlignmentSegment segment, RsmEntities rsmEntities)
        {
            var positioningSystem = rsmEntities.usesTopography.usesPositioningSystem.Find(item => item.name == "Relative Positioning System") as LinearPositioningSystem;
            var associatedNetElement = new AssociatedNetElement()
            {
                isLocatedToSide = LateralSide.centre,
                appliesInDirection = ApplicationDirection.undefined,
                netElement = new tElementWithIDref { @ref = rsmEntities.usesTrackTopology.usesNetElement.Find(item => ((LinearElementWithLength)item).name.Equals("kilometerLine")).id },
            };
            var intrinsicCoordinate = new IntrinsicCoordinate
            {
                coordinates =
                        {
                           new tElementWithIDref{ @ref = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.name == "LinearCoordinate" + segment.initialSegment.value).id }
                        },
                value = (segment.initialSegment.value - positioningSystem.startLinearPositioningSystem.value) / (positioningSystem.endMeasure.value - positioningSystem.startMeasure.value)
            };
            rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
            associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });


            intrinsicCoordinate = new IntrinsicCoordinate
            {
                coordinates =
                        {
                           new tElementWithIDref{ @ref = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.name == "LinearCoordinate" + segment.endSegment.value).id }
                        },
                value = (segment.endSegment.value - positioningSystem.startLinearPositioningSystem.value) / (positioningSystem.endMeasure.value - positioningSystem.startMeasure.value)
            };
            rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(intrinsicCoordinate);
            associatedNetElement.bounds.Add(new tElementWithIDref { @ref = intrinsicCoordinate.id });

            segment.hasLinearLocation.associatedNetElements.Add(associatedNetElement);
        }

        
    }
}