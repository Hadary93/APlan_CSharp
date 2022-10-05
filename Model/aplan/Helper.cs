using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.sig;
using Models.TopoModels.EULYNX.rsmNE;
using Models.TopoModels.EULYNX.rsmTrack;
using GeoJSON.Net.Feature;
using aplan.database;
using LiteDB;

using db = Models.TopoModels.EULYNX.db;

using Signal = Models.TopoModels.EULYNX.rsmSig.Signal;
using System.IO;

namespace aplan.core
{
    class Helper
    {


        /// <summary>
        /// Method to parse km value from station format
        /// </summary>
        /// <param name="originalValue"></param>
        public static double valueParser(double originalValue)
        {
            double temp, km, m;
            temp = originalValue % 10000000;
            km = Math.Round(temp / 100000, 1);
            m = Math.Round(temp % 100, 3);
            temp = km * 1000 + m;
            return temp / 1000;
        }


        /// <summary>
        /// Method to round any numbers up.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="precision"></param>
        public static double roundUp(double input, int precision)
        {
            double multiplier = Math.Pow(10, Convert.ToDouble(precision));
            return Math.Ceiling(input * multiplier) / multiplier;
        }


        /// <summary>
        /// Method to generate UUID.
        /// </summary>
        public static string generateUUID()
        {
            return Guid.NewGuid().ToString();
        }


        /// <summary>
        /// Method to check collection availability.
        /// </summary>
        public static bool collectionIsNull(FeatureCollection featureCollection)
        {
            if (featureCollection == null)
            {
                Console.WriteLine("Collection is not found");
                return true;
            }
            return false;
        }


        /// <summary>
        /// Method to check collection availabilityin the database.
        /// </summary>
        public static bool collectionIsNull<T>(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                Console.WriteLine("Collection is not found");
                return true;
            }
            return false;
        }


        /// <summary>
        /// Method to refer a unit.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="unitName"></param>
        public static tElementWithIDref referUnit(RsmEntities rsmEntities, string unitName)
        {
            tElementWithIDref reference = new();
            try
            {
                reference.@ref = rsmEntities.usesUnit.Find(item => item.name.Equals(unitName)).id;
                return reference;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("unit is not existed");
                reference.@ref = "unit is not available";
                return reference;
            }
            return reference;
        }


        /// <summary>
        /// Method to refer an elevation.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="value"></param>
        public static tElementWithIDref referElevation(RsmEntities rsmEntities, double value)
        {
            tElementWithIDref reference = new();
            try
            {
                reference.@ref = rsmEntities.usesTopography.usesElevationAndInclination.Find(item => item.inclination.value == value).id;
                return reference;
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("elevation is not existed");
                //reference.@ref = "elevation is not available";
                return reference;
            }
            return reference;
        }


        /// <summary>
        /// Method to refer a cant.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="id">cant id</param>
        public static tElementWithIDref referCant(RsmEntities rsmEntities, string id)
        {
            tElementWithIDref reference = new();
            try
            {
                reference.@ref = rsmEntities.usesTopography.usesCant.Find(item => item.id.Equals(id)).id;
                return reference;
            }
            catch (NullReferenceException)
            {
                Debug.WriteLine("cant is not existed");
                return reference;
            }
            return reference;
        }


        /// <summary>
        /// Method to refer a coordinate.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="coordinateName">name of the coordinate</param>
        public static tElementWithIDref referCoordinate(RsmEntities rsmEntities, string coordinateName)
        {
            tElementWithIDref reference = new();
            try
            {
                reference.@ref = rsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(item => item.name.Equals(coordinateName)).id;
                return reference;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("coordinate is not existed");
                reference.@ref = "coordinate is not available";
                return reference;
            }
            return reference;
        }


        /// <summary>
        /// Method to refer a positioning system.
        /// </summary>
        /// <param name="rsmEntities"></param>
        /// <param name="positioningSystemName"></param>
        public static tElementWithIDref referPositioningSystem(RsmEntities rsmEntities, string positioningSystemName)
        {
            tElementWithIDref reference = new();
            try
            {
                reference.@ref = rsmEntities.usesTopography.usesPositioningSystem.Find(item => item.name.Equals(positioningSystemName)).id;
                return reference;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("positioning system is not existed");
                reference.@ref = "positioning system is not available";
                return reference;
            }
            return reference;
        }


        /// <summary>
        /// Method to convert string to lateral side enum.
        /// </summary>
        /// <param name="value">string value ("right", "left", etc.</param>
        public static LateralSide convertToLateralSide(string value)
        {
            switch (value)
            {
                case "right":
                    return LateralSide.right;
                case "left":
                    return LateralSide.left;
                case "center":
                    return LateralSide.centre;
                case "both":
                    return LateralSide.both;
                default:
                    return LateralSide.undefined;
            }
        }


        /// <summary>
        /// Method to convert int to application direction enum.
        /// </summary>
        /// <param name="value">int value (1, 2, etc.</param>
        public static ApplicationDirection convertToApplicationDirection(int value)
        {
            switch (value)
            {
                case 1:
                    // can be both as well, depends on situation
                    // but now let it be one case
                    return ApplicationDirection.normal;
                case 2:
                    return ApplicationDirection.reverse;
                case 4:
                    return ApplicationDirection.undefined;
                default:
                    return ApplicationDirection.undefined;
            }
        }

        /// <summary>
        /// Method to convert string to signal frame type enum.
        /// </summary>
        /// <param name="value">string value ("main", "shunting", etc.</param>
        public static SignalFrameTypes convertToSignalFrameType(string value)
        {
            switch (value)
            {
                // more types are available, but these should be enough for the time being
                case "main":
                    return SignalFrameTypes.main;
                case "shunting":
                    return SignalFrameTypes.shunting;
                case "sign":
                    return SignalFrameTypes.sign;
                case "slab":
                    return SignalFrameTypes.slab;
                default:
                    return SignalFrameTypes.other;
            }
        }


        /// <summary>
        /// Method to convert string to fixing type enum.
        /// </summary>
        /// <param name="value">string value ("post", "pole", etc.</param>
        public static FixingTypes convertToFixingType(string value)
        {
            switch (value)
            {
                // more types are available, but these should be enough for the time being
                case "post":
                    return FixingTypes.post;
                case "foundation":
                    return FixingTypes.foundation;
                case "pole":
                    return FixingTypes.pole;
                case "platform":
                    return FixingTypes.platform;
                default:
                    return FixingTypes.other;
            }
        }


        /// <summary>
        /// Method to convert string to DB signal function type enum.
        /// </summary>
        /// <param name="value">string value ("entry", "exit", etc.</param>
        public static db.SignalFunctionTypes convertToDbSignalFuntionType(string value)
        {
            switch (value)
            {
                // more types are available, but these should be enough for the time being
                case "entry":
                    return db.SignalFunctionTypes.entry;
                case "exit":
                    return db.SignalFunctionTypes.exit;
                case "block":
                    return db.SignalFunctionTypes.block;
                case "protection":
                    return db.SignalFunctionTypes.protection;
                default:
                    return db.SignalFunctionTypes.further;
            }
        }


        /// <summary>
        /// Method to convert string to DB signal type enum.
        /// </summary>
        /// <param name="value">string value ("main", "shunting", etc.</param>
        public static db.SignalTypeTypes convertToDbSignalTypeType(string value)
        {
            switch (value)
            {
                // more types are available, but these should be enough for the time being
                case "main":
                    return db.SignalTypeTypes.main;
                case "shunting":
                    return db.SignalTypeTypes.shunting;
                case "distant":
                    return db.SignalTypeTypes.distant;
                case "repeater":
                    return db.SignalTypeTypes.repeater;
                default:
                    return db.SignalTypeTypes.main;
            }
        }


        /// <summary>
        /// Method to convert located net entity object to string.
        /// </summary>
        /// <param name="locatedNetEntity">located net entity found</param>
        /// <param name="db">lite database</param>
        public static string convertLocatedEntityTypeToString(LocatedNetEntity locatedNetEntity, LiteDatabase db)
        {
            if(locatedNetEntity is Signal)
            {
                var id = locatedNetEntity.id;
                var aSignal = db.GetCollection<database.Signal>("Signals").Find(x => x.uuid == id).FirstOrDefault();
                return aSignal.signalType;
            }
            else if(locatedNetEntity is Turnout)
            {
                var id = locatedNetEntity.id;
                var aTurnout = db.GetCollection<TrackEntity>("TrackEntities").Find(x => x.uuid == id).FirstOrDefault();
                return aTurnout.trackEntityObjectElement;
            }
            return "";

           
        }


        /// <summary>
        /// Transforms hectometer+meter notation into km
        /// </summary>
        /// <param name="hektometerPlusMeter">hektometer + meter</param>
        /// <returns>kilometre value, e.g. 74.752</returns>
        public static double? hmToKm(string hektometerPlusMeter = "74,7 + 52")
        {
            if (hektometerPlusMeter == null)
                return 0;

            var hm_m = hektometerPlusMeter.Split('+');
            if (hm_m.Count() != 2) return null;

            if (double.TryParse(hm_m[0], out var hm) && double.TryParse(hm_m[1], out var m))
                return hm + m / 1000;
            else
                return null;
        }


        /// <summary>
        /// Transforms hectometer+meter notation into km
        /// </summary>
        /// <param name="hektometerPlusMeter">hektometer + meter</param>
        /// <returns>kilometre value, e.g. 74.752</returns>
        public static double? hmToKm_mileage(string hektometerPlusMeter)
        {
            if (hektometerPlusMeter == null)
                return 0;

            var hm_m = hektometerPlusMeter.Split('+');
            if (hm_m.Count() != 2) return null;

            if (double.TryParse(hm_m[0], out var hm) && double.TryParse(hm_m[1], out var m))
                return hm / 10 + m / 1000;
            else
                return null;
        }


        /// <summary>
        /// Collects the VON and BIS values, and returns the lowest and highest chainage in kilometre.
        /// </summary>
        /// <param name="database">internal database</param>
        /// <returns></returns>
        public static (double? min, double? max) findMinMax(LiteDatabase database)
        {
            IEnumerable<double?[]> min_max;
            IOrderedEnumerable<double?> sortedChainages;
            min_max = from record in database.GetCollection<Mileage>("Mileage").FindAll()
                      let min = record.startKM
                      let max = record.endKM
                      select new double?[] { min, max };

            sortedChainages = min_max
                .SelectMany(v => v)
                .OrderBy(v => v);
            

            return (min: sortedChainages.FirstOrDefault(), max: sortedChainages.LastOrDefault());
        }


        /// <summary>
        /// Method to delete existing file.
        /// </summary>
        /// <param name="path">path to the file</param>
        public static void deleteFile(string path)
        {
            try
            {
                // Check if file exists with its full path    
                if (File.Exists(path))
                {
                    // If file found, delete it    
                    File.Delete(path);
                    Debug.WriteLine("File deleted.");
                }
                else Debug.WriteLine("File not found");
            }
            catch (IOException ioExp)
            {
                Debug.WriteLine(ioExp.Message);
            }
        }



        //public static (double? min, double? max) findMinMaxLinearCoordinate(LiteDatabase database)
        //{
        //    IEnumerable<double?[]> min_max;
        //    IOrderedEnumerable<double?> sortedChainages;
        //    min_max = from record in database.GetCollection<database.LinearCoordinate>("LinearCoordinates").FindAll()
        //              let min = record.linearCoordinate
        //              let max = record.linearCoordinate
        //              select new double?[] { min, max };

        //    sortedChainages = min_max
        //        .SelectMany(v => v)
        //        .OrderBy(v => v);


        //    return (min: sortedChainages.FirstOrDefault(), max: sortedChainages.LastOrDefault());
        //}
        //public static (double? min, double? max) FindChainageMinMax(FeatureCollection collection)
        //{
        //    var von_bis = from f in collection?.Features
        //                  let von = hmToKm_mileage((string)f.Properties["KM_A_TEXT"])
        //                  let bis = hmToKm_mileage((string)f.Properties["KM_E_TEXT"])
        //                  select new double?[] { von, bis };

        //    var sortedChainages = von_bis
        //        .SelectMany(v => v)
        //        .OrderBy(v => v);

        //    return (min: sortedChainages.FirstOrDefault(), max: sortedChainages.LastOrDefault());
        //}



    }
}