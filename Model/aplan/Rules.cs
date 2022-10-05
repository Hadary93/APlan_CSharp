using Models.TopoModels.EULYNX.rsmCommon;

namespace aplan.core
{
    class Rules
    {

        /*
        * Die Signale sind so anzuordnen, dass ihre Zuordnung zu Gleisen mit Zug- oder Rangierfahrten, für die sie gelten,eindeutig ist.
        * Hauptsignale sind grundsätzlich wie folgt anzuordnen:
        * a) an eingleisigen Strecken und in Bahnhöfen unmittelbar rechts neben oder über dem zugehörigen Gleis;
        * b) an zweigleisigen Strecken für das:
            - Regelgleis unmittelbar rechts,
            - Gegengleis unmittelbar links neben oder über dem zugehörigen Gleis.
        * 
        */
        /// <summary>
        /// b) Gleiszuordnung.
        /// Method to determine which side the estimated signal is allocated
        /// Taken from 819.0202 Chapter 4 Verse 2
        /// </summary>
        /// <param name="direction">direction of the track</param>
        /// <param name="trackType">type of the track (single/double)</param>
        public static string sideAllocation(ApplicationDirection direction, string trackType)
        {
            switch (trackType)
            {
                case "single":
                    return "right";
                case "double":
                    if (direction == ApplicationDirection.normal)
                    {
                        return "right";
                    }
                    else if (direction == ApplicationDirection.reverse)
                    {
                        return "left";
                    }
                    break;
                default:
                    return "right";
            }
            return null;
        }


        /// <summary>
        /// b) Gleiszuordnung.
        /// Method to determine which side the estimated signal is allocated
        /// Taken from 819.0202 Chapter 4 Verse 2
        /// </summary>
        /// <param name="trackForm"></param>
        /// <param name="radius">type of the track (single/double)</param>
        /// <param name="situationCase">type of the track (single/double)</param>
        public static double lateralDistance(int trackForm, double radius, string situationCase)
        {
            if (trackForm == 1 || (trackForm == 2 && radius > 250))
            {
                switch (situationCase)
                {
                    case "normal":
                        return 3.10;
                        break;
                    case "platformArea":
                        return 3.00;
                        break;
                    case "station":
                        return 2.20;
                        break;
                    case "inBetween":
                        return 2.20;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                // according to regelwerk, we should pay attention to EBO 9
                // what is EBO 9?
            }
            return 0;
        }


        /// <summary>
        /// d) Abstand vom Gefahrpunkt.
        /// internal function to calculate the flexion of safety distance
        /// Taken from 819.0202 Chapter 11
        /// </summary>
        /// <param name="slope">bigger value</param>
        internal static double flexionCalculator(double slope)
        {
            const double distance = 200;
            double extension = distance * (slope * 0.05);
            return distance + extension;
        }


        /// <summary>
        /// d) Abstand vom Gefahrpunkt.
        /// internal function to calculate the extension of safety distance
        /// Taken from 819.0202 Chapter 11
        /// </summary>
        /// <param name="slope">bigger value</param>
        internal static double extensionCalculator(double slope)
        {
            const double distance = 200;
            double extension = distance * (slope * 0.1);
            return distance + extension;
        }


        /// <summary>
        /// d) Abstand vom Gefahrpunkt.
        /// Deliver the safety distance for the main signal to be implemented from the first danger point.
        /// Taken from 819.0202 Chapter 11
        /// </summary>
        /// <param name="slope">bigger slope value</param>
        /// <param name="speed">speed allowance on the track</param>
        public static double safetyDistance(double slope, double speed)
        {
            if (slope == 0)
            {
                if (speed >= 100)
                {
                    return 200;
                }
                else
                {
                    return 100;
                }
            }
            else if (slope > 0)
            {
                //return flexionCalculator(slope);
                return 200;
            }
            else
            {
                if (extensionCalculator(slope) > 300)
                {
                    return 300;
                }
                else
                {
                    return extensionCalculator(slope);
                }
            }
            return 0;
        }


        public static double slip(double slope, double speed)
        {
            if (slope == 0)
            {
                if (speed >= 100)
                {
                    return 200;
                }
                else
                {
                    return 100;
                }
            }
            else if (slope > 0)
            {
                //return flexionCalculator(slope);
                return 200;
            }
            else
            {
                if (extensionCalculator(slope) > 300)
                {
                    return 300;
                }
                else
                {
                    return extensionCalculator(slope);
                }
            }
            return 0;
        }

        // true = open
        // false = close
        public static bool isInRestrictedArea(bool boundaryType, double boundaryLocation1, double boundaryLocation2, double objectLocation)
        {
            double firstPoint, secondPoint;
            switch (boundaryType)
            {
                case true:
                    firstPoint = boundaryLocation1 - 100;
                    secondPoint = boundaryLocation2 + 100;
                    if (objectLocation >= firstPoint && objectLocation <= secondPoint)
                    {
                        return true;
                    }
                    return false;
                case false:
                    firstPoint = boundaryLocation1 - 50;
                    secondPoint = boundaryLocation2 + 50;
                    if (objectLocation >= firstPoint && objectLocation <= secondPoint)
                    {
                        return true;
                    }
                    return false;
            }
        }


        public static double axleCounter(string condition)
        {
            // unit in meter
            switch (condition)
            {
                case "main":
                    return 0;
                case "shunting":
                    return 0;
                case "flank_protection":
                    return 0;
                case "switch_tip":
                    return 3;
                case "switch_boundary_mark":
                    return 6;
                case "switch_boundary_mark_with_protection":
                    return 16;
                default:
                    return 0;
            }
        }

    }
}