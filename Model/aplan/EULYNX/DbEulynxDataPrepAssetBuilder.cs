using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.sig;
using System.Collections.Generic;
using System.Linq;
using aplan.core;

using db = Models.TopoModels.EULYNX.db;

using Signal = Models.TopoModels.EULYNX.rsmSig.Signal;
using VehiclePassageDetector = Models.TopoModels.EULYNX.rsmSig.VehiclePassageDetector;
using TpsDevice = Models.TopoModels.EULYNX.rsmSig.TpsDevice;

using static aplan.core.Helper;


namespace aplan.eulynx
{
    class DbEulynxDataPrepAssetBuilder
    {
        //instance
        private static DbEulynxDataPrepAssetBuilder dbEulynxDataPrepAssetBuilder;

        //singleton constructor
        private DbEulynxDataPrepAssetBuilder() { }

        //singleton method
        public static DbEulynxDataPrepAssetBuilder getInstance()
        {
            if (dbEulynxDataPrepAssetBuilder == null)
            {
                dbEulynxDataPrepAssetBuilder = new DbEulynxDataPrepAssetBuilder();
            }
            return dbEulynxDataPrepAssetBuilder;
        }


        /// <summary>
        /// Build the EULYNX DB signal.
        /// APlan version
        /// TODO: only the variants that occur in Scheibenberg are in the enums for now.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmSignal">rsm signal which will be refer to</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        public void buildSignal(DataPrepEntities dataPrepEntities, Signal rsmSignal, database.Signal signal)
        {
            buildSignalFrame(dataPrepEntities, rsmSignal, signal);
            buildFixing(dataPrepEntities, rsmSignal, signal);
            buildDbSignal(dataPrepEntities, rsmSignal, signal);
            addSignalFunction(dataPrepEntities, signal);
            addSignalType(dataPrepEntities, signal);
        }

        /// <summary>
        /// Build the EULYNX axle counter.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmVPD">rsm vehichle passage detector which will be refer to</param>
        public void buildAxleCounter(DataPrepEntities dataPrepEntities, VehiclePassageDetector rsmVPD)
        {
            // since it just considers physical asset, only axle counting head will be created

            var axleCountingHead = new AxleCountingHead()
            {
                id = generateUUID(),
                refersToRsmVehiclePassageDetector = new tElementWithIDref { @ref = rsmVPD.id}
            };
            dataPrepEntities.ownsTrackAsset.Add(axleCountingHead);

        }

        /// <summary>
        /// Build the ETCS balise group level 2.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmTpsDevices">rsm tps devices which will be refer to</param>
        /// <param name="etcsBaliseGroupFunction">balise group function</param>
        public void buildEtcsBaliseGroupLevel2(DataPrepEntities dataPrepEntities, List<TpsDevice> rsmTpsDevices, string etcsBaliseGroupFunction)
        {
            // balise group track asset
            EtcsBaliseGroup etcsBaliseGroup;
            buildEtcsBaliseGroup(dataPrepEntities, out etcsBaliseGroup);

            foreach(var tpsDevice in rsmTpsDevices)
            {
                // single balises
                buildEtcsBalise(dataPrepEntities, tpsDevice, etcsBaliseGroup);
            }

            buildEtcsBaliseGroupProperties(dataPrepEntities, etcsBaliseGroup, etcsBaliseGroupFunction);

        }

        /// <summary>
        /// Build the ETCS balise group properties.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="etcsBaliseGroup">etcs balise group which will be refer to</param>
        /// <param name="etcsBaliseGroupFunction">balise group function</param>
        public void buildEtcsBaliseGroupProperties(DataPrepEntities dataPrepEntities, EtcsBaliseGroup etcsBaliseGroup, string etcsBaliseGroupFunction)
        {
            var etcsBalisGroupLevel2 = new EtcsBaliseGroupLevel2
            {
                id = generateUUID(),
                appliesToTpsDataTxSystem = new tElementWithIDref { @ref = etcsBaliseGroup.id },
                implementsFunction =
                {
                    new BaliseGroupFunction { id = generateUUID(), type = etcsBaliseGroupFunction }
                }
            };
            dataPrepEntities.ownsTpsDataTransmissionSystemProperties.Add(etcsBalisGroupLevel2);
        }

        /// <summary>
        /// Build the ETCS balise group.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="etcsBaliseGroup">rsm tps devices which will be refer to</param>
        public void buildEtcsBaliseGroup(DataPrepEntities dataPrepEntities, out EtcsBaliseGroup etcsBaliseGroup)
        {
            etcsBaliseGroup = new EtcsBaliseGroup
            {
                id = generateUUID()
            };
            dataPrepEntities.ownsTrackAsset.Add(etcsBaliseGroup);
        }

        /// <summary>
        /// Build the ETCS balise group.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmTpsDevice">rsm tps devices which will be refer to</param>
        /// <param name="etcsBaliseGroup">rsm tps devices which will be refer to</param>
        public void buildEtcsBalise(DataPrepEntities dataPrepEntities, TpsDevice rsmTpsDevice, EtcsBaliseGroup etcsBaliseGroup)
        {
            var etcsBalise = new EtcsBalise
            {
                id = generateUUID(),
                refersToRsmTpsDevice = new tElementWithIDref { @ref = rsmTpsDevice.id },
                implementsTpsDataTxSystem = new tElementWithIDref { @ref = etcsBaliseGroup.id },

            };
            dataPrepEntities.ownsTrackAsset.Add(etcsBalise);
        }

        /// <summary>
        /// Build the ETCS signal frame.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmSignal">rsm signal which will be refer to</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        private static void buildSignalFrame(DataPrepEntities dataPrepEntities, Signal rsmSignal, database.Signal signal)
        {
            // store signal frame
            var signalFrame = new SignalFrame
            {
                id = generateUUID(),
                isOfSignalFrameType = convertToSignalFrameType(signal.signalType), // helper
                isFixed = true,
                hasPosition =
                            {
                                new HorizontalOffsetOfReferencePoint
                                {
                                    isLocatedAt = new tElementWithIDref{@ref = rsmSignal.locations.First().@ref},
                                    value = new Length{ value=Rules.lateralDistance(1/*assuming this is "gerade"*/,0/*radius should be determine by segment*/,"normal")} // need to be called from rule
                                }
                            }
            };
            signal.signalFrameId = signalFrame.id;
            dataPrepEntities.ownsSignalFrame.Add(signalFrame);
        }

        /// <summary> 
        /// Build the ETCS fixing.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmSignal">rsm signal which will be refer to</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        private static void buildFixing(DataPrepEntities dataPrepEntities, Signal rsmSignal, database.Signal signal)
        {
            // store fixing
            var fixing = new Fixing
            {
                id = generateUUID(),
                isLocatedAt =
                        {
                            new tElementWithIDref{@ref=rsmSignal.locations.First().@ref}
                        },
                isOfFixingType = FixingTypes.post,
                supportsSignalFrame =
                        {
                            new tElementWithIDref{@ref=signal.signalFrameId}
                        }
            };
            dataPrepEntities.ownsEquipmentSupport.Add(fixing);
        }

        /// <summary> 
        /// Build the Deutsche Bahn Signal.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="rsmSignal">rsm signal which will be refer to</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        private static void buildDbSignal(DataPrepEntities dataPrepEntities, Signal rsmSignal, database.Signal signal)
        {
            // store DB signal
            // must be lightsignal typed?
            
            
            var dbSignal = new db.LightSignalTyped
            {
                id = generateUUID(),
                refersToRsmSignal = new tElementWithIDref { @ref = rsmSignal.id },
                hasConfiguration = new Configuration
                {
                    id = generateUUID(),
                    hasConfigurationProperty =
                                {
                                    new db.Designation
                                    {
                                    id = generateUUID(),
                                    nameOnSite = "Naming convention tba.",
                                    shortNameLayoutPlan = "Naming convention tba.",
                                    longNameLayoutPlan = "Naming convention tba.",
                                    nameInTable =  "Naming convention tba."
                                    }
                                }
                },
                isDesignedForFixingType = convertToFixingType(signal.fixingType),
                hasSignalFrame =
                            {
                                new tElementWithIDref{ @ref = signal.signalFrameId }
                            }
            };
            signal.signalRsmId = dbSignal.id;
            dataPrepEntities.ownsTrackAsset.Add(dbSignal);
        }

        /// <summary> 
        /// Add signal function.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        private static void addSignalFunction(DataPrepEntities dataPrepEntities, database.Signal signal)
        {
            // store signal function
            dataPrepEntities.ownsSignalFunction.Add(
                new db.SignalFunction
                {
                    id = generateUUID(),
                    appliesToSignal = new tElementWithIDref { @ref = signal.signalRsmId },
                    description = "Einfahr_Signal",
                    isOfSignalFunctionType = convertToDbSignalFuntionType(signal.signalFunction)
                });

           
        }

        /// <summary> 
        /// Add signal type.
        /// </summary>
        /// <param name="dataPrepEntities">dataprep entities object</param>
        /// <param name="signal">aplan database signal which will be refer to</param>
        private static void addSignalType(DataPrepEntities dataPrepEntities, database.Signal signal)
        {
            // store signal type
            dataPrepEntities.ownsSignalType.Add(
                new db.SignalType
                {
                    id = generateUUID(),
                    appliesToSignal = new tElementWithIDref { @ref = signal.signalRsmId },
                    isOfSignalTypeType = convertToDbSignalTypeType(signal.signalType)
                }
                );

            
        }
    
    }
}
