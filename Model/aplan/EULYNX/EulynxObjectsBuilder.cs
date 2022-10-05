using Models.TopoModels.EULYNX.generic;
using aplan.core;

namespace aplan.eulynx
{
    abstract class EulynxObjectsBuilder
    {
        protected EulynxDataPrepInterface eulynx;
        
        public abstract void buildContainer();
        public abstract void buildGeneralObjects();
        public abstract void buildPositioningObjects(Database database);
        public abstract void buildTopologyObjects(Database database);
        public abstract void buildTopographyObjects(Database database);
        public abstract void buildInfrastructureObjects(Database database);

    }

    

    //class SNCFEulynxObjectsCreator : EulynxObjectsCreator
    //{
    //    public override EulynxDataPrepInterfaceExtension buildContainer()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void createPositioningObjects()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void createTopologyObjects()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void createTopographyObjects()
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public override void createInfrastructureObjects()
    //    {
    //        throw new NotImplementedException();
    //    }
    //}
}

