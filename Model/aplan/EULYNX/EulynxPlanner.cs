using aplan.core;
using LiteDB;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.rsmNE;
using Models.TopoModels.EULYNX.rsmTrack;
using Models.TopoModels.EULYNX.sig;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using db = Models.TopoModels.EULYNX.db;
using rsmSig = Models.TopoModels.EULYNX.rsmSig;

using AssociatedNetElement = Models.TopoModels.EULYNX.rsmCommon.AssociatedNetElement;
using LinearCoordinate = Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate;
using NetElement = Models.TopoModels.EULYNX.rsmCommon.NetElement;
using Signal = Models.TopoModels.EULYNX.rsmSig.Signal;
using VehiclePassageDetector = Models.TopoModels.EULYNX.rsmSig.VehiclePassageDetector;
using TpsDevice = Models.TopoModels.EULYNX.rsmSig.TpsDevice;
using VerticalAlignmentSegment = Models.TopoModels.EULYNX.rsmCommon.VerticalAlignmentSegment;

using static aplan.core.Helper;
using System;

namespace aplan.eulynx

{
    class EulynxPlanner
    {

        //instance
        private static EulynxPlanner eulynxPlanner;

        //singleton constructor
        private EulynxPlanner() { }

        //singleton method
        public static EulynxPlanner getInstance()
        {
            if (eulynxPlanner == null)
            {
                eulynxPlanner = new EulynxPlanner();
            }
            return eulynxPlanner;
        }

        /// <summary>
        /// Method to print CCS infrastructure information in console for integration purpose with UI.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void printCCSInConsole(EulynxDataPrepInterface eulynx)
        {
            // print signals
            printSignalsInConsole(eulynx);
            // print axle counter
            printAxleCountersInConsole(eulynx);

        }

        /// <summary>
        /// Method to print signal's information in console for integration purpose with UI.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void printSignalsInConsole(EulynxDataPrepInterface eulynx)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

            Console.WriteLine("Signals");

            foreach (rsmSig.Signal rsmSignal in rsmEntities.ownsSignal)
            {
                string dbFunction = "";

                // get signal's function and print it
                var euSignal = dataPrepEntities.ownsTrackAsset.Find(item => ((db.LightSignalTyped)item).refersToRsmSignal.@ref == rsmSignal.id);
                var function = dataPrepEntities.ownsSignalFunction.Find(item => item.appliesToSignal.@ref == euSignal.id) as db.SignalFunction;
                if (function.isOfSignalFunctionType == db.SignalFunctionTypes.entry)
                {
                    dbFunction = "entry";
                }
                else if (function.isOfSignalFunctionType == db.SignalFunctionTypes.exit)
                {
                    dbFunction = "exit";
                }
                else // further options can be written later
                {
                    dbFunction = "unknown";
                }
                Console.WriteLine(dbFunction);

                // get signal's location and print it
                var signalLocation = rsmEntities.usesLocation.Find(item => item.id == rsmSignal.locations.First().@ref) as SpotLocation; // dereference
                var signalIntrinsicCoordinateRef = signalLocation.associatedNetElements.First().bounds.First();
                var signalIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == signalIntrinsicCoordinateRef.@ref);
                var signalCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(item => item.id == signalIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
                var signalKmValue = signalCoordinate.measure.value.Value;
                Console.WriteLine(signalKmValue);

                // get signal's lateral distance and print it
                var dbSignal = dataPrepEntities.ownsTrackAsset.First(item => item is db.LightSignalTyped && ((db.LightSignalTyped)item).refersToRsmSignal.@ref == rsmSignal.id) as db.LightSignalTyped;
                var dbSignalFrame = dataPrepEntities.ownsSignalFrame.First(item => item.id == dbSignal.hasSignalFrame.First().@ref);
                Console.WriteLine(dbSignalFrame.hasPosition.First().value.value.Value);

                // get signal's side location and print it
                if (signalLocation.associatedNetElements.First().isLocatedToSide == LateralSide.right)
                {
                    Console.WriteLine("right");
                }
                else if (signalLocation.associatedNetElements.First().isLocatedToSide == LateralSide.left)
                {
                    Console.WriteLine("left");
                }

                // get net element id also name which signal is belong to
                if (signalLocation.associatedNetElements.First().appliesInDirection.Value == ApplicationDirection.normal)
                {
                    Console.WriteLine("normal");
                }
                else if (signalLocation.associatedNetElements.First().appliesInDirection.Value == ApplicationDirection.reverse)
                {
                    Console.WriteLine("reverse");
                }
            }
        }

        /// <summary>
        /// Method to print axle counter's information in console for integration purpose with UI.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void printAxleCountersInConsole(EulynxDataPrepInterface eulynx)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

            Console.WriteLine("Axle Counters");

            foreach (var onTrackSignallingDevice in rsmEntities.ownsOnTrackSignallingDevice)
            {
                if(onTrackSignallingDevice is VehiclePassageDetector)
                {
                    Console.WriteLine(onTrackSignallingDevice.name);

                    // get axle counter's location and print it
                    var acLocation = rsmEntities.usesLocation.Find(item => item.id == onTrackSignallingDevice.locations.First().@ref) as SpotLocation; // dereference
                    var acIntrinsicCoordinateRef = acLocation.associatedNetElements.First().bounds.First();
                    var acIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == acIntrinsicCoordinateRef.@ref);
                    var acCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(item => item.id == acIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
                    var acKmValue = acCoordinate.measure.value.Value;
                    Console.WriteLine(acKmValue);

                    // get signal's side location and print it
                    if (acLocation.associatedNetElements.First().isLocatedToSide == LateralSide.right)
                    {
                        Console.WriteLine("right");
                    }
                    else if (acLocation.associatedNetElements.First().isLocatedToSide == LateralSide.left)
                    {
                        Console.WriteLine("left");
                    }
                    else if (acLocation.associatedNetElements.First().isLocatedToSide == LateralSide.centre)
                    {
                        Console.WriteLine("center");
                    }

                    //name, location km, side location
                }
            }

        }

        /// <summary>
        /// Method to print ETCS infrastructure information in console for integration purpose with UI.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void printETCSIInConsole(EulynxDataPrepInterface eulynx)
        {
            // print DP
            printBalisesInConsole(eulynx);

        }

        /// <summary>
        /// Method to print balise's information in console for integration purpose with UI.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void printBalisesInConsole(EulynxDataPrepInterface eulynx)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

            Console.WriteLine("Balises");

            foreach (var onTrackSignallingDevice in rsmEntities.ownsOnTrackSignallingDevice)
            {
                if(onTrackSignallingDevice is TpsDevice)
                {
                    //name
                    Console.WriteLine(onTrackSignallingDevice.name);

                    //dp function
                    var euBalise = dataPrepEntities.ownsTrackAsset.First(x => x is EtcsBalise && ((EtcsBalise)x).refersToRsmTpsDevice.@ref == onTrackSignallingDevice.id) as EtcsBalise;
                    var euBaliseGroupId = euBalise.implementsTpsDataTxSystem.@ref;
                    var euBaliseProperty = dataPrepEntities.ownsTpsDataTransmissionSystemProperties.First(x => x is EtcsBaliseGroupLevel2 && ((EtcsBaliseGroupLevel2)x).appliesToTpsDataTxSystem.@ref == euBaliseGroupId) as EtcsBaliseGroupLevel2;
                    var euBaliseFunction = euBaliseProperty.implementsFunction.First().type;
                    Console.WriteLine(euBaliseFunction);


                    // get axle counter's location and print it
                    var tpsDeviceLocation = rsmEntities.usesLocation.Find(item => item.id == onTrackSignallingDevice.locations.First().@ref) as SpotLocation; // dereference
                    var tpsDeviceIntrinsicCoordinateRef = tpsDeviceLocation.associatedNetElements.First().bounds.First();
                    var tpsDeviceIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == tpsDeviceIntrinsicCoordinateRef.@ref);
                    var tpsDeviceCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(item => item.id == tpsDeviceIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
                    var tpsDeviceKmValue = tpsDeviceCoordinate.measure.value.Value;
                    Console.WriteLine(tpsDeviceKmValue);

                    // get net element id also name which signal is belong to
                    if (tpsDeviceLocation.associatedNetElements.First().appliesInDirection.Value == ApplicationDirection.normal)
                    {
                        Console.WriteLine("normal");
                    }
                    else if (tpsDeviceLocation.associatedNetElements.First().appliesInDirection.Value == ApplicationDirection.reverse)
                    {
                        Console.WriteLine("reverse");
                    }
                }
            }
        }

        /// <summary>
        /// Method to plan CCS infrastructure automatically.
        /// This method calls another methods
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planCCS(EulynxDataPrepInterface eulynx, Database database)
        {
            // plan entry aSignal
            planEntrySignal(eulynx, database);
            // plan exit aSignal
            planExitSignal(eulynx, database);
            // plan axle counter
            //planAxleCounter(eulynx, database);
        }

        /// <summary>
        /// Method to plan ETCS infrastructure automatically.
        /// This method calls another methods
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planETCS(EulynxDataPrepInterface eulynx, Database database)
        {
            planDp20(eulynx, database);
            planDp21(eulynx, database);
        }

        /// <summary>
        /// Method to plan data point 20.
        /// This method has been written in accordance to RIL 819.1344
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planDp20(EulynxDataPrepInterface eulynx, Database database)
        {
            using(var liteDB = database.accessDB())
            {
                // list handled entity?
                List<Signal> handledSignals = new List<Signal>();
                // collections
                var signalCollection = liteDB.GetCollection<database.Signal>("Signals");

                // help variables
                var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
                var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;
                var neighbouringObjects = new List<LocatedNetEntity>();
                double slope, distance, speed = 100; // speed variable is assumed

                // iterate through signals
                foreach(var rsmSignal in rsmEntities.ownsSignal)
                {
                    if (handledSignals.Contains(rsmSignal))
                        continue;

                    // get internal db signal
                    var aSignal = liteDB.GetCollection<database.Signal>("Signals").Find(x => x.uuid == rsmSignal.id).FirstOrDefault();

                    // get euSignal
                    var euSignals = dataPrepEntities.ownsTrackAsset.FindAll(x => x is db.LightSignalTyped);
                    var euSignal = euSignals.Find(x => ((db.LightSignalTyped)x).refersToRsmSignal.@ref == rsmSignal.id) as db.LightSignalTyped;

                    var euSignalFunctions = dataPrepEntities.ownsSignalFunction.FindAll(x => x is db.SignalFunction);
                    var euSignalFunction = euSignalFunctions.Find(x => ((db.SignalFunction)x).appliesToSignal.@ref == euSignal.id) as db.SignalFunction;
                
                    if(euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.entry || euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.block)
                    {
                        // condition disjunction?
                        // list other signals forward and backward
                        bool alreadyPlacedDp = false;

                        // loop found signals forward
                        // loop found signals backward

                        // regular dp placement
                        if (!alreadyPlacedDp)
                        {
                            planDp20reg(eulynx, liteDB, rsmSignal, euSignal, aSignal);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Method to plan data point 21.
        /// This method has been written in accordance to RIL 819.1344
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planDp21(EulynxDataPrepInterface eulynx, Database database)
        {
            using (var liteDB = database.accessDB())
            {
                // list handled entity?
                List<Signal> handledSignals = new List<Signal>();
                // collections
                var signalCollection = liteDB.GetCollection<database.Signal>("Signals");

                // help variables
                var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
                var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;
                var neighbouringObjects = new List<LocatedNetEntity>();
                double slope, distance, speed = 100; // speed variable is assumed

                // iterate through signals
                foreach (var rsmSignal in rsmEntities.ownsSignal)
                {
                    if (handledSignals.Contains(rsmSignal))
                        continue;

                    // get internal db signal
                    var aSignal = liteDB.GetCollection<database.Signal>("Signals").Find(x => x.uuid == rsmSignal.id).FirstOrDefault();

                    // get euSignal
                    var euSignals = dataPrepEntities.ownsTrackAsset.FindAll(x => x is db.LightSignalTyped);
                    var euSignal = euSignals.Find(x => ((db.LightSignalTyped)x).refersToRsmSignal.@ref == rsmSignal.id) as db.LightSignalTyped;

                    var euSignalFunctions = dataPrepEntities.ownsSignalFunction.FindAll(x => x is db.SignalFunction);
                    var euSignalFunction = euSignalFunctions.Find(x => ((db.SignalFunction)x).appliesToSignal.@ref == euSignal.id) as db.SignalFunction;

                    if (euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.exit || 
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.intermediateExitSignal ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.entryAndExit ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.groupMainAndIntermediate ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.groupMain ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.groupIntermediate ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.trainProtection ||
                        euSignalFunction.isOfSignalFunctionType == db.SignalFunctionTypes.intermediate)
                    {
                        // condition disjunction?
                        // list other signals forward and backward
                        bool alreadyPlacedDp = false;

                        // loop found signals forward
                        // loop found signals backward

                        // regular dp placement
                        if (!alreadyPlacedDp)
                        {
                            planDp21reg(eulynx, liteDB, rsmSignal, euSignal, aSignal);
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Method to instantiate APlan internal associated net element.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        private static database.AssociatedNetElement instantiateInternalAssociatedNetElement(RsmEntities rsmEntities, LiteDatabase liteDB, LocatedNetEntity entity, double km, string lateralSide)
        {
            // find associated net element for first tps device
            var location = rsmEntities.usesLocation.Find(loc => loc.id == entity.locations.First().@ref) as SpotLocation; // dereference
            var rsmNetElement = rsmEntities.usesTrackTopology.usesNetElement.Find(rsmNe => rsmNe.id == location.associatedNetElements.First().netElement.@ref) as LinearElementWithLength; // dereference
            var aNetElement = liteDB.GetCollection<database.NetElement>("NetElements").Find(x => x.uuid == rsmNetElement.id).FirstOrDefault();

            // intrinsic coordinate calculation for first tps device
            var initialNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
            var endNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
            var initialNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(coor => coor.id.Equals(initialNeRef))).measure.value.Value;
            var endNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(coor => coor.id.Equals(endNeRef))).measure.value.Value;
            var length = (Length)rsmNetElement.elementLength.quantiy.First(item => item is Length);
            var intrinsicValue = (km - initialNeValue) / (length.value.Value / 1000);

            var associatedNetElement = new database.AssociatedNetElement
            {
                netElementId = aNetElement.id,
                intrinsicValue = intrinsicValue,
                lateralSide = lateralSide,
                appliesInDirection = aNetElement.codeDirection
            };

            return associatedNetElement;
        }

        /// <summary>
        /// Method to plan regular data point 21.
        /// This method has been written in accordance to RIL 819.1344
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planDp20reg(EulynxDataPrepInterface eulynx, LiteDatabase liteDB, Signal rsmSignal, db.LightSignalTyped euSignal, database.Signal aSignal)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

            // generate internal database tps device
            // calculate
            // 819.1344 Pg. 109
            var aTpsDevice1 = new database.TpsDevice
            {
                uuid = generateUUID(),
                name = "balise_20_1",
                deviceType = "balise",
                km = aSignal.km - 6/1000
            };

            var aTpsDevice2 = new database.TpsDevice
            {
                uuid = generateUUID(),
                name = "balise_20_2",
                deviceType = "balise",
                km = aSignal.km - 2.4/1000
            };

            string lateralSide = "center";

            var firstAssociatedNetElement = instantiateInternalAssociatedNetElement(rsmEntities, liteDB, rsmSignal, aTpsDevice1.km, lateralSide);
            var secondAssociatedNetElement = instantiateInternalAssociatedNetElement(rsmEntities, liteDB, rsmSignal, aTpsDevice2.km, lateralSide);

            aTpsDevice1.associatedNetElements = new List<database.AssociatedNetElement>();
            aTpsDevice1.associatedNetElements.Add(firstAssociatedNetElement);
            aTpsDevice2.associatedNetElements = new List<database.AssociatedNetElement>();
            aTpsDevice2.associatedNetElements.Add(secondAssociatedNetElement);

            // build tps device 
            TpsDevice rsmTpsDevice1, rsmTpsDevice2;
            var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();

            eulynxRsmAssetBuilder.buildTpsDevice(rsmEntities, aTpsDevice1, out rsmTpsDevice1);
            eulynxRsmAssetBuilder.buildTpsDevice(rsmEntities, aTpsDevice2, out rsmTpsDevice2);

            var rsmTpsDevices = new List<TpsDevice>();
            rsmTpsDevices.Add(rsmTpsDevice1);
            rsmTpsDevices.Add(rsmTpsDevice2);   

            var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
            dbEulynxDataPrepBuilder.buildEtcsBaliseGroupLevel2(dataPrepEntities, rsmTpsDevices, "20");
            
            
        }

        /// <summary>
        /// Method to plan regular data point 21.
        /// This method has been written in accordance to RIL 819.1344
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        public void planDp21reg(EulynxDataPrepInterface eulynx, LiteDatabase liteDB, Signal rsmSignal, db.LightSignalTyped euSignal, database.Signal aSignal)
        {
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

            // generate internal database tps device
            // calculate
            // 819.1344 Pg. 109
            var aTpsDevice1 = new database.TpsDevice
            {
                uuid = generateUUID(),
                name = "balise_21_1",
                deviceType = "balise",
                km = aSignal.km - 6/1000
            };

            var aTpsDevice2 = new database.TpsDevice
            {
                uuid = generateUUID(),
                name = "balise_21_2",
                deviceType = "balise",
                km = aSignal.km - 2.4/1000
            };

            string lateralSide = "center";

            var firstAssociatedNetElement = instantiateInternalAssociatedNetElement(rsmEntities, liteDB, rsmSignal, aTpsDevice1.km, lateralSide);
            var secondAssociatedNetElement = instantiateInternalAssociatedNetElement(rsmEntities, liteDB, rsmSignal, aTpsDevice2.km, lateralSide);

            aTpsDevice1.associatedNetElements = new List<database.AssociatedNetElement>();
            aTpsDevice1.associatedNetElements.Add(firstAssociatedNetElement);
            aTpsDevice2.associatedNetElements = new List<database.AssociatedNetElement>();
            aTpsDevice2.associatedNetElements.Add(secondAssociatedNetElement);

            // build tps device 
            TpsDevice rsmTpsDevice1, rsmTpsDevice2;
            var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();

            eulynxRsmAssetBuilder.buildTpsDevice(rsmEntities, aTpsDevice1, out rsmTpsDevice1);
            eulynxRsmAssetBuilder.buildTpsDevice(rsmEntities, aTpsDevice2, out rsmTpsDevice2);

            var rsmTpsDevices = new List<TpsDevice>();
            rsmTpsDevices.Add(rsmTpsDevice1);
            rsmTpsDevices.Add(rsmTpsDevice2);

            var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
            dbEulynxDataPrepBuilder.buildEtcsBaliseGroupLevel2(dataPrepEntities, rsmTpsDevices, "21");


        }


        /// <summary>
        /// Method to plan entry signal in a given data set.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        /// <param name="database">internal database</param>
        public void planEntrySignal(EulynxDataPrepInterface eulynx, Database database)
        {
            using (var db = database.accessDB())
            {
                // collections
                var signalCollection = db.GetCollection<database.Signal>("Signals");

                // help variables
                var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
                var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;
                var neighbouringObjects = new List<LocatedNetEntity>();
                double slope, distance, speed = 100; // speed variable is assumed

                // collect net element which will be used as a reference point
                foreach (var netElement in pickFirstNetElements(db))
                {
                    // build database aSignal
                    var aSignal = new database.Signal()
                    {
                        uuid = generateUUID(),
                        signalType = "main",
                        signalFunction = "entry",
                        fixingType = "post",
                        associatedNetElements = new List<database.AssociatedNetElement>()
                    };

                    #region analysis and defining required value for signal

                    var rsmNetElement = (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id == netElement.uuid);

                    // from respective point, walk towards net element direction
                    var walkDirection = netElement.codeDirection == 1 ? ApplicationDirection.normal : ApplicationDirection.reverse;
                    var intrinsicCoordinate = netElement.codeDirection == 1 ? rsmNetElement.associatedPositioning.First().intrinsicCoordinates.First() : rsmNetElement.associatedPositioning.First().intrinsicCoordinates.Last();

                    //var example = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(x => x.id == intrinsicCoordinate.coordinates.First().@ref)).measure.value.Value;
                    //Debug.WriteLine(example);

                    // get located net entities associated with current net element
                    neighbouringObjects = getNeighbouringObjects<LocatedNetEntity>(intrinsicCoordinate, walkDirection, rsmEntities, rsmNetElement);

                    // check detected entity (to be deleted)
                    //Debug.WriteLine(neighbouringObjects);
                    //foreach (var locatedNetEntity in neighbouringObjects)
                    //{
                    //    Debug.WriteLine(locatedNetEntity.name);
                    //}

                    // find first danger point
                    var firstDangerPoint = neighbouringObjects.First();
                    var dangerPointLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == firstDangerPoint.locations.First().@ref) as SpotLocation; // dereference
                    var dangerPointIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == dangerPointLocation.associatedNetElements.First().bounds.First().@ref); // dereference
                    var dangerPointCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == dangerPointIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate; // dereference

                    // set first distance
                    var firstPoint = dangerPointCoordinate.measure.value.Value - 0.2; // measure in km

                    // calculate slope
                    var segment = isPinnedOn(rsmEntities, firstPoint, walkDirection);
                    slope = slopeCalculator(rsmEntities, segment, firstPoint);

                    // determine real safety distance based on rule
                    distance = Rules.safetyDistance(slope, speed) / 1000;
                    Debug.WriteLine(slope);
                    Debug.WriteLine(distance);

                    // is it electrified?
                    //      yes -> is it in restricted area? (call function)
                    //      no  -> continue
                    // is it in area of level crossing? (call function)

                    // final value of aSignal
                    var signalKmValue = netElement.codeDirection == 1 ? dangerPointCoordinate.measure.value.Value - distance : dangerPointCoordinate.measure.value.Value + distance;

                    //intrinsic coordinate calculation for aSignal
                    var initialNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                    var endNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                    var initialNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialNeRef))).measure.value.Value;
                    var endNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endNeRef))).measure.value.Value;
                    var length = (Length)rsmNetElement.elementLength.quantiy.First(item => item is Length);
                    var intrinsicValue = (signalKmValue - initialNeValue) / (length.value.Value / 1000);

                    // lateral side calculation
                    var lateralSide = Rules.sideAllocation(walkDirection, netElement.trackType);

                    #endregion

                    // assign calculated value to each attributes
                    aSignal.km = signalKmValue;

                    var associatedNetElement = new database.AssociatedNetElement
                    {
                        netElementId = netElement.uuid,
                        intrinsicValue = intrinsicValue,
                        lateralSide = lateralSide,
                        appliesInDirection = netElement.codeDirection
                    };

                    aSignal.associatedNetElements.Add(associatedNetElement);

                    // build rsm aSignal
                    Signal rsmSignal;
                    var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                    eulynxRsmAssetBuilder.buildSignal(rsmEntities, aSignal, out rsmSignal);

                    var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                    dbEulynxDataPrepBuilder.buildSignal(dataPrepEntities, rsmSignal, aSignal);

                    // insert internal model to internal database
                    signalCollection.Insert(aSignal);

                }
            }
        }


        /// <summary>
        /// Method to plan exit aSignal in a given data set.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        /// <param name="database">internal database</param>
        public void planExitSignal(EulynxDataPrepInterface eulynx, Database database)
        {
            using (var db = database.accessDB())
            {
                // collections
                var signalCollection = db.GetCollection<database.Signal>("Signals");
                var netElementCollection = db.GetCollection<database.NetElement>("NetElements");

                // help variables
                var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
                var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;

                // query all existed entry signals
                var entrySignals = new List<db.SignalFunction>();
                var querySignals = from sig in dataPrepEntities.ownsSignalFunction
                                   let entrySig = sig as db.SignalFunction
                                   where entrySig.isOfSignalFunctionType == Models.TopoModels.EULYNX.db.SignalFunctionTypes.entry
                                   select entrySig;

                // collect all existed entry aSignal and put into a list
                foreach (db.SignalFunction entrySignal in querySignals)
                {
                    entrySignals.Add(entrySignal);
                }

                // start from each entry aSignal
                foreach (db.SignalFunction entrySignal in entrySignals)
                {
                    // build database aSignal
                    var aSignal = new database.Signal()
                    {
                        uuid = generateUUID(),
                        signalType = "main",
                        signalFunction = "exit",
                        fixingType = "post",
                        associatedNetElements = new List<database.AssociatedNetElement>()
                    };

                    // get rsm aSignal
                    var dbSignal = dataPrepEntities.ownsTrackAsset.Find(item => item.id == entrySignal.appliesToSignal.@ref) as db.LightSignalTyped; // dereference
                    var rsmSignal = rsmEntities.ownsSignal.Find(item => item.id == dbSignal.refersToRsmSignal.@ref); // dereference

                    // find attached net element
                    var signalLocation = rsmEntities.usesLocation.Find(item => item.id == rsmSignal.locations.First().@ref) as SpotLocation; // dereference
                    var rsmNetElement = rsmEntities.usesTrackTopology.usesNetElement.Find(item => item.id == signalLocation.associatedNetElements.First().netElement.@ref) as LinearElementWithLength; // dereference
                    var internalNetElement = netElementCollection.Find(x => x.uuid == rsmNetElement.id).First();

                    // get entry aSignal's intrinsic coordinate and km value
                    var signalIntrinsicCoordinateRef = signalLocation.associatedNetElements.First().bounds.First();
                    var signalIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == signalIntrinsicCoordinateRef.@ref);
                    var signalCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(item => item.id == signalIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
                    var signalKmValue = signalCoordinate.measure.value.Value;

                    // get walk direction
                    var walkDirection = signalLocation.associatedNetElements.First().appliesInDirection.Value;

                    // walk from entry aSignal spot km through the end of net element by given direction
                    var neighbouringObjects = new List<LocatedNetEntity>();
                    neighbouringObjects = getNeighbouringObjects<LocatedNetEntity>(signalIntrinsicCoordinate, walkDirection, rsmEntities, rsmNetElement);

                    // soon to be deleted, testing purpose
                    //foreach (LocatedNetEntity locatedNetEntity in neighbouringObjects)
                    //{
                    //    Debug.WriteLine(locatedNetEntity.name);
                    //    Debug.WriteLine(rsmNetElement.name);
                    //}

                    if (neighbouringObjects.Exists(item => item.name == "platform"))
                    {
                        var platform = neighbouringObjects.Find(item => item.name == "platform");
                        var platformLocation = rsmEntities.usesLocation.Find(item => item.id == platform.locations.First().@ref) as SpotLocation;

                        // find first danger point start from platform
                        signalIntrinsicCoordinateRef = platformLocation.associatedNetElements.First().bounds.First();
                        signalIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == signalIntrinsicCoordinateRef.@ref);
                        neighbouringObjects = rsmNetElement.getNeighbouringObjects<LocatedNetEntity>(signalIntrinsicCoordinate, walkDirection, rsmEntities);
                        var firstDangerPoint = neighbouringObjects.First();
                        var dangerPointLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == firstDangerPoint.locations.First().@ref) as SpotLocation; // dereference
                        var dangerPointCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == dangerPointLocation.coordinates.First().@ref) as LinearCoordinate; // dereference

                        // exit aSignal calculation
                    }
                    else if (neighbouringObjects.Exists(item => item is Turnout))
                    {
                        // get turnout and its km
                        var turnout = neighbouringObjects.Find(item => item is Turnout); // dereference
                        var turnoutLocation = rsmEntities.usesLocation.Find(item => item.id == turnout.locations.First().@ref) as SpotLocation; // dereference

                        // walk back to trace is there any aSignal that cover this turnout
                        var turnoutIntrinsicCoordinate = walkDirection == ApplicationDirection.normal ? rsmNetElement.associatedPositioning.First().intrinsicCoordinates.Last() : rsmNetElement.associatedPositioning.First().intrinsicCoordinates.First();
                        walkDirection = walkDirection == ApplicationDirection.normal ? ApplicationDirection.reverse : ApplicationDirection.normal; // flip the direction
                        neighbouringObjects = getNeighbouringObjects<LocatedNetEntity>(turnoutIntrinsicCoordinate, walkDirection, rsmEntities, rsmNetElement);

                        if (neighbouringObjects.Exists(item => item is Signal))
                        {
                            // continue to next net element
                            walkDirection = walkDirection == ApplicationDirection.reverse ? ApplicationDirection.normal : ApplicationDirection.reverse; // flip the direction
                            nextNetElement(eulynx, db, rsmNetElement, aSignal, rsmSignal);

                            signalCollection.Insert(aSignal);
                        }
                        else
                        {
                            // put object
                        }
                    }
                    else
                    {
                        // next net element
                        nextNetElement(eulynx, db, rsmNetElement, aSignal, rsmSignal);
                    }

                }
            }

        }

        /// <summary>
        /// Method to plan axle counter in a given data set.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        /// <param name="database">internal database</param>
        public void planAxleCounter(EulynxDataPrepInterface eulynx, Database database)
        {
            using (var db = database.accessDB())
            {
                // collections
                var signalCollection = db.GetCollection<database.Signal>("Signals");

                // help variables
                var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
                var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;
                var neighbouringObjects = new List<LocatedNetEntity>();

                // iterate trough existing signals and points
                foreach (var signal in rsmEntities.ownsSignal)
                {
                    // build database axle counter
                    var axleCounter = new database.AxleCounter()
                    {
                        uuid = generateUUID(),
                        name = "test",
                        longname = "tests",
                        associatedNetElements = new List<database.AssociatedNetElement>()
                    };

                    // get net entity coordinate
                    var entityLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == signal.locations.First().@ref) as SpotLocation; // dereference
                    var entityIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == entityLocation.associatedNetElements.First().bounds.First().@ref); // dereference
                    var entityCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == entityIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate; // dereference

                    // get rsm and database net element
                    var rsmNetElement = (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id == entityLocation.associatedNetElements.First().netElement.@ref);
                    var netElement = db.GetCollection<database.NetElement>("NetElements").Find(x => x.uuid == rsmNetElement.id).First();
                    
                    // calculate axle counter linear coordinate value
                    var condition = convertLocatedEntityTypeToString(signal, db); //extra function
                    var distance = Rules.axleCounter(condition);
                    var axleCounterKmValue = netElement.codeDirection == 1 ? entityCoordinate.measure.value.Value - distance : entityCoordinate.measure.value.Value + distance;

                    // intrinsic coordinate calculation for axle counter
                    var initialNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                    var endNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                    var initialNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialNeRef))).measure.value.Value;
                    var endNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endNeRef))).measure.value.Value;
                    var length = (Length)rsmNetElement.elementLength.quantiy.First(item => item is Length);
                    var intrinsicValue = (axleCounterKmValue - initialNeValue) / (length.value.Value / 1000);

                    // assign calculated value to each attributes
                    axleCounter.km = axleCounterKmValue;

                    var associatedNetElement = new database.AssociatedNetElement
                    {
                        intrinsicValue = intrinsicValue,
                        appliesInDirection = netElement.codeDirection
                    };

                    axleCounter.associatedNetElements.Add(associatedNetElement);

                    // build rsm axle counter
                    VehiclePassageDetector rsmAxleCounter;
                    var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                    eulynxRsmAssetBuilder.buildAxleCounter(rsmEntities, axleCounter, out rsmAxleCounter);

                    var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                    dbEulynxDataPrepBuilder.buildAxleCounter(dataPrepEntities, rsmAxleCounter);

                    Debug.WriteLine(signal.name);
                }

                foreach (var point in rsmEntities.ownsPoint)
                {
                    // build database axle counter
                    var axleCounter = new database.AxleCounter()
                    {
                        uuid = generateUUID(),
                        name = "axle counter",
                        longname = "axle counter",
                        associatedNetElements = new List<database.AssociatedNetElement>()
                    };

                    // get net entity coordinate
                    var entityLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == point.locations.First().@ref) as SpotLocation; // dereference
                    var entityIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == entityLocation.associatedNetElements.First().bounds.First().@ref); // dereference
                    var entityCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == entityIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate; // dereference

                    if (entityCoordinate.measure.value == null)
                        continue;

                    // get rsm and database net element
                    var rsmNetElement = (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(ne => ne.id == entityLocation.associatedNetElements.First().netElement.@ref);
                    var netElement = db.GetCollection<database.NetElement>("NetElements").Find(x => x.uuid == rsmNetElement.id).FirstOrDefault();
                    

                    // calculate axle counter linear coordinate value
                    var condition = convertLocatedEntityTypeToString(point, db); //extra function
                    var distance = Rules.axleCounter(condition);
                    var axleCounterKmValue = netElement.codeDirection == 1 ? entityCoordinate.measure.value.Value - distance : entityCoordinate.measure.value.Value + distance;

                    // intrinsic coordinate calculation for axle counter
                    var initialNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                    var endNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                    var initialNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialNeRef))).measure.value.Value;
                    var endNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endNeRef))).measure.value.Value;
                    var length = (Length)rsmNetElement.elementLength.quantiy.First(item => item is Length);
                    var intrinsicValue = (axleCounterKmValue - initialNeValue) / (length.value.Value / 1000);

                    // assign calculated value to each attributes
                    axleCounter.km = axleCounterKmValue;

                    var associatedNetElement = new database.AssociatedNetElement
                    {
                        intrinsicValue = intrinsicValue,
                        appliesInDirection = netElement.codeDirection
                    };

                    axleCounter.associatedNetElements.Add(associatedNetElement);

                    // build rsm axle counter
                    VehiclePassageDetector rsmAxleCounter;
                    var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                    eulynxRsmAssetBuilder.buildAxleCounter(rsmEntities, axleCounter, out rsmAxleCounter);

                    var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                    dbEulynxDataPrepBuilder.buildAxleCounter(dataPrepEntities, rsmAxleCounter);

                    Debug.WriteLine(point.name);
                }
                //// collect net element which will be used as a reference point
                //foreach (var netElement in pickFirstNetElements(liteDB))
                //{
                //    var rsmNetElement = (LinearElementWithLength)rsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id == netElement.uuid);

                //    // from respective point, walk towards net element direction
                //    var walkDirection = netElement.codeDirection == 1 ? ApplicationDirection.normal : ApplicationDirection.reverse;
                //    var intrinsicCoordinate = netElement.codeDirection == 1 ? rsmNetElement.associatedPositioning.First().intrinsicCoordinates.First() : rsmNetElement.associatedPositioning.First().intrinsicCoordinates.Last();
                //    neighbouringObjects = rsmNetElement.getNeighbouringObjects<LocatedNetEntity>(intrinsicCoordinate, walkDirection, rsmEntities);

                //    // loop through found entities                 
                //    foreach (var locatedNetEntity in neighbouringObjects)
                //    {
                //        // build database axle counter
                //        var axleCounter = new database.AxleCounter()
                //        {
                //            uuid = generateUUID(),
                //            name = "test",
                //            longname = "tests",
                //            associatedNetElements = new List<database.AssociatedNetElement>()
                //        };

                //        // get net entity coordinate
                //        var entityLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == locatedNetEntity.locations.First().@ref) as SpotLocation; // dereference
                //        var entityIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == entityLocation.associatedNetElements.First().bounds.First().@ref); // dereference
                //        var entityCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == entityIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate; // dereference

                //        // calculate axle counter linear coordinate value
                //        var condition = convertLocatedEntityTypeToString(locatedNetEntity, liteDB); //extra function
                //        var distance = Rules.axleCounter(condition);
                //        var axleCounterKmValue = netElement.codeDirection == 1 ? entityCoordinate.measure.value.Value - distance : entityCoordinate.measure.value.Value + distance;

                //        // intrinsic coordinate calculation for axle counter
                //        var initialNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                //        var endNeRef = rsmNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                //        var initialNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialNeRef))).measure.value.Value;
                //        var endNeValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endNeRef))).measure.value.Value;
                //        var length = (Length)rsmNetElement.elementLength.quantiy.First(item => item is Length);
                //        var intrinsicValue = (axleCounterKmValue - initialNeValue) / (length.value.Value / 1000);

                //        // assign calculated value to each attributes
                //        axleCounter.km = axleCounterKmValue;

                //        var associatedNetElement = new database.AssociatedNetElement
                //        {
                //            intrinsicValue = intrinsicValue,
                //            appliesInDirection = netElement.codeDirection
                //        };

                //        axleCounter.associatedNetElements.Add(associatedNetElement);

                //        // build rsm axle counter
                //        VehiclePassageDetector rsmAxleCounter;
                //        var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                //        eulynxRsmAssetBuilder.buildAxleCounter(rsmEntities, axleCounter, out rsmAxleCounter);

                //        var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                //        dbEulynxDataPrepBuilder.buildAxleCounter(dataPrepEntities, rsmAxleCounter, axleCounter);

                //        Debug.WriteLine(locatedNetEntity.name);
                //    }
            }
        }

        /// <summary>
        /// Internal class method to get a list of first net element used for calculation.
        /// </summary>
        /// <param name="db">lite database</param>
        private static List<database.NetElement> pickFirstNetElements(LiteDatabase db)
        {
            // using database to find net elements
            // help variables
            var firstNetElements = new List<database.NetElement>();
            string nodeId;
            bool nodeExist;
            database.Node matchNode;

            // get net element collection only for main track
            var collectionNE = db.GetCollection<database.NetElement>("NetElements").Find(x => x.codeDirection == 1 || x.codeDirection == 2);
            var collectionNO = db.GetCollection<database.Node>("Nodes").FindAll();

            foreach (var netElement in collectionNE)
            {
                nodeExist = true;
                // get node id from possible first element
                if (netElement.codeDirection == 1)
                    nodeId = netElement.startNodeId;
                else
                    nodeId = netElement.endNodeId;

                // loop through nodes to find out
                // if id exists or not
                try
                {
                    matchNode = collectionNO.First(x => x.id == nodeId);
                }
                catch
                {
                    matchNode = null;
                }

                if (matchNode == null)
                    nodeExist = false;

                // if not exists, then net element will be picked
                if (!nodeExist)
                    firstNetElements.Add(netElement);
            }
            return firstNetElements;
        }

        /// <summary>
        /// Internal class method to get initial vertical alignment for calculation.
        /// </summary>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="point">beginning point of calculation/param>
        /// <param name="direction">application direction of the net element</param>
        private static VerticalAlignmentSegment isPinnedOn(RsmEntities rsmEntities, double point, ApplicationDirection direction)
        {
            double initial = 0, end = 0, distance = 0;
            var query = from segment in rsmEntities.usesTopography.usesVerticalAlignmentSegment
                        where segment?.hasLinearLocation?.associatedNetElements?.First().appliesInDirection == direction
                        select segment;

            foreach (var segment in rsmEntities.usesTopography.usesVerticalAlignmentSegment)
            {
                try
                {
                    if (segment.hasLinearLocation?.associatedNetElements?.FirstOrDefault().appliesInDirection == direction)
                    {
                        // get coordinates reference
                        var inititalRef = segment.hasLinearLocation.polyLines.FirstOrDefault().coordinates.First().@ref;
                        var endRef = segment.hasLinearLocation.polyLines.FirstOrDefault().coordinates.Last().@ref;


                        foreach (PositioningSystemCoordinate coordinate in rsmEntities.usesTopography.usesPositioningSystemCoordinate)
                        {
                            if (coordinate is LinearCoordinate)
                            {
                                var linearCoordinate = (LinearCoordinate)coordinate;
                                // match the coordinates
                                if (linearCoordinate.id == inititalRef)
                                {
                                    initial = linearCoordinate.measure.value.Value;
                                }
                                else if (linearCoordinate.id == endRef)
                                {
                                    end = linearCoordinate.measure.value.Value;
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                continue;
                            }

                        }
                        // investigate if point is in between initial or end
                        if (point >= initial && point <= end)
                        {
                            return segment;
                        }
                    }
                }
                catch
                {
                    continue;
                }
               
            }
            return null;

            //foreach (VerticalAlignmentSegment segment in query)
            //{
            //    // get coordinates reference
            //    var inititalRef = segment.hasLinearLocation.polyLines.FirstOrDefault().coordinates.First().@ref;
            //    var endRef = segment.hasLinearLocation.polyLines.FirstOrDefault().coordinates.Last().@ref;


            //    foreach (PositioningSystemCoordinate coordinate in rsmEntities.usesTopography.usesPositioningSystemCoordinate)
            //    {
            //        if (coordinate is LinearCoordinate)
            //        {
            //            var linearCoordinate = (LinearCoordinate)coordinate;
            //            // match the coordinates
            //            if (linearCoordinate.id == inititalRef)
            //            {
            //                initial = linearCoordinate.measure.value.Value;
            //            }
            //            else if (linearCoordinate.id == endRef)
            //            {
            //                end = linearCoordinate.measure.value.Value;
            //            }
            //            else
            //            {
            //                continue;
            //            }
            //        }
            //        else
            //        {
            //            continue;
            //        }

            //    }
            //    // investigate if point is in between initial or end
            //    if (point >= initial && point <= end)
            //    {
            //        return segment;
            //    }
            //}
            //return null;
        }

        /// <summary>
        /// Internal class method to calculate total slope in 1km as well as 2km away from danger point.
        /// And finally determine the greatest one to be used for next calculation
        /// </summary>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="pinnedSegment">segment which the calculation begin with/param>
        /// <param name="point">beginning point of calculation</param>
        private static double slopeCalculator(RsmEntities rsmEntities, VerticalAlignmentSegment pinnedSegment, double point)
        {
            double slope = 0, point2km = 0, point1km = 0, refPoint = 0, slope2km = 0, slope1km = 0;
            int counter = 0;
            bool breaker = true;
            LinearLocation location = null;
            VerticalAlignmentSegment nextSegment;
            ApplicationDirection direction;
            switch (pinnedSegment.type)
            {
                case "Gerade":
                    direction = pinnedSegment.hasLinearLocation.associatedNetElements.First().appliesInDirection.Value;
                    // associated net elemetn not yet implemented
                    point2km = direction == ApplicationDirection.normal ? point - 2 : point + 2;
                    point1km = direction == ApplicationDirection.normal ? point - 1 : point + 1;

                    // direction normal, km increasing
                    if (direction == ApplicationDirection.normal)
                    {
                        // 2km away from first distance
                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                        nextSegment = pinnedSegment;
                        while (refPoint > point2km)
                        {
                            var elevationRef = nextSegment.initialElevation.@ref;
                            slope += rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == elevationRef).inclination.value.Value;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref));
                            //var km = (LinearLocation)rsmEntities.usesLocation.Find(l => ((LinearLocation)l).polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id);

                            var queryLocation = from loc in rsmEntities.usesLocation
                                                where loc is LinearLocation
                                                let linLoc = loc as LinearLocation
                                                where linLoc.polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id
                                                select linLoc;

                            foreach (LinearLocation linLoc in queryLocation)
                            {
                                location = linLoc;
                                breaker = false;
                            }

                            if (breaker) { break; }

                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            //pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                            breaker = true;
                        }
                        slope2km = slope / counter;

                        // 1km away from first distance
                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                        slope = 0;
                        nextSegment = pinnedSegment;
                        while (refPoint > point1km)
                        {
                            var elevationRef = nextSegment.initialElevation.@ref;
                            slope += rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == elevationRef).inclination.value.Value;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref));
                            //km = (LinearLocation)rsmEntities.usesLocation.Find(l => ((LinearLocation)l).polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id);

                            var queryLocation = from loc in rsmEntities.usesLocation
                                                where loc is LinearLocation
                                                let linLoc = loc as LinearLocation
                                                where linLoc.polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id
                                                select linLoc;



                            foreach (LinearLocation linLoc in queryLocation)
                            {
                                location = linLoc;
                                breaker = false;
                            }

                            if (breaker) { break; }
                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            //pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                            breaker = true;
                        }
                        slope1km = slope / counter;

                        slope = slope2km > slope1km ? slope2km : slope1km;
                        return slope;
                    }
                    else
                    {
                        // 2km away from first distance
                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref)).measure.value.Value;
                        nextSegment = pinnedSegment;
                        while (refPoint < point2km)
                        {
                            var elevationRef = nextSegment.initialElevation.@ref;
                            slope += rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == elevationRef).inclination.value.Value;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref));
                            //km = rsmEntities.usesLocation.Find(item => ((LinearLocation)item).polyLines.FirstOrDefault().coordinates.First().@ref == coordinate.id);

                            var queryLocation = from loc in rsmEntities.usesLocation
                                                where loc is LinearLocation
                                                let linLoc = loc as LinearLocation
                                                where linLoc.polyLines.FirstOrDefault().coordinates.First().@ref == coordinate.id
                                                select linLoc;

                            foreach (LinearLocation linLoc in queryLocation)
                            {
                                location = linLoc;
                                breaker = false;
                            }
                            if (breaker) { break; }


                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            //pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref)).measure.value.Value;
                            breaker = true;
                        }
                        slope2km = slope / counter;

                        // 1km away from first distance
                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref)).measure.value.Value;
                        slope = 0;
                        nextSegment = pinnedSegment;
                        while (refPoint < point1km)
                        {
                            var elevationRef = nextSegment.initialElevation.@ref;
                            slope += rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == elevationRef).inclination.value.Value;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref));
                            //km = (LinearLocation)rsmEntities.usesLocation.Find(l => ((LinearLocation)l).polyLines.FirstOrDefault().coordinates.First().@ref == coordinate.id);

                            var queryLocation = from loc in rsmEntities.usesLocation
                                                where loc is LinearLocation
                                                let linLoc = loc as LinearLocation
                                                where linLoc.polyLines.FirstOrDefault().coordinates.First().@ref == coordinate.id
                                                select linLoc;

                            foreach (LinearLocation linLoc in queryLocation)
                            {
                                location = linLoc;
                                breaker = false;
                            }
                            if (breaker) { break; }

                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            //pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.Last().@ref)).measure.value.Value;
                            breaker = true;
                        }
                        slope1km = slope / counter;

                        slope = slope2km > slope1km ? slope2km : slope1km;
                        return slope;
                    }
                    break;
                case "quadratische Parabel":
                    direction = pinnedSegment.hasLinearLocation.associatedNetElements.First().appliesInDirection.Value;
                    // associated net elemetn not yet implemented
                    point2km = direction == ApplicationDirection.normal ? point - 2 : point + 2;
                    point1km = direction == ApplicationDirection.normal ? point - 1 : point + 1;
                    if (direction == ApplicationDirection.normal)
                    {
                        var endElevationRef = pinnedSegment.finalElevation.@ref;
                        var initialElevationRef = pinnedSegment.initialElevation.@ref;
                        var endSlope = rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == endElevationRef).inclination.value.Value;
                        var initialSlope = rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == initialElevationRef).inclination.value.Value;
                        var a = (endSlope - initialSlope) / pinnedSegment.length.value.Value;
                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                        // s0 = s1 - a * x1;
                        initialElevationRef = pinnedSegment.initialElevation.@ref;
                        initialSlope = rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == initialElevationRef).inclination.value.Value;
                        var s0 = initialSlope - a * refPoint;
                        // vSlope = a * x1 + s0;
                        slope = a * point + s0;
                        counter++;
                        while (refPoint > point2km)
                        {
                            initialElevationRef = pinnedSegment.initialElevation.@ref;
                            initialSlope = rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == initialElevationRef).inclination.value.Value;
                            slope += initialSlope;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref));
                            //km = (LinearLocation)rsmEntities.usesLocation.Find(l => ((LinearLocation)l).polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id);

                            var queryLocation = from loc in rsmEntities.usesLocation
                                                where loc is LinearLocation
                                                let linLoc = loc as LinearLocation
                                                where linLoc.polyLines.FirstOrDefault().coordinates.First().@ref == coordinate.id
                                                select linLoc;
                            foreach (LinearLocation linLoc in queryLocation)
                            {
                                location = linLoc;
                            }

                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;

                        }
                        slope2km = slope / counter;

                        // 1km away from first distance

                        refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;
                        slope = a * point + s0;
                        counter++;
                        while (refPoint > point1km)
                        {
                            initialElevationRef = pinnedSegment.initialElevation.@ref;
                            initialSlope = rsmEntities.usesTopography.usesElevationAndInclination.Find(x => x.id == initialElevationRef).inclination.value.Value;
                            counter++;
                            var coordinate = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == pinnedSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref));
                            location = (LinearLocation)rsmEntities.usesLocation.Find(l => ((LinearLocation)l).polyLines.FirstOrDefault().coordinates.Last().@ref == coordinate.id);
                            nextSegment = rsmEntities.usesTopography.usesVerticalAlignmentSegment.Find(s => s.hasLinearLocation.id == location.id);
                            pinnedSegment = nextSegment;
                            refPoint = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(c => c.id == nextSegment.hasLinearLocation.polyLines.First().coordinates.First().@ref)).measure.value.Value;

                        }
                        slope1km = slope / counter;

                        slope = slope2km > slope1km ? slope2km : slope1km;
                        return slope;
                    }

                    break;
            }
            return 0;
        }

        /// <summary>
        /// Internal class method to plan exit aSignal in a given data set.
        /// </summary>
        /// <param name="eulynx">eulynx dp container</param>
        /// <param name="db">accessible internal database</param>
        /// <param name="rsmNetElement">current net element</param>
        /// <param name="internalSignal">internal exit aSignal</param>
        /// <param name="rsmEntrySignal">current entry aSignal</param>
        private static void nextNetElement(EulynxDataPrepInterface eulynx, LiteDatabase db, LinearElementWithLength rsmNetElement, database.Signal internalSignal, Signal rsmEntrySignal)
        {
            // help variables
            var rsmEntities = eulynx.hasDataContainer.First().ownsRsmEntities;
            var dataPrepEntities = eulynx.hasDataContainer.First().ownsDataPrepEntities;
            double slope = 0;
            var speed = 100; // from data?
            double distance;
            var visited = new HashSet<NetElement>();
            var queue = new Queue<LinearElementWithLength>();

            // entry aSignal km value
            var signalLocation = rsmEntities.usesLocation.Find(loc => loc.id == rsmEntrySignal.locations.First().@ref) as SpotLocation; // dereference
            var signalIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == signalLocation.associatedNetElements.First().bounds.First().@ref); // dereference
            var signalCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.First(coor => coor.id == signalIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
            var entrySignalKmValue = signalCoordinate.measure.value.Value;

            // walk direction
            var walkDirection = signalLocation.associatedNetElements.First().appliesInDirection.Value;

            // query current net element relation
            var query = from pr in rsmEntities.usesTrackTopology.usesPositionedRelation
                        where (walkDirection == ApplicationDirection.normal ? pr.elementA.@ref == rsmNetElement.id : pr.elementB.@ref == rsmNetElement.id)
                        select (pr.elementA.@ref == rsmNetElement.id ? pr.elementB.@ref : pr.elementA.@ref);

            // add possible next net element in queue
            foreach (string netElementRef in query)
            {
                queue.Enqueue(rsmEntities.usesTrackTopology.usesNetElement.Find(item => item.id == netElementRef) as LinearElementWithLength);
            }

            // loop until queue is empty
            while (queue.Count() != 0)
            {
                // net element which connected with the given current net element
                var rsmNextNetElement = queue.Dequeue();

                if (rsmNextNetElement != null && !visited.Contains(rsmNextNetElement))
                {

                    // add current net element to visited list
                    visited.Add(rsmNextNetElement);

                    // find current net element in database
                    var internalNextNetElement = db.GetCollection<database.NetElement>("NetElements").FindOne(ne => ne.uuid == rsmNextNetElement.id);

                    // walk from beginning of net element through the end of net element by given direction
                    var intrinsicCoordinate = walkDirection == ApplicationDirection.normal ? rsmNextNetElement.associatedPositioning.First().intrinsicCoordinates.First() : rsmNextNetElement.associatedPositioning.First().intrinsicCoordinates.Last();
                    var neighbouringObjects = new List<LocatedNetEntity>();
                    neighbouringObjects = getNeighbouringObjects<LocatedNetEntity>(intrinsicCoordinate, walkDirection, rsmEntities, rsmNextNetElement);

                    // soon to be deleted, testing purpose
                    //foreach (LocatedNetEntity locatedNetEntity in neighbouringObjects)
                    //{
                    //    Debug.WriteLine(locatedNetEntity.name);
                    //    Debug.WriteLine(rsmNextNetElement.name);
                    //}

                    if (neighbouringObjects.Exists(item => item.name == "platform"))
                    {
                        #region analysis and defining required value for signal

                        var platform = neighbouringObjects.Find(item => item.name == "platform");
                        var platformLocation = rsmEntities.usesLocation.Find(item => item.id == platform.locations.First().@ref) as SpotLocation;

                        // find first danger point start from platform
                        var intrinsicCoordinateRef = platformLocation.associatedNetElements.First().bounds.First();
                        intrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == intrinsicCoordinateRef.@ref);
                        neighbouringObjects = rsmNextNetElement.getNeighbouringObjects<LocatedNetEntity>(intrinsicCoordinate, walkDirection, rsmEntities);
                        var firstDangerPoint = neighbouringObjects.First();
                        var dangerPointLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == firstDangerPoint.locations.First().@ref) as SpotLocation; // dereference
                        var dangerPointCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == dangerPointLocation.coordinates.First().@ref) as LinearCoordinate; // dereference

                        // exit aSignal calculation

                        // set first distance
                        var firstPoint = dangerPointCoordinate.measure.value.Value - 0.2; // measure in km

                        // calculate slope
                        var segment = isPinnedOn(rsmEntities, firstPoint, walkDirection);
                        slope = slopeCalculator(rsmEntities, segment, firstPoint);

                        // determine real safety distance based on rule
                        distance = Rules.safetyDistance(slope, speed) / 1000;
                        Debug.WriteLine(slope);
                        Debug.WriteLine(distance);

                        var signalKmValue = walkDirection == ApplicationDirection.normal ? dangerPointCoordinate.measure.value.Value - distance : dangerPointCoordinate.measure.value.Value + distance;

                        //intrinsic coordinate calculation for aSignal
                        var initialNERef = rsmNextNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                        var endNERef = rsmNextNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                        var initialNEValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(initialNERef))).measure.value.Value;
                        var endNEValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.id.Equals(endNERef))).measure.value.Value;
                        var length = (Length)rsmNextNetElement.elementLength.quantiy.First(item => item is Length);
                        var intrinsicValue = (signalKmValue - initialNEValue) / (length.value.Value / 1000);

                        // lateral side calculation
                        var lateralSide = Rules.sideAllocation(walkDirection, internalNextNetElement.trackType);

                        #endregion

                        // assign calculated value to each attributes
                        internalSignal.km = signalKmValue;

                        var associatedNetElement = new database.AssociatedNetElement
                        {
                            intrinsicValue = intrinsicValue,
                            lateralSide = lateralSide,
                            appliesInDirection = internalNextNetElement.codeDirection
                        };

                        internalSignal.associatedNetElements.Add(associatedNetElement);

                        // build rsm aSignal
                        Signal rsmExitSignal;
                        var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                        eulynxRsmAssetBuilder.buildSignal(rsmEntities, internalSignal, out rsmExitSignal);

                        // eulynx 
                        var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                        dbEulynxDataPrepBuilder.buildSignal(dataPrepEntities, rsmExitSignal, internalSignal);
                    }
                    else if (neighbouringObjects.Exists(item => item is Turnout))
                    {
                        var dangerPoint = neighbouringObjects.Find(item => item is Turnout);

                        // walk back to trace is there any aSignal that cover this turnout
                        intrinsicCoordinate = walkDirection == ApplicationDirection.normal ? rsmNextNetElement.associatedPositioning.First().intrinsicCoordinates.Last() : rsmNextNetElement.associatedPositioning.First().intrinsicCoordinates.First();
                        walkDirection = walkDirection == ApplicationDirection.normal ? ApplicationDirection.reverse : ApplicationDirection.normal; // flip the direction
                        neighbouringObjects = getNeighbouringObjects<LocatedNetEntity>(intrinsicCoordinate, walkDirection, rsmEntities, rsmNextNetElement);
                        if (neighbouringObjects.Exists(item => item is rsmSig.Signal))
                        {
                            // continue to next net element
                            walkDirection = walkDirection == ApplicationDirection.reverse ? ApplicationDirection.normal : ApplicationDirection.reverse; // flip the direction
                            nextNetElement(eulynx, db, rsmNetElement, internalSignal, rsmEntrySignal);
                        }
                        else
                        {
                            // put object
                            Debug.WriteLine("put object");

                            // get application direction of current net element
                            var applicationDirection = internalNextNetElement.codeDirection switch
                            {
                                1 => ApplicationDirection.normal,
                                2 => ApplicationDirection.reverse,
                                4 => ApplicationDirection.undefined
                            };

                            walkDirection = walkDirection == ApplicationDirection.reverse ? ApplicationDirection.normal : ApplicationDirection.reverse; // flip the direction
                            
                            // is direction of the track same like application direction of the aSignal?
                            if (applicationDirection == walkDirection)
                            {
                                // exit aSignal calculation

                                #region analysis and defining required value for exit signal

                                // get danger point coordinate
                                var dangerPointLocation = rsmEntities.usesLocation.FirstOrDefault(loc => loc.id == dangerPoint.locations.First().@ref) as SpotLocation; // dereference
                                var dangerPointIntrinsicCoordinate = rsmEntities.usesTopography.usesIntrinsicCoordinate.FirstOrDefault(iCoor => iCoor.id == dangerPointLocation.associatedNetElements.First().bounds.First().@ref); // dereference
                                var dangerPointCoordinate = rsmEntities.usesTopography.usesPositioningSystemCoordinate.FirstOrDefault(coor => coor.id == dangerPointIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate; // dereference

                                // set first point
                                var firstPoint = dangerPointCoordinate.measure.value.Value - 0.2; // measure in km

                                // calculate slope
                                var segment = isPinnedOn(rsmEntities, firstPoint, applicationDirection);
                                slope = slopeCalculator(rsmEntities, segment, firstPoint);
                                Debug.WriteLine(slope);

                                // determine real safety distance based on rule
                                distance = Rules.slip(slope, speed) / 1000;

                                // final calculation
                                var signalKmValue = walkDirection == ApplicationDirection.normal ? dangerPointCoordinate.measure.value.Value - distance : dangerPointCoordinate.measure.value.Value + distance;

                                //intrinsic coordinate calculation for aSignal
                                var initialNERef = rsmNextNetElement.associatedPositioning[0].intrinsicCoordinates[0].coordinates[0].@ref;
                                var endNERef = rsmNextNetElement.associatedPositioning[0].intrinsicCoordinates[1].coordinates[0].@ref;
                                var initialNEValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate?.Find(item => item.id.Equals(initialNERef)))?.measure?.value.Value;
                                var endNEValue = ((LinearCoordinate)rsmEntities.usesTopography.usesPositioningSystemCoordinate?.Find(item => item.id.Equals(endNERef)))?.measure?.value.Value;
                                var length = (Length)rsmNextNetElement.elementLength.quantiy.First(item => item is Length);
                                var intrinsicValue = (double)(signalKmValue - initialNEValue) / (length.value.Value / 1000);

                                // lateral side calculation
                                var lateralSide = Rules.sideAllocation(applicationDirection, internalNextNetElement.trackType);

                                #endregion

                                Debug.WriteLine(signalLocation);

                                // check if km of exit aSignal is in the area of aSignal folge abstand?
                                if (walkDirection == ApplicationDirection.normal ? signalKmValue < entrySignalKmValue + 0.9 : signalKmValue > entrySignalKmValue - 0.9)
                                {
                                    // continue to next net element
                                    nextNetElement(eulynx, db, rsmNextNetElement, internalSignal, rsmEntrySignal);
                                    break;
                                }
                                else if (walkDirection == ApplicationDirection.normal ? signalKmValue > entrySignalKmValue + 1.5 : signalKmValue < entrySignalKmValue - 1.5)
                                {
                                    signalKmValue = walkDirection == ApplicationDirection.normal ? entrySignalKmValue + 1.5 : entrySignalKmValue - 1.5;
                                }

                                // assign calculated value to each attributes
                                internalSignal.km = signalKmValue;

                                var associatedNetElement = new database.AssociatedNetElement
                                {
                                    netElementId = rsmNextNetElement.id,
                                    intrinsicValue = intrinsicValue,
                                    lateralSide = lateralSide,
                                    appliesInDirection = internalNextNetElement.codeDirection
                                };

                                internalSignal.associatedNetElements.Add(associatedNetElement);

                                // build rsm aSignal
                                Signal rsmExitSignal;
                                var eulynxRsmAssetBuilder = EulynxRsmAssetBuilder.getInstance();
                                eulynxRsmAssetBuilder.buildSignal(rsmEntities, internalSignal, out rsmExitSignal);

                                // build eulynx liteDB aSignal
                                var dbEulynxDataPrepBuilder = DbEulynxDataPrepAssetBuilder.getInstance();
                                dbEulynxDataPrepBuilder.buildSignal(dataPrepEntities, rsmExitSignal, internalSignal);

                                Debug.WriteLine("exit aSignal object is created . . .");
                                break;
                            }
                            else
                            {
                                var rsmCoverSignal = new Signal
                                {
                                    id = generateUUID(),
                                    name = "cover aSignal",
                                    longname = "covering" + dangerPoint.name,
                                    locations = null // where?
                                };
                                Debug.WriteLine("common aSignal object is created . . .");
                            }

                            // continue to next net element
                            nextNetElement(eulynx, db, rsmNextNetElement, internalSignal, rsmEntrySignal);
                        }
                    }
                    else
                    {
                        // next net element
                        //nextNetElement(currentNetElement, walkDirection, entrySignalKMValue, rsmEntities, signallingEntities);
                    }

                }
                else
                {
                    Debug.WriteLine("reached the end of the edge . . .");
                    continue;
                }
            }
        }

        /// <summary>
        /// Internal class method to get the neighbouring object start from given point.
        /// </summary>
        /// <param name="intrinsicCoordinate">point where the discovery is started</param>
        /// <param name="walkDirection">to which direction the program should walk towards to</param>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="netelement">net element ehich would be discovered</param>
        public static List<T> getNeighbouringObjects<T>(
            IntrinsicCoordinate intrinsicCoordinate, 
            ApplicationDirection walkDirection, 
            RsmEntities rsmEntities, 
            LinearElementWithLength netelement) where T : LocatedNetEntity
        {
            RsmEntities rsmEntities2 = rsmEntities;
            IntrinsicCoordinate intrinsicCoordinate2 = intrinsicCoordinate;
            if (walkDirection == ApplicationDirection.both || walkDirection == ApplicationDirection.undefined || intrinsicCoordinate2 == null)
            {
                return null;
            }

            IEnumerable<(tElementWithIDref, SpotLocation, double?)> source = from loc in rsmEntities2.usesLocation
                                                                             where loc is SpotLocation
                                                                             let refToSpot = new tElementWithIDref
                                                                             {
                                                                                 @ref = loc.id
                                                                             }
                                                                             let spot = loc as SpotLocation
                                                                             let iCrd = spot?.associatedNetElements?.FirstOrDefault((AssociatedNetElement ane) => ane.netElement!.@ref.Equals(netelement.id))?.bounds?[0]
                                                                             where iCrd != null
                                                                             let iCrdValue = (from ic in rsmEntities2.usesTopography!.usesIntrinsicCoordinate
                                                                                              where iCrd != null && ic.id == iCrd?.@ref
                                                                                              select ic.value.GetValueOrDefault())?.FirstOrDefault()
                                                                             where (walkDirection != ApplicationDirection.normal) ? (iCrdValue < intrinsicCoordinate2?.value) : (iCrdValue > intrinsicCoordinate2?.value)
                                                                             select (refToSpot, spot, iCrdValue);
            IOrderedEnumerable<(tElementWithIDref, SpotLocation, double?)> obj = ((walkDirection == ApplicationDirection.normal) ? source.OrderBy<(tElementWithIDref, SpotLocation, double?), double?>(((tElementWithIDref refToSpot, SpotLocation spot, double? iCrdValue) s) => s.iCrdValue) : source.OrderByDescending<(tElementWithIDref, SpotLocation, double?), double?>(((tElementWithIDref refToSpot, SpotLocation spot, double? iCrdValue) s) => s.iCrdValue));
            List<T> list = new List<T>();
            IEnumerable<T> source2 = (from o in ((IEnumerable<LocatedNetEntity>)rsmEntities2.ownsOnTrackSignallingDevice).Union((IEnumerable<LocatedNetEntity>)rsmEntities2.ownsPoint).Union(rsmEntities2.ownsSignal).Union(rsmEntities2.ownsVehicleStop)
                                      where o is T
                                      select o).Cast<T>();
            foreach (var sortedSpot in obj)
            {
                IEnumerable<T> collection = from locatedNetEntity in source2
                                            where locatedNetEntity.locations.Any((tElementWithIDref l) => l.@ref == sortedSpot.Item1.@ref)
                                            where locatedNetEntity != null
                                            select locatedNetEntity;
                list.AddRange(collection);
            }

            return list;
        }

    }
}