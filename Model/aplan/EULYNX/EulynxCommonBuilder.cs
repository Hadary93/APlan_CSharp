using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using System.Linq;
using aplan.core;

namespace aplan.eulynx
{
    class EulynxCommonBuilder
    {
        /// <summary>
        /// Method to define unit.
        /// </summary>
        /// <param name="eulynx"></param>
        /// <param name="name"></param>
        /// <param name="symbol"></param>
        public static void addUnit(EulynxDataPrepInterface eulynx, string name, string symbol)
        {
            Unit unit = new Unit()
            {
                id = Helper.generateUUID(),
                name = name,
                symbol = symbol
            };
            eulynx.hasDataContainer.First().ownsRsmEntities.usesUnit.Add(unit);
        }
    }
}
