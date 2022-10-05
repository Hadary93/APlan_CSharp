using Models.TopoModels.EULYNX.generic;

namespace aplan.core
{
    public interface IModelService
    {
        public abstract void inputHandling(
            string format, Database database,
            string mileage, string edge, string node, string ha, string va, string ca,
            string mdb);
        public abstract object objectsCreation(string country, Database database);
        public abstract void serialization(EulynxDataPrepInterface eulynx, string station, string outputPath);
        public abstract EulynxDataPrepInterface deserialization(string path);
        public abstract void validation();

    }
}
