using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Shapes;
using aplan.core;
using aplan.eulynx;
using Models.TopoModels.EULYNX.generic;
using Models.TopoModels.EULYNX.rsmCommon;
using APLan.HelperClasses;
using System.Collections;
using System.Windows.Media;

namespace APLan.ViewModels
{
    public class ModelViewModel
    {

        #region attributes
        bool start = false;
        public static int routeNumber;
        public static ArrayList boundPoints;
        public static Point firspoint = new Point(0, 0);
        public static Database db { get; set; }
        private RsmEntities RsmEntities { get; set; }
        private List<DataContainer> DataContainer { get; set; }
        public static EulynxService eulynxService { get; set; }
        public static EulynxDataPrepInterface eulynx { get; set; }
        private List<IntrinsicCoordinate> IntrensicCoordinates { get; set; }
        private List<PositioningNetElement> PositioningNetElements { get; set; }
        private List<PositioningSystemCoordinate> PositioningSystemCoordinates { get; set; }
        public ObservableCollection<Polyline> lines = new ObservableCollection<Polyline>();
        #endregion

        #region constructors
        public ModelViewModel()
        {
            assignInitialBoundary();
        }
        public ModelViewModel(string country, string format,
                string mileageFilePath, string edgesFilePath, string nodesFilePath,
                string horizontalAlignmentsFilePath, string verticalAlignmentsFilePath, string cantAlingnmentsFilePath,
                string mdbFilePath)
        {
            assignInitialBoundary();

            generateAEuLynxObject(country, format,
                mileageFilePath,  edgesFilePath,  nodesFilePath,
                 horizontalAlignmentsFilePath, verticalAlignmentsFilePath, cantAlingnmentsFilePath,
                 mdbFilePath);
           
        }
        #endregion
        
        /// <summary>
        /// assign initial infinity boundary at beginning
        /// </summary>
        public void assignInitialBoundary()
        {
            boundPoints = new ArrayList();
            boundPoints.Add(double.PositiveInfinity);
            boundPoints.Add(double.NegativeInfinity);
            boundPoints.Add(double.PositiveInfinity);
            boundPoints.Add(double.NegativeInfinity);
        }
        
        /// <summary>
        /// generate the Eulynx Object
        /// </summary>
        /// <param name="country"></param>
        /// <param name="format"></param>
        /// <param name="mileageFilePath"></param>
        /// <param name="edgesFilePath"></param>
        /// <param name="nodesFilePath"></param>
        /// <param name="horizontalAlignmentsFilePath"></param>
        /// <param name="verticalAlignmentsFilePath"></param>
        /// <param name="cantAlingnmentsFilePath"></param>
        /// <param name="mdbFilePath"></param>
        public void generateAEuLynxObject(string country,string format,
                string mileageFilePath, string edgesFilePath, string nodesFilePath,
                string horizontalAlignmentsFilePath, string verticalAlignmentsFilePath, string cantAlingnmentsFilePath,
                string mdbFilePath)
        {
            //routeNumber = 6624; // this is just example, in the end this should be generic
            Globals.routeNumber = 6624;
            eulynxService = EulynxService.getInstance();
            db = Database.getInstance();
      
            eulynxService.inputHandling(
                format, db,
                mileageFilePath, edgesFilePath, nodesFilePath,
                horizontalAlignmentsFilePath, verticalAlignmentsFilePath, cantAlingnmentsFilePath,
                mdbFilePath);

            // eulynx object creation
            eulynx = (EulynxDataPrepInterface)eulynxService.objectsCreation(country, db);            
        }
        
        /// <summary>
        /// draw the Eulynx Object
        /// </summary>
        /// <param name="canvasSize"></param>
        /// <param name="gleiskantenList"></param>
        /// <param name="gleiskantenPointsList"></param>
        /// <param name="Entwurfselement_LA_list"></param>
        /// <param name="Entwurfselement_LAPointsList"></param>
        /// <param name="Entwurfselement_KM_list"></param>
        /// <param name="Entwurfselement_KMPointsList"></param>
        /// <param name="Entwurfselement_HO_list"></param>
        /// <param name="Entwurfselement_HOList"></param>
        /// <param name="Entwurfselement_UH_list"></param>
        /// <param name="Entwurfselement_UHPointsList"></param>
        /// <param name="gleisknotenList"></param>
        public void drawObject(double canvasSize, 
            ObservableCollection<CustomPolyLine> gleiskantenList,
            ObservableCollection<Point> gleiskantenPointsList,
            ObservableCollection<CustomPolyLine> Entwurfselement_LA_list,
            ObservableCollection<Point> Entwurfselement_LAPointsList,
            ObservableCollection<CustomPolyLine> Entwurfselement_KM_list,
            ObservableCollection<Point> Entwurfselement_KMPointsList,
            ObservableCollection<CustomPolyLine> Entwurfselement_HO_list,
            ObservableCollection<Point> Entwurfselement_HOList,
            ObservableCollection<CustomPolyLine> Entwurfselement_UH_list,
            ObservableCollection<Point> Entwurfselement_UHPointsList,
            ObservableCollection<CustomNode> gleisknotenList)
        {
            DataContainer = eulynx.hasDataContainer;
            RsmEntities = DataContainer[0].ownsRsmEntities;
            PositioningNetElements = RsmEntities.usesTrackTopology.usesNetElement;

            PositioningSystemCoordinates = RsmEntities.usesTopography.usesPositioningSystemCoordinate;
            IntrensicCoordinates = RsmEntities.usesTopography.usesIntrinsicCoordinate;
            List<Unit> units = RsmEntities.usesUnit;
            //MessageBox.Show("DrawStarted");

            DrawNetElement(PositioningNetElements, PositioningSystemCoordinates, canvasSize, gleiskantenList, units);
            DrawVerticalAlignment(RsmEntities, PositioningSystemCoordinates, canvasSize, Entwurfselement_HO_list, units);
            DrawHorizontalAlignment(RsmEntities, PositioningSystemCoordinates, canvasSize, Entwurfselement_LA_list, units);
            DrawMielage(RsmEntities, PositioningSystemCoordinates, canvasSize, Entwurfselement_KM_list, units);
            DrawCantlAlignment(RsmEntities, PositioningSystemCoordinates, canvasSize, Entwurfselement_UH_list, units);
            DrawNodes(RsmEntities, PositioningSystemCoordinates, IntrensicCoordinates, canvasSize, gleisknotenList, units);
            
            calculatePointsScaling(); //this should be always called before the Nodes due to templating in the XAML
            
            
            
            //points extraction after scaling
            ExtractPoints(gleiskantenList, gleiskantenPointsList);
            ExtractPoints(Entwurfselement_LA_list, Entwurfselement_LAPointsList);
            ExtractPoints(Entwurfselement_KM_list, Entwurfselement_KMPointsList);
            ExtractPoints(Entwurfselement_HO_list, Entwurfselement_HOList);
            ExtractPoints(Entwurfselement_UH_list, Entwurfselement_UHPointsList);


            System.Windows.Controls.Canvas.SetLeft(Views.Draw.drawing, -DrawViewModel.sharedCanvasSize / 2);
            System.Windows.Controls.Canvas.SetTop(Views.Draw.drawing, -DrawViewModel.sharedCanvasSize / 2);

        }
        
        /// <summary>
        /// draw the NetElements in the EulynxObject
        /// </summary>
        /// <param name="positioningNetElements"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="customPolylines"></param>
        /// <param name="units"></param>
        public void DrawNetElement (List<PositioningNetElement> positioningNetElements, 
            List<PositioningSystemCoordinate> PSCoordinates,
            double canvasSize,
            ObservableCollection<CustomPolyLine> customPolylines,
            List<Unit> units)
        {

            List<PositioningSystemCoordinate> positioningSystemCoordinates = new List<PositioningSystemCoordinate>();
            PositioningSystemCoordinate positioningSystemCoordinate = new PositioningSystemCoordinate();
            List<Polyline> p = new List<Polyline>();


            
            foreach (PositioningNetElement positioningNetElement in positioningNetElements)
            {     
                CustomPolyLine polyline = new CustomPolyLine();
                polyline.Name = positioningNetElement.name;
                polyline.Data.Add(new KeyValue() { Key="Name", Value= positioningNetElement.name});
                polyline.Data.Add(new KeyValue() { Key = "Length", Value =
                    ((Length)((LinearElementWithLength)positioningNetElement).elementLength.quantiy[0]).value
                    .ToString() + " " +
                    units.Find(x => x.id.Equals(((LinearElementWithLength)positioningNetElement).elementLength.quantiy[0].unit.@ref)).name
                });
                
                List<AssociatedPositioning> associatedPositionings = positioningNetElement.associatedPositioning;
                foreach (AssociatedPositioning associatedPositioning in associatedPositionings)
                {
                    List<IntrinsicCoordinate> intrinsicCoordinates = associatedPositioning.intrinsicCoordinates;
                    foreach (IntrinsicCoordinate intrinsicCoordinate in intrinsicCoordinates)
                    {
                        List<tElementWithIDref> tElementWithIDrefs = intrinsicCoordinate.coordinates;
                        foreach (tElementWithIDref tElementWithIDref in tElementWithIDrefs)
                        {
                            positioningSystemCoordinate = PSCoordinates.Find(x => x.id.Equals(tElementWithIDref.@ref));
                            
                            if (positioningSystemCoordinate is CartesianCoordinate)
                            {
                                
                                CartesianCoordinate cartesianCoordinate = positioningSystemCoordinate as CartesianCoordinate;
                                if (firspoint.X==0)
                                {
                                    
                                    firspoint.X = (((double)cartesianCoordinate.x));
                                    firspoint.Y = (((double)cartesianCoordinate.y));
                                    ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                                }
                                polyline.GlobalPoint = new Point(firspoint.X, firspoint.Y);
                                Point newPoint = new Point((((double)cartesianCoordinate.x)), (((double)cartesianCoordinate.y)));
                                extractBoundary(newPoint);
                                polyline.Points.Add(newPoint);
                            }else if (positioningSystemCoordinate is LinearCoordinate)
                            {
                                extractStartEnd((LinearCoordinate)positioningSystemCoordinate,polyline,units, PSCoordinates);
                            }
                        }
                    }
                }
                polyline.Color = new SolidColorBrush() { Color = Colors.DarkOrange };
                customPolylines.Add(polyline);
            }

        }

        /// <summary>
        /// draw the VerticalAlignment in the EulynxObject
        /// </summary>
        /// <param name="ownsRsmEntity"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="customPolylines"></param>
        /// <param name="units"></param>
        public void DrawVerticalAlignment(RsmEntities ownsRsmEntity,
            List<PositioningSystemCoordinate> PSCoordinates,
            double canvasSize,
            ObservableCollection<CustomPolyLine> customPolylines,
            List<Unit> units)
        {
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmCommon.VerticalAlignmentSegment>
                usesVerticalAlignemntSegment = ownsRsmEntity.usesTopography.usesVerticalAlignmentSegment;

            foreach (Models.TopoModels.EULYNX.rsmCommon.VerticalAlignmentSegment segment in usesVerticalAlignemntSegment )
            {
                CustomPolyLine polyline = new CustomPolyLine();
                verticalElementExtractProperties(segment, polyline,ownsRsmEntity);
                polyline.Name = null;
                PolyLine poly = segment.hasLinearLocation.polyLines[1];
                foreach (tElementWithIDref element in poly.coordinates)
                {
                    CartesianCoordinate coordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(element.@ref)); 
                    if (firspoint.X == 0)
                    {

                        firspoint.X = (((double)coordinate.x));
                        firspoint.Y = (((double)coordinate.y));
                        ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                    }
                    polyline.GlobalPoint = new Point(firspoint.X, firspoint.Y);
                    Point newPoint = new Point((((double)coordinate.x) ), (((double)coordinate.y) ));
                    polyline.Points.Add(newPoint);
                    extractBoundary(newPoint);
                }
                polyline.Color = new SolidColorBrush() { Color = Colors.DarkBlue };
                customPolylines.Add(polyline);
            }

        }

        /// <summary>
        /// draw the HorizontalAlignment in the EulynxObject
        /// </summary>
        /// <param name="ownsRsmEntity"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="customPolylines"></param>
        /// <param name="units"></param>
        public void DrawHorizontalAlignment(RsmEntities ownsRsmEntity,
           List<PositioningSystemCoordinate> PSCoordinates,
           double canvasSize,
           ObservableCollection<CustomPolyLine> customPolylines,
           List<Unit> units)
        {
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment>
                usesHorizontalAlignemntSegment = ownsRsmEntity.usesTopography.usesHorizontalAlignmentSegment;

            foreach (Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment segment in usesHorizontalAlignemntSegment)
            {
                if (segment.initialAzimuth!=null)
                {
                    CustomPolyLine polyline = new CustomPolyLine();
                    HorizontalElementExtractProperties(segment, polyline, ownsRsmEntity);
                    polyline.Name = null;
                    PolyLine poly = segment.hasLinearLocation.polyLines[1];
                    foreach (tElementWithIDref element in poly.coordinates)
                    {
                        CartesianCoordinate coordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(element.@ref));
                        //polyline.Points.Add(new Point((double)coordinate.x, (double)coordinate.y));
                        if (firspoint.X == 0)
                        {

                            firspoint.X = (((double)coordinate.x));
                            firspoint.Y = (((double)coordinate.y));
                            ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                        }
                        polyline.GlobalPoint = new Point(firspoint.X, firspoint.Y);
                        Point newPoint = new Point((((double)coordinate.x) ), (((double)coordinate.y) ));
                        polyline.Points.Add(newPoint);
                        extractBoundary(newPoint);
                    }
                    polyline.Color = new SolidColorBrush() { Color = Colors.DarkRed };
                    customPolylines.Add(polyline);
                }
               
            }

        }

        /// <summary>
        /// draw the Mielage in the EulynxObject
        /// </summary>
        /// <param name="ownsRsmEntity"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="customPolylines"></param>
        /// <param name="units"></param>
        public void DrawMielage(RsmEntities ownsRsmEntity,
            List<PositioningSystemCoordinate> PSCoordinates,
            double canvasSize,
            ObservableCollection<CustomPolyLine> customPolylines,
            List<Unit> units)
        {
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment>
                usesHorizontalAlignemntSegment = ownsRsmEntity.usesTopography.usesHorizontalAlignmentSegment;

            foreach (Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment segment in usesHorizontalAlignemntSegment)
            {
                if (segment.initialAzimuth == null)
                {
                    CustomPolyLine polyline = new CustomPolyLine();
                    HorizontalElementExtractProperties(segment, polyline, ownsRsmEntity);
                    polyline.Name = null;
                    //PolyLine polyIntrensic = segment.hasLinearLocation.polyLines[0];
                    PolyLine poly = segment.hasLinearLocation.polyLines[1];
                    foreach (tElementWithIDref element in poly.coordinates)
                    {
                        CartesianCoordinate coordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(element.@ref));
                        if (firspoint.X == 0)
                        {

                            firspoint.X = (((double)coordinate.x));
                            firspoint.Y = (((double)coordinate.y));
                            ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                        }
                        polyline.GlobalPoint = new Point(firspoint.X, firspoint.Y);
                        Point newPoint = new Point((((double)coordinate.x) ), (((double)coordinate.y) ));
                        polyline.Points.Add(newPoint);
                        extractBoundary(newPoint);
                    }
                    polyline.Color = new SolidColorBrush() { Color= Colors.DarkViolet};
                    customPolylines.Add(polyline);
                }

            }

        }
        /// <summary>
        /// draw the CantAlignment in the EulynxObject
        /// </summary>
        /// <param name="ownsRsmEntity"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="customPolylines"></param>
        /// <param name="units"></param>
        public void DrawCantlAlignment(RsmEntities ownsRsmEntity,
           List<PositioningSystemCoordinate> PSCoordinates,
           double canvasSize,
           ObservableCollection<CustomPolyLine> customPolylines,
           List<Unit> units)
        {
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmCommon.AlignmentCantSegment>
                usesHorizontalAlignemntSegment = ownsRsmEntity.usesTopography.usesAlignmentCantSegment;

            foreach (Models.TopoModels.EULYNX.rsmCommon.AlignmentCantSegment segment in usesHorizontalAlignemntSegment)
            {
                CustomPolyLine polyline = new CustomPolyLine();
                CantElementExtractProperties(segment, polyline, ownsRsmEntity);
                polyline.Name = null;
                PolyLine poly = segment.hasLinearLocation.polyLines[1];
                foreach (tElementWithIDref element in poly.coordinates)
                {
                    CartesianCoordinate coordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(element.@ref));
                    //polyline.Points.Add(new Point((double)coordinate.x, (double)coordinate.y));
                    if (firspoint.X == 0)
                    {

                        firspoint.X = (((double)coordinate.x));
                        firspoint.Y = (((double)coordinate.y));
                        ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                    }
                    polyline.GlobalPoint = new Point(firspoint.X, firspoint.Y);
                    Point newPoint = new Point((((double)coordinate.x)), (((double)coordinate.y)));
                    polyline.Points.Add(newPoint);
                    extractBoundary(newPoint);
                }
                polyline.Color = new SolidColorBrush() { Color = Colors.Green };
                customPolylines.Add(polyline);
            }

        }

        /// <summary>
        /// draw the Nodes in the EulynxObject
        /// </summary>
        /// <param name="ownsRsmEntity"></param>
        /// <param name="PSCoordinates"></param>
        /// <param name="IntrensicCoordinates"></param>
        /// <param name="canvasSize"></param>
        /// <param name="allNodes"></param>
        /// <param name="units"></param>
        /// <returns></returns>
        public List<CartesianCoordinate> DrawNodes(RsmEntities ownsRsmEntity,
            List<PositioningSystemCoordinate> PSCoordinates,
            List<IntrinsicCoordinate> IntrensicCoordinates,
            double canvasSize,
            ObservableCollection<CustomNode> allNodes,
            List<Unit> units)
        {
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmTrack.Turnout>
                Nodes = ownsRsmEntity.ownsPoint;
            System.Collections.Generic.List<Models.TopoModels.EULYNX.rsmTrack.VehicleStop>
                VehicleNodes = ownsRsmEntity.ownsVehicleStop;
            List<CartesianCoordinate> AllCoordinates = new List<CartesianCoordinate>();


            //turn outs.
            foreach (Models.TopoModels.EULYNX.rsmTrack.Turnout turn in Nodes)
            {
                
                Models.TopoModels.EULYNX.rsmCommon.tElementWithIDref loc = turn.locations[0];
                SpotLocation spot = (SpotLocation)ownsRsmEntity.usesLocation.Find(x => x.id.Equals(loc.@ref));
                List<AssociatedNetElement> AssElement = spot.associatedNetElements;

                APLan.HelperClasses.CustomNode node = new CustomNode();
                TurnOutElementExtractProperties(turn, node, ownsRsmEntity);
                node.Name = null;
                foreach (AssociatedNetElement ele in AssElement)
                {
                    IntrinsicCoordinate intCoordinate = (IntrinsicCoordinate)IntrensicCoordinates.Find(x => x.id.Equals(ele.bounds[0].@ref));
                    CartesianCoordinate cartCoordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(intCoordinate.coordinates[1].@ref));
                    if (!AllCoordinates.Contains(cartCoordinate))
                    {
                        AllCoordinates.Add(cartCoordinate);
                        if (firspoint.X == 0)
                        {
                            firspoint.X = (((double)cartCoordinate.x));
                            firspoint.Y = (((double)cartCoordinate.y));
                            ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                        }
                        Point newPoint = new Point((((double)cartCoordinate.x) ), (((double)cartCoordinate.y) ));
                        node.NodePoint = newPoint;
                        allNodes.Add(node);
                        extractBoundary(newPoint);
                    }

                }
            }
            //Vehicle Stops
            foreach (Models.TopoModels.EULYNX.rsmTrack.VehicleStop Vstop in VehicleNodes)
                {

                    Models.TopoModels.EULYNX.rsmCommon.tElementWithIDref locVstop = Vstop.locations[0];
                    SpotLocation spotVstop = (SpotLocation)ownsRsmEntity.usesLocation.Find(x => x.id.Equals(locVstop.@ref));
                    List<AssociatedNetElement> AssElementVstop = spotVstop.associatedNetElements;
                    APLan.HelperClasses.CustomNode node = new CustomNode();
                    VehicleStopElementExtractProperties(Vstop, node, ownsRsmEntity);
                    node.Name = null;
                    foreach (AssociatedNetElement ele in AssElementVstop)
                    {
                        IntrinsicCoordinate intCoordinate = (IntrinsicCoordinate)IntrensicCoordinates.Find(x => x.id.Equals(ele.bounds[0].@ref));
                        CartesianCoordinate cartCoordinate = (CartesianCoordinate)PSCoordinates.Find(x => x.id.Equals(intCoordinate.coordinates[1].@ref));
                        if (!AllCoordinates.Contains(cartCoordinate))
                        {
                            AllCoordinates.Add(cartCoordinate);
                            if (firspoint.X == 0)
                            {

                                firspoint.X = (((double)cartCoordinate.x));
                                firspoint.Y = (((double)cartCoordinate.y));
                                ViewModels.DrawViewModel.GlobalDrawingPoint = firspoint;
                            }
                            Point newPoint = new Point((((double)cartCoordinate.x) ), (((double)cartCoordinate.y) ));
                            node.NodePoint = newPoint;
                            allNodes.Add(node);
                            //extractBoundary(newPoint);
                        }

                    }
                    //polyline.Points.Add(new Point((double)coordinate.x, (double)coordinate.y));
                }
               
           
            return AllCoordinates;
        }

        /// <summary>
        /// extract the stard and end kilometer value of a polyline.
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="polyLine"></param>
        /// <param name="units"></param>
        /// <param name="PSCoordinates"></param>
        public void extractStartEnd( LinearCoordinate coordinate , CustomPolyLine polyLine,List<Unit> units, List<PositioningSystemCoordinate> PSCoordinates)
        {
            if (start == false)
            {
                polyLine.Data.Add(new KeyValue() { Key = "Start", Value = coordinate.measure.value?.ToString()+"  " + units.Find(x=> x.id.Equals( coordinate.measure.unit.@ref)).name});
                start = true;
            }
            else
            {
                polyLine.Data.Add(new KeyValue() { Key = "End", Value = coordinate.measure.value?.ToString() + "  " + units.Find(x => x.id.Equals(coordinate.measure.unit.@ref)).name });
                start = false;
            }
        }
        /// <summary>
        /// extract the boundary of the whole drawing according to the polylines
        /// </summary>
        /// <param name="point"></param>
        public void extractBoundary(Point point)
        {
            if (point.X < (double)boundPoints[0])
            {
                
                boundPoints[0] = point.X;
            }else
            if (point.X > (double)boundPoints[1])
            {
                boundPoints[1] = point.X;
            }
            if (point.Y < (double)boundPoints[2])
            {
                boundPoints[2] = point.Y;
            }else
            if (point.Y > (double)boundPoints[3])
            {
                boundPoints[3] = point.Y;
            } 
        }
        /// <summary>
        /// calculate how much the drawing should be scaled down or up to be proprotionally visible.
        /// </summary>
        public void calculatePointsScaling()
        {
            double scaleX = (double)boundPoints[1] - (double)boundPoints[0];
            double scaleY = (double)boundPoints[3] - (double)boundPoints[2];

            if (scaleX>scaleY)
            {
                ViewModels.DrawViewModel.drawingScale = (ViewModels.DrawViewModel.sharedCanvasSize / scaleX) * 0.1;
            }
            else
            {
                ViewModels.DrawViewModel.drawingScale = (ViewModels.DrawViewModel.sharedCanvasSize / scaleY) * 0.1;
            }
        }
        /// <summary>
        /// extract the points used to draw polylines.
        /// </summary>
        /// <param name="polylines"></param>
        /// <param name="pointsContainer"></param>
        public void ExtractPoints(ObservableCollection<CustomPolyLine> polylines, ObservableCollection<Point> pointsContainer)
        {
            foreach (CustomPolyLine line in polylines)
            {
                for (int i =0; i<line.Points.Count; i++)
                {
                    pointsContainer.Add(line.Points[i]);
                }
            }
        }
        
        //Extract properties of the polylines and nodes.
        public void verticalElementExtractProperties(VerticalAlignmentSegment element, CustomPolyLine polyline, RsmEntities ownsRsmEntity)
        {
            //double? initialSegmentValue = element.initialSegment.value;
            //string? initialSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.initialSegment.unit.@ref)).name;

            //double? endSegmentValue= element.endSegment.value;
            //string? endSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x=>x.id.Equals(element.endSegment.unit.@ref)).name;

            Gradient initialElevation = ownsRsmEntity.usesTopography.usesElevationAndInclination.Find(x => x.id.Equals(element.initialElevation.@ref)).inclination;

            var initialElevationValue = initialElevation.value;
            var initialElevationUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(initialElevation.unit.@ref)).name;

            var finalElevationValue = initialElevation.value;
            var finalElevationUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(initialElevation.unit.@ref)).name;

            var length = element.length.value;
            var lengthUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.length.unit.@ref)).name;

            var type = element.type;

            LinearCoordinate startKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
            x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[0].@ref)) as LinearCoordinate);

            LinearCoordinate endKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
             x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[1].@ref)) as LinearCoordinate);

            var startKMvalue = startKM.measure.value;
            var startKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(startKM.measure.unit.@ref)).name;

            var endKMvalue = endKM.measure.value;
            var endKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(endKM.measure.unit.@ref)).name;


            polyline.Data.Add(new KeyValue() {Key= "startKM", Value= startKMvalue.ToString()+" "+ startKMunit });
            polyline.Data.Add(new KeyValue() { Key = "endSegmentValue", Value = endKMvalue.ToString() + " " + endKMunit });
            polyline.Data.Add(new KeyValue() { Key = "initialElevationValue", Value = initialElevationValue.ToString() + " " + initialElevationUnit });
            polyline.Data.Add(new KeyValue() { Key = "finalElevationValue", Value = finalElevationValue.ToString() + " " + finalElevationUnit });
            polyline.Data.Add(new KeyValue() { Key = "length", Value = length.ToString() + " " + lengthUnit });
            //polyline.Data.Add(new KeyValue() { Key = "startKMvalue", Value = startKMvalue.ToString() + " " + startKMunit });
            //polyline.Data.Add(new KeyValue() { Key = "endKMvalue", Value = endKMvalue.ToString() + " " + endKMunit });
            polyline.Data.Add(new KeyValue() { Key = "type", Value = type});

        }
        public void HorizontalElementExtractProperties(Models.TopoModels.EULYNX.rsmCommon.HorizontalAlignmentSegment element, CustomPolyLine polyline, RsmEntities ownsRsmEntity)
        {
            //double? initialSegmentValue = element.initialSegment.value;
            //string? initialSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.initialSegment.unit.@ref)).name;

            //double? endSegmentValue = element.endSegment.value;
            //string? endSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.endSegment.unit.@ref)).name;

            var initialAzimuth = element.initialAzimuth?.value;
            var initialAzimuthUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.initialAzimuth?.unit.@ref))?.name;

            double? radius = null;
            string? radiusUnit = null;

            double? initialRadius=null;
            string? initialRadiusUnit = null;

            double? finalRadius = null;
            string? finalRadiusUnit = null;

            string? type=null;


            if (element is HorizontalSegmentArc)
            {
                radius = ((HorizontalSegmentArc)element).radius.value;
                radiusUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(((HorizontalSegmentArc)element).radius.unit.@ref)).name;
            }
            else if (element is HorizontalSegmentTransition)
            {

                initialRadius = ((HorizontalSegmentTransition)element).initialRadius.value;
                initialRadiusUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(((HorizontalSegmentTransition)element).initialRadius.unit.@ref)).name;

                finalRadius = ((HorizontalSegmentTransition)element).finalRadius.value;
                finalRadiusUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(((HorizontalSegmentTransition)element).finalRadius.unit.@ref)).name;

                type = ((HorizontalSegmentTransition)element).type;
            }

            var length = element.length.value;
            var lengthUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.length.unit.@ref)).name;


            LinearCoordinate startKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
            x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[0].@ref)) as LinearCoordinate);

            LinearCoordinate endKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
             x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[1].@ref)) as LinearCoordinate);

            var startKMvalue = startKM.measure.value;
            var startKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(startKM.measure.unit.@ref)).name;

            var endKMvalue = endKM.measure.value;
            var endKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(endKM.measure.unit.@ref)).name;

            if (startKMvalue != null)
            {
                polyline.Data.Add(new KeyValue() { Key = "initialSegmentValue", Value = startKMvalue.ToString() + " " + startKMunit });
                polyline.Data.Add(new KeyValue() { Key = "endSegmentValue", Value = endKMvalue.ToString() + " " + endKMunit });
            }
            if (initialAzimuth!=null)
            {
                polyline.Data.Add(new KeyValue() { Key = "initialAzimuth", Value = initialAzimuth.ToString() + " " + initialAzimuthUnit });
            }
            if (initialRadius!=null)
            {
                polyline.Data.Add(new KeyValue() { Key = "initialRadius", Value = initialRadius.ToString() + " " + initialRadiusUnit });
            }
            if (finalRadius!=null)
            {
                polyline.Data.Add(new KeyValue() { Key = "finalRadius", Value = finalRadius.ToString() + " " + finalRadiusUnit });
            }
            if (radius != null)
            {
                polyline.Data.Add(new KeyValue() { Key = "radius", Value = radius.ToString() + " " + radiusUnit });
            }
            if (type != null)
            {
                polyline.Data.Add(new KeyValue() { Key = "type", Value = type});
            }

            polyline.Data.Add(new KeyValue() { Key = "length", Value = length.ToString() + " " + lengthUnit });
            //polyline.Data.Add(new KeyValue() { Key = "startKMvalue", Value = startKMvalue.ToString() + " " + startKMunit });
            //polyline.Data.Add(new KeyValue() { Key = "endKMvalue", Value = endKMvalue.ToString() + " " + endKMunit });


        }
        public void CantElementExtractProperties(Models.TopoModels.EULYNX.rsmCommon.AlignmentCantSegment element, CustomPolyLine polyline, RsmEntities ownsRsmEntity)
        {
            //double? initialSegmentValue = element.initialSegment.value;
            //string? initialSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.initialSegment.unit.@ref)).name;

            //double? endSegmentValue = element.endSegment.value;
            //string? endSegmentValueUnit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(element.endSegment.unit.@ref)).name;

            string? transitionType = null;
            if (element is SegmentCantTransition)
            {
                transitionType = ((SegmentCantTransition)element).transitionShape.isOfTransitionType.Value.ToString();
            }
            LinearCoordinate startKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
            x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[0].@ref)) as LinearCoordinate);

            LinearCoordinate endKM = (ownsRsmEntity.usesTopography.usesPositioningSystemCoordinate.Find(x =>
             x.id.Equals(element.hasLinearLocation.polyLines[0].coordinates[1].@ref)) as LinearCoordinate);

            var startKMvalue = startKM.measure.value;
            var startKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(startKM.measure.unit.@ref)).name;

            var endKMvalue = endKM.measure.value;
            var endKMunit = ownsRsmEntity.usesUnit.Find(x => x.id.Equals(endKM.measure.unit.@ref)).name;
            if (startKMvalue != null)
            {
                polyline.Data.Add(new KeyValue() { Key = "initialSegmentValue", Value = startKMvalue.ToString() + " " + startKMunit });
                polyline.Data.Add(new KeyValue() { Key = "endSegmentValue", Value = endKMvalue.ToString() + " " + endKMunit });
            }
            if (transitionType!=null)
            {
                polyline.Data.Add(new KeyValue() { Key = "transitionType", Value = transitionType});
            }
           

            
            //polyline.Data.Add(new KeyValue() { Key = "startKMvalue", Value = startKMvalue.ToString() + " " + startKMunit });
            //polyline.Data.Add(new KeyValue() { Key = "endKMvalue", Value = endKMvalue.ToString() + " " + endKMunit });


        }
        public void TurnOutElementExtractProperties(Models.TopoModels.EULYNX.rsmTrack.Turnout element, CustomNode node, RsmEntities ownsRsmEntity)
        {
            string? longname = element.longname;
            string? name = element.name;

            node.Data.Add(new KeyValue() { Key = "longname", Value = longname.ToString()});
            node.Data.Add(new KeyValue() { Key = "name", Value = name.ToString() });
        }
        public void VehicleStopElementExtractProperties(Models.TopoModels.EULYNX.rsmTrack.VehicleStop element, CustomNode node, RsmEntities ownsRsmEntity)
        {
            var name = element.name;
            node.Data.Add(new KeyValue() { Key = "name", Value = name.ToString() });
        }

    }
}
