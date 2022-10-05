using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.generic;

using LinearCoordinate = Models.TopoModels.EULYNX.rsmCommon.LinearCoordinate;
using AssociatedNetElement = Models.TopoModels.EULYNX.rsmCommon.AssociatedNetElement;

using Signal = Models.TopoModels.EULYNX.rsmSig.Signal;
using VehiclePassageDetector = Models.TopoModels.EULYNX.rsmSig.VehiclePassageDetector;
using TpsDevice = Models.TopoModels.EULYNX.rsmSig.TpsDevice;

using static aplan.core.Helper;

namespace aplan.eulynx
{
    class EulynxRsmAssetBuilder
    {
        // instance
        private static EulynxRsmAssetBuilder eulynxRsmAssetBuilder;

        // singleton constructor
        private EulynxRsmAssetBuilder() { }

        // singleton method
        public static EulynxRsmAssetBuilder getInstance()
        {
            if (eulynxRsmAssetBuilder == null)
            {
                eulynxRsmAssetBuilder = new EulynxRsmAssetBuilder();
            }
            return eulynxRsmAssetBuilder;
        }

        /// <summary>
        /// Build the RSM signal.
        /// </summary>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="aSignal">aplan database signal which will be refer to</param>
        /// <param name="rsmSignal">produced rsm signal</param>
        public void buildSignal(RsmEntities rsmEntities, database.Signal aSignal, out Signal rsmSignal)
        {
            // help variables
            var km = referUnit(rsmEntities, "kilometer");

            // instatiate aSignal coordinate
            var signalCoordinate = new LinearCoordinate
            {
                id = generateUUID(),
                name = "LinearCoordinate" + aSignal.km,
                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
            };
            signalCoordinate.measure = new Length { value = aSignal.km, unit = km };

            // aSignal cartesian coordinate?

            // add coordinate to rsm entities container
            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(signalCoordinate);


            // instantiate spot km
            SpotLocation signalLocation = new SpotLocation
            {
                id = generateUUID(),
                //coordinates = { new tElementWithIDref { @ref = signalCoordinate.id } }
            };

            foreach (var item in aSignal.associatedNetElements)
            {
                // instantiate aSignal intrinsic coordinate
                var signalIntrinsicCoordinate = new IntrinsicCoordinate
                {
                    id = generateUUID(),
                    value = item.intrinsicValue,
                    coordinates =
                    {
                         new tElementWithIDref { @ref = signalCoordinate.id }
                    }
                    
                };

                // add intrinsic coordinate to rsm entities container
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(signalIntrinsicCoordinate);

                // instantiate associatedNetElement
                var associatedNetElement = new AssociatedNetElement
                {
                    isLocatedToSide = convertToLateralSide(item.lateralSide),
                    appliesInDirection = convertToApplicationDirection(item.appliesInDirection),
                    bounds = { new tElementWithIDref { @ref = signalIntrinsicCoordinate.id} },
                    netElement = new tElementWithIDref { @ref = item.netElementId }
                };

                signalLocation.associatedNetElements.Add(associatedNetElement);
            }

            // add aSignal spot km to rsm entities container
            rsmEntities.usesLocation.Add(signalLocation);

            // construct and store the aSignal's spot km
            rsmSignal = new Signal
            {
                id = aSignal.uuid,
                name = aSignal.signalFunction, // need a proper name(?)
                longname = aSignal.signalType, // need a proper name(?)
                locations = { new tElementWithIDref { @ref = signalLocation.id } }
            };

            rsmEntities.ownsSignal.Add(rsmSignal);
        }

        /// <summary>
        /// Build the RSM axle counter.
        /// </summary>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="aAxleCounter">aplan database axle counter which will be refer to</param>
        /// <param name="rsmAxleCounter">produced rsm axle counter</param>
        public void buildAxleCounter(RsmEntities rsmEntities, database.AxleCounter aAxleCounter, out VehiclePassageDetector rsmAxleCounter)
        {
            // help variables
            var km = referUnit(rsmEntities, "kilometer");

            // instatiate axle counter coordinate
            var axleCounterCoordinate = new LinearCoordinate
            {
                id = generateUUID(),
                name = "LinearCoordinate" + aAxleCounter.km,
                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
            };
            axleCounterCoordinate.measure = new Length { value = aAxleCounter.km, unit = km };

            // axle counter cartesian coordinate?

            // add coordinate to rsm entities container
            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(axleCounterCoordinate);


            // instantiate spot km
            SpotLocation axleCounterLocation = new SpotLocation
            {
                id = generateUUID()
            };

            foreach (var item in aAxleCounter.associatedNetElements)
            {
                // instantiate axle counter intrinsic coordinate
                var axleCounterIntrinsicCoordinate = new IntrinsicCoordinate
                {
                    id = generateUUID(),
                    value = item.intrinsicValue,
                    coordinates = { new tElementWithIDref { @ref = axleCounterCoordinate.id } }
                   
                };

                // add intrinsic coordinate to rsm entities container
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(axleCounterIntrinsicCoordinate);

                // instantiate associatedNetElement
                var associatedNetElement = new AssociatedNetElement
                {
                    isLocatedToSide = convertToLateralSide(item.lateralSide),
                    appliesInDirection = convertToApplicationDirection(item.appliesInDirection),
                    bounds = { new tElementWithIDref { @ref = axleCounterIntrinsicCoordinate.id } },
                    netElement = new tElementWithIDref { @ref = item.netElementId }
                };

                axleCounterLocation.associatedNetElements.Add(associatedNetElement);
            }

            // add aSignal spot km to rsm entities container
            rsmEntities.usesLocation.Add(axleCounterLocation);

            // construct and store the aSignal's spot km
            rsmAxleCounter = new VehiclePassageDetector
            {
                id = generateUUID(),
                name = aAxleCounter.name, // need a proper name(?)
                longname = aAxleCounter.longname, // need a proper name(?)
                locations = { new tElementWithIDref { @ref = axleCounterLocation.id } }
            };

            rsmEntities.ownsOnTrackSignallingDevice.Add(rsmAxleCounter);
        }

        /// <summary>
        /// Build the RSM tps device.
        /// </summary>
        /// <param name="rsmEntities">rsm entities container</param>
        /// <param name="aTpsDevice">aplan database axle counter which will be refer to</param>
        /// <param name="rsmTpsDevice">produced rsm axle counter</param>
        public void buildTpsDevice(RsmEntities rsmEntities, database.TpsDevice aTpsDevice, out TpsDevice rsmTpsDevice)
        {
            // help variables
            var km = referUnit(rsmEntities, "kilometer");

            // instatiate aTpsDevice coordinate
            var tpsDeviceCoordinate = new LinearCoordinate
            {
                id = generateUUID(),
                name = "LinearCoordinate" + aTpsDevice.km,
                positioningSystem = referPositioningSystem(rsmEntities, "Relative Positioning System"),
            };
            tpsDeviceCoordinate.measure = new Length { value = aTpsDevice.km, unit = km };

            // aSignal cartesian coordinate?

            // add coordinate to rsm entities container
            rsmEntities.usesTopography.usesPositioningSystemCoordinate.Add(tpsDeviceCoordinate);

            // instantiate spot km
            SpotLocation tpsDevicelLocation = new SpotLocation { id = generateUUID() };

            foreach (var item in aTpsDevice.associatedNetElements)
            {
                // instantiate aSignal intrinsic coordinate
                var tpsDeviceIntrinsicCoordinate = new IntrinsicCoordinate
                {
                    id = generateUUID(),
                    value = item.intrinsicValue,
                    coordinates =
                    {
                         new tElementWithIDref { @ref = tpsDeviceCoordinate.id }
                    }

                };

                // add intrinsic coordinate to rsm entities container
                rsmEntities.usesTopography.usesIntrinsicCoordinate.Add(tpsDeviceIntrinsicCoordinate);

                // instantiate associatedNetElement
                var associatedNetElement = new AssociatedNetElement
                {
                    isLocatedToSide = convertToLateralSide(item.lateralSide),
                    appliesInDirection = convertToApplicationDirection(item.appliesInDirection),
                    bounds = { new tElementWithIDref { @ref = tpsDeviceIntrinsicCoordinate.id } },
                    netElement = new tElementWithIDref { @ref = item.netElementId }
                };

                tpsDevicelLocation.associatedNetElements.Add(associatedNetElement);
            }

            // add aSignal spot km to rsm entities container
            rsmEntities.usesLocation.Add(tpsDevicelLocation);

            // construct and store the aSignal's spot km
            rsmTpsDevice = new TpsDevice
            {
                id = aTpsDevice.uuid,
                name = aTpsDevice.name, // need a proper name(?)
                longname = aTpsDevice.name, // need a proper name(?)
                locations = { new tElementWithIDref { @ref = tpsDevicelLocation.id } }
            };

            rsmEntities.ownsOnTrackSignallingDevice.Add(rsmTpsDevice);
        }
    }
}
