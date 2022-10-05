using APLan.Commands;
using APLan.HelperClasses;
using APLan.Views;

using aplan.core;

using Models.TopoModels.EULYNX.rsmCommon;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.sig;
using Models.TopoModels.EULYNX.db;

using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace APLan.ViewModels
{
    public class PlanningTabViewModel : INotifyPropertyChanged
    {
        #region INotify Essentials
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion

        #region attributes
        private string planType;
        private string exportType;
        public string PlanType
        {
            get => planType;
            set
            {
                planType = value.Split(':')[1].Trim();
                OnPropertyChanged();
            }
        }
        public string ExportType
        {
            get => exportType;
            set
            {
                exportType = value.Split(':')[1].Trim();
                OnPropertyChanged();
            }
        }
        List<PositioningNetElement> VisitedElements;
        public ObservableCollection<Signalinfo> Signals
        {
            get;
            set;
        }
        PositioningNetElement neededElement = null;
        #endregion

        #region commands
        public ICommand PlanButton { get; set; }
        public ICommand ExportButton { get; set; }
        #endregion

        #region constructor
        public PlanningTabViewModel()
        {
            PlanButton = new RelayCommand(ExecutePlanButton);
            ExportButton = new RelayCommand(ExecuteExportButton);
            Signals = new ObservableCollection<Signalinfo>();
            VisitedElements = new List<PositioningNetElement>();
        }
        #endregion

        #region logic
        public void ExecutePlanButton(object parameter)
        {
            var EulynxObject= ModelViewModel.eulynx;
            var eulynxService = ModelViewModel.eulynxService;
            Database dataBase = ModelViewModel.db;
            bool etcs = false;
            if (EulynxObject == null)
            {
                MessageBox.Show("No current Eulynx object");

            }else if (PlanType=="ETCS" && EulynxObject!=null)
            {
                etcs = true;
                eulynxService.plan(EulynxObject, dataBase, etcs);
                extractMainSignals(EulynxObject);
                extractOnTrackSignals(EulynxObject, dataBase);
            }
            else
            {
                MessageBox.Show("Kindly Select a Plan type");
            }       
        }

        public void ExecuteExportButton(object parameter)
        {
            ExportWindow export = new ExportWindow();
            export.ShowDialog();
        }

        public void extractOnTrackSignals(EulynxDataPrepInterface EulynxObject,Database dataBase)
        {
            var dataPrepEntities = EulynxObject.hasDataContainer.First().ownsDataPrepEntities;
            List<Unit> units = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesUnit;
            List<Models.TopoModels.EULYNX.rsmSig.OnTrackSignallingDevice> onTrackSignals = EulynxObject.hasDataContainer.First().ownsRsmEntities.ownsOnTrackSignallingDevice;
            List<BaseLocation> locations = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesLocation;
            List<PositioningSystemCoordinate> PSCoordinates = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesPositioningSystemCoordinate;
            List<IntrinsicCoordinate> intrCoordinates = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesIntrinsicCoordinate;
            List<PositioningNetElement> netElements = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTrackTopology.usesNetElement;
            foreach (Models.TopoModels.EULYNX.rsmSig.OnTrackSignallingDevice signal in onTrackSignals)
            {
                //dp function
                var euBalise = dataPrepEntities.ownsTrackAsset.First(x => x is EtcsBalise && ((EtcsBalise)x).refersToRsmTpsDevice.@ref == signal.id) as EtcsBalise;
                var euBaliseGroupId = euBalise.implementsTpsDataTxSystem.@ref;
                var euBaliseProperty = dataPrepEntities.ownsTpsDataTransmissionSystemProperties.First(x => x is EtcsBaliseGroupLevel2 && ((EtcsBaliseGroupLevel2)x).appliesToTpsDataTxSystem.@ref == euBaliseGroupId) as EtcsBaliseGroupLevel2;
                var euBaliseFunction = euBaliseProperty.implementsFunction.First().type;


                // get axle counter's location and print it
                var spotLoc = locations.Find(item => item.id == signal.locations.First().@ref) as SpotLocation; // dereference
                var tpsDeviceIntrinsicCoordinateRef = spotLoc.associatedNetElements.First().bounds.First();
                var tpsDeviceIntrinsicCoordinate = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesIntrinsicCoordinate.First(item => item.id == tpsDeviceIntrinsicCoordinateRef.@ref);
                var tpsDeviceCoordinate = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesPositioningSystemCoordinate.First(item => item.id == tpsDeviceIntrinsicCoordinate.coordinates.First().@ref) as LinearCoordinate;
                var tpsDeviceKmValue = tpsDeviceCoordinate.measure.value.Value;


                AssociatedNetElement assElement = spotLoc.associatedNetElements[0];
                tElementWithIDref bound = assElement.bounds[0];
                IntrinsicCoordinate intcoord = intrCoordinates.Find(x => x.id.Equals(bound.@ref));
                LinearCoordinate SignalIntrensic = PSCoordinates.Find(x => x.id.Equals(intcoord.coordinates[0].@ref)) as LinearCoordinate;



                //get relatedNetElement
                var element = spotLoc.associatedNetElements[0].netElement;
                PositioningNetElement netElement = netElements.Find(x => x.id.Equals(element.@ref));
                aplan.database.NetElement dataBaseElement = null;
                using (var liteDB = dataBase.accessDB())
                {
                    dataBaseElement = (liteDB).GetCollection<aplan.database.NetElement>("NetElements").Find(x => x.id == element.@ref).FirstOrDefault();
                }
                netElement = netElements.Find(x => x.id.Equals(dataBaseElement.uuid));

                // if the km value is not between the the NetElement range.
                if ((double)intcoord.value > 1)
                {
                    getNeighbors(netElement, EulynxObject, SignalIntrensic);
                    if (neededElement != null)
                    {
                        netElement = neededElement;
                    }
                }

                Unit elementLenthunit = units.Find(x => x.id.Equals(((LinearElementWithLength)netElement).elementLength.quantiy[0].unit.@ref));
                List<IntrinsicCoordinate> elementIntrensics = netElement.associatedPositioning[0].intrinsicCoordinates;
                LinearCoordinate elementStartKm = PSCoordinates.Find(x => x.id.Equals(elementIntrensics[0].coordinates[0].@ref)) as LinearCoordinate;
                LinearCoordinate elementEndKm = PSCoordinates.Find(x => x.id.Equals(elementIntrensics[1].coordinates[0].@ref)) as LinearCoordinate;
                List<System.Windows.Point> points = getNetElementCartesianCoordinates(netElement, PSCoordinates);
                var newSignal = new Signalinfo()
                {
                    SignalImageSource = new BitmapImage(new Uri(@"/Resources/SymbolsImages/BalisengruppeGesteuertTri.png", UriKind.RelativeOrAbsolute)),
                    LongName = signal.longname,
                    Name = signal.name,
                    Function = euBaliseFunction,
                    IntrinsicValue = tpsDeviceKmValue,
                    AttachedToElementname = netElement.name,
                    Side = assElement.isLocatedToSide.ToString(),
                    Direction = assElement.appliesInDirection.ToString(),
                    Coordinates = points,
                    AttachedToElementLength = ((Length)((LinearElementWithLength)netElement).elementLength.quantiy[0]).value,
                    LateralDistance = 3.1
                    
                };
                calculateSignalLocation(newSignal, SignalIntrensic, elementStartKm, elementEndKm);
                Signals.Add(newSignal);
            }
        }

        public void extractMainSignals(EulynxDataPrepInterface EulynxObject)
        {
            List<Unit> units = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesUnit;
            List<Models.TopoModels.EULYNX.sig.SignalFrame> signalFrames = EulynxObject.hasDataContainer.First().ownsDataPrepEntities.ownsSignalFrame;
            List<PositioningSystemCoordinate> PSCoordinates = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesPositioningSystemCoordinate;
            List<PositioningNetElement> netElements = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTrackTopology.usesNetElement;
            List<BaseLocation> locations = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesLocation;
            List<IntrinsicCoordinate> intrCoordinates = EulynxObject.hasDataContainer.First().ownsRsmEntities.usesTopography.usesIntrinsicCoordinate;
            List<Models.TopoModels.EULYNX.rsmSig.Signal> mainSignals = EulynxObject.hasDataContainer.First().ownsRsmEntities.ownsSignal;

            foreach (Models.TopoModels.EULYNX.rsmSig.Signal signal in mainSignals)
            {

                tElementWithIDref loc = signal.locations[0];
                SpotLocation spotLoc = (SpotLocation)locations.Find(x => x.id.Equals(loc.@ref));
                AssociatedNetElement assElement = spotLoc.associatedNetElements[0];
                tElementWithIDref bound = assElement.bounds[0];
                IntrinsicCoordinate intcoord = intrCoordinates.Find(x => x.id.Equals(bound.@ref));
                LinearCoordinate SignalIntrensic = PSCoordinates.Find(x => x.id.Equals(intcoord.coordinates[0].@ref)) as LinearCoordinate;
                PositioningNetElement element = netElements.Find(x => x.id.Equals(assElement.netElement.@ref));
                if ((double)intcoord.value>1)
                {
                    getNeighbors(element, EulynxObject, SignalIntrensic);
                    if (neededElement != null)
                    {
                        element = neededElement;
                    } 
                }
                
                Unit elementLenthunit = units.Find(x => x.id.Equals(((LinearElementWithLength)element).elementLength.quantiy[0].unit.@ref));
                List<IntrinsicCoordinate> elementIntrensics = element.associatedPositioning[0].intrinsicCoordinates;
                LinearCoordinate elementStartKm = PSCoordinates.Find(x => x.id.Equals(elementIntrensics[0].coordinates[0].@ref)) as LinearCoordinate;
                LinearCoordinate elementEndKm = PSCoordinates.Find(x => x.id.Equals(elementIntrensics[1].coordinates[0].@ref)) as LinearCoordinate;
                List<System.Windows.Point> points = getNetElementCartesianCoordinates(element, PSCoordinates);


                //signal type.
                var typeFunction = extractSignalTypeAndFunction(signal, EulynxObject);

                var newSignal = new Signalinfo()
                {
                    SignalImageSource = new BitmapImage(new Uri(@"/Resources/SymbolsImages/mehraZugoderStellwerksbedient.png", UriKind.RelativeOrAbsolute)),
                    LongName = signal.longname,
                    Name = signal.name,
                    Type= typeFunction[0],
                    Function = typeFunction[1],
                    AttachedToElementname = element.name,
                    IntrinsicValue = (double)SignalIntrensic.measure.value,
                    Side = assElement.isLocatedToSide.ToString(),
                    Direction = assElement.appliesInDirection.ToString(),
                    Coordinates = points,
                    AttachedToElementLength = ((Length)((LinearElementWithLength)element).elementLength.quantiy[0]).value,
                    LateralDistance = 3.1
        };
                calculateSignalLocation(newSignal, SignalIntrensic, elementStartKm, elementEndKm);
                Signals.Add(newSignal);
                //extract lateral distance
                //foreach (SignalFrame s in signalFrames)
                //{

                //}
            }

        }

        public void getNeighbors(PositioningNetElement element, EulynxDataPrepInterface EulynxObject, LinearCoordinate SignalIntrensic)
        {
            VisitedElements.Add(element);
            PositioningNetElement hostElement =null;
            List<PositionedRelation> relations=  EulynxObject.hasDataContainer[0].ownsRsmEntities.usesTrackTopology.usesPositionedRelation;
            List<PositionedRelation> targetRelations= relations.FindAll(x => ((x.elementA.@ref== element.id) || (x.elementB.@ref == element.id)));
            List<PositioningNetElement> relatedElements = new List<PositioningNetElement>();
            foreach (PositionedRelation relation in targetRelations)
            {
                PositioningNetElement elementA = EulynxObject.hasDataContainer[0].ownsRsmEntities.usesTrackTopology.usesNetElement.Find(x=>x.id.Equals(relation.elementA.@ref));
                PositioningNetElement elementB = EulynxObject.hasDataContainer[0].ownsRsmEntities.usesTrackTopology.usesNetElement.Find(x => x.id.Equals(relation.elementB.@ref));

                if (!relatedElements.Contains(elementA) && !VisitedElements.Contains(elementA))
                {
                    relatedElements.Add(elementA);
                }
                if (!relatedElements.Contains(elementB) && !VisitedElements.Contains(elementB))
                {
                    relatedElements.Add(elementB);
                }
              // VisitedElements.Add(elementA);
              
            }
            for ( int i=0; i< relatedElements.Count; i++)
            {
                List<IntrinsicCoordinate> intCoordiante= relatedElements[i].associatedPositioning[0].intrinsicCoordinates;
                LinearCoordinate startKm =(LinearCoordinate) EulynxObject.hasDataContainer[0].ownsRsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(pos => pos.id.Equals(intCoordiante[0].coordinates[0].@ref));
                LinearCoordinate endKm = (LinearCoordinate)EulynxObject.hasDataContainer[0].ownsRsmEntities.usesTopography.usesPositioningSystemCoordinate.Find(pos => pos.id.Equals(intCoordiante[1].coordinates[0].@ref));
                if (startKm.measure.value!=null)
                {
                    if (startKm.measure.value<= SignalIntrensic.measure.value && endKm.measure.value >= SignalIntrensic.measure.value)
                    {
                        neededElement = relatedElements[i];
                        hostElement= relatedElements[i];
                        break;
                        //return hostElement;
                    }
                }
                else
                {
                    relatedElements.Remove(relatedElements[i]);
                    i--;
                }
            }

            if (hostElement==null)
            {
                foreach (PositioningNetElement elem in relatedElements)

                {
                    getNeighbors(elem, EulynxObject, SignalIntrensic);
                }
            }
        }
        public List<System.Windows.Point> getNetElementCartesianCoordinates(PositioningNetElement positioningNetElements,List<PositioningSystemCoordinate> PSCoordinates)
        {
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            AssociatedPositioning associatedPositionings = positioningNetElements.associatedPositioning[1];
            List<IntrinsicCoordinate> intrinsicCoordinates = associatedPositionings.intrinsicCoordinates;
            foreach (IntrinsicCoordinate intrinsicCoordinate in intrinsicCoordinates)
            {
                List<tElementWithIDref> tElementWithIDrefs = intrinsicCoordinate.coordinates;
                foreach (tElementWithIDref tElementWithIDref in tElementWithIDrefs)
                {
                    CartesianCoordinate cartCoordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(tElementWithIDref.@ref));
                    System.Windows.Point newPoint = new System.Windows.Point((((double)cartCoordinate.x)), ((double)cartCoordinate.y));
                    points.Add(newPoint);
                }
            }
            return points;
        }
        public void calculateSignalLocation(Signalinfo signal, LinearCoordinate signalIntrensic, LinearCoordinate elementStart, LinearCoordinate elementEnd)
        {

            double? targetValue = (signalIntrensic.measure.value - elementStart.measure.value)*1000 / signal.AttachedToElementLength;
       
            double? targetLengthLocation = targetValue * signal.AttachedToElementLength;
            
            double currentLength = 0;
            for (int i=0; i<signal.Coordinates.Count-1; i++)
            {
                currentLength += Math.Sqrt(Math.Pow((signal.Coordinates[i].X - signal.Coordinates[i + 1].X), 2.0) + Math.Pow((signal.Coordinates[i].Y - signal.Coordinates[i + 1].Y),2.0));
                if (currentLength>targetLengthLocation)
                {
                    double? factor = targetLengthLocation / currentLength;
                    double? xdiff = signal.Coordinates[i+1].X - signal.Coordinates[i].X;
                    double? ydiff = signal.Coordinates[i+1].Y - signal.Coordinates[i].Y;
                    applyRotation(signal, xdiff, ydiff);
                    signal.LocationCoordinate = new System.Windows.Point((double)(signal.Coordinates[i].X+ xdiff* factor), (double)(signal.Coordinates[i].Y +ydiff * factor));
                    applyHorizontalOffset(signal, signal.Coordinates[i + 1]);
                    applyDirection(signal);
                    break;
                }
            }
            if (signal.LocationCoordinate.X == 0)
            {
                signal.LocationCoordinate = new System.Windows.Point((double)(signal.Coordinates[signal.Coordinates.Count - 1].X) - DrawViewModel.GlobalDrawingPoint.X, (double)(signal.Coordinates[signal.Coordinates.Count - 1].Y) - DrawViewModel.GlobalDrawingPoint.Y);
            }
        }
        public void applyRotation(Signalinfo signal, double? xdiff, double? ydiff)
        {
            //negative sign for drawing
            if (ydiff>=0 && xdiff>=0)
            {
                signal.Orientation = -Math.Atan((double)(ydiff / xdiff)) * (180 / Math.PI);
            }else if (ydiff < 0 && xdiff < 0)
            {
                signal.Orientation = -Math.Atan((double)(ydiff / xdiff)) * (180 / Math.PI)-180;
            }
            else if (ydiff > 0 && xdiff < 0)
            {
                signal.Orientation = -Math.Atan((double)(ydiff / xdiff)) * (180 / Math.PI) - 90;
            }
            else if (ydiff < 0 && xdiff > 0)
            {
                signal.Orientation = -Math.Atan((double)(ydiff / xdiff)) * (180 / Math.PI) - 360;
            }

        }
        public void applyHorizontalOffset(Signalinfo signal, System.Windows.Point backward)
        {
            var x = signal.LocationCoordinate.X;
            var y = signal.LocationCoordinate.Y;
            var shift = 10;

            var shiftx = 0.0;
            var shifty = 0.0;
            if (signal.Side == "right")
            {
                if (Math.Abs(signal.Orientation) <= 90)
                {
                    shiftx = shift * Math.Sin(Math.Abs(signal.Orientation) * (Math.PI) / 180);
                    shifty = -shift * Math.Cos(Math.Abs(signal.Orientation) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 180 && Math.Abs(signal.Orientation) > 90)
                {
                    shiftx = shift * Math.Cos((Math.Abs(signal.Orientation) - 90) * (Math.PI) / 180);
                    shifty = shift * Math.Sin((Math.Abs(signal.Orientation) - 90) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 270 && Math.Abs(signal.Orientation) > 180)
                {

                    shiftx = -shift * Math.Sin((Math.Abs(signal.Orientation) - 180) * (Math.PI) / 180);
                    shifty = shift * Math.Cos((Math.Abs(signal.Orientation) - 180) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 360 && Math.Abs(signal.Orientation) > 270)
                {
                    shiftx = -shift * Math.Cos((Math.Abs(signal.Orientation) - 270) * (Math.PI) / 180);
                    shifty = -shift * Math.Sin((Math.Abs(signal.Orientation) - 270) * (Math.PI) / 180);
                }

                }
                else if (signal.Side == "left")
                {
                if (Math.Abs(signal.Orientation) <= 90)
                {
                    shiftx = -shift * Math.Sin(Math.Abs(signal.Orientation) * (Math.PI) / 180);
                    shifty = shift * Math.Cos(Math.Abs(signal.Orientation) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 180 && Math.Abs(signal.Orientation) > 90)
                {
                    shiftx = -shift * Math.Cos((Math.Abs(signal.Orientation) - 90) * (Math.PI) / 180);
                    shifty = -shift * Math.Sin((Math.Abs(signal.Orientation) - 90) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 270 && Math.Abs(signal.Orientation) > 180)
                {

                    shiftx = shift * Math.Sin((Math.Abs(signal.Orientation) - 180) * (Math.PI) / 180);
                    shifty = -shift * Math.Cos((Math.Abs(signal.Orientation) - 180) * (Math.PI) / 180);
                }
                else if (Math.Abs(signal.Orientation) <= 360 && Math.Abs(signal.Orientation) > 270)
                {
                    shiftx = shift * Math.Cos((Math.Abs(signal.Orientation) - 270) * (Math.PI) / 180);
                    shifty = shift * Math.Sin((Math.Abs(signal.Orientation) - 270) * (Math.PI) / 180);
                }

            }
            x += shiftx;
            y += shifty;

            signal.LocationCoordinate = new System.Windows.Point(x, y);
        }
        public void applyDirection(Signalinfo signal)
        {
            if (signal.Direction.Equals("2") || signal.Direction.Equals("reverse"))
            {
                signal.Orientation = signal.Orientation-180;
            }
        }
        public string [] extractSignalTypeAndFunction(Models.TopoModels.EULYNX.rsmSig.Signal signal , EulynxDataPrepInterface EulynxObject)
        {
            string type = "notFound" ;
            string function= "notFound";

            var ownsSignal = EulynxObject.hasDataContainer[0].ownsRsmEntities.ownsSignal;
            var ownsSignalType = EulynxObject.hasDataContainer[0].ownsDataPrepEntities.ownsSignalType;
            var ownsSignalFunction = EulynxObject.hasDataContainer[0].ownsDataPrepEntities.ownsSignalFunction;
            var ownsTrackAsset = EulynxObject.hasDataContainer[0].ownsDataPrepEntities.ownsTrackAsset;

            LightSignalTyped trackAsset=null;
            foreach (Models.TopoModels.EULYNX.db.SignalType signalType in ownsSignalType)
            {
                trackAsset = ownsTrackAsset.Find(x => x.id.Equals(signalType.appliesToSignal.@ref)) as LightSignalTyped;
                Models.TopoModels.EULYNX.rsmSig.Signal current_signal = ownsSignal.Find(x => x.id.Equals(trackAsset.refersToRsmSignal.@ref));
                if (current_signal.id == signal.id)
                {
                    foreach (Models.TopoModels.EULYNX.db.SignalFunction signalFunction in ownsSignalFunction)
                    {
                        if (signalFunction.appliesToSignal.@ref.Equals(signalType.appliesToSignal.@ref))
                        {
                            function = signalFunction.isOfSignalFunctionType?.ToString();
                        }
                    }
                    type = signalType.isOfSignalTypeType?.ToString();
                }
            }

            return  new string[2] { type, function };
        }
        #endregion
    }
}
