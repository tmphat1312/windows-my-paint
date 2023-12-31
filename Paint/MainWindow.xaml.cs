using Contract;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.Numerics;
using Vector = System.Windows.Vector;
using System.Security.Cryptography;

namespace Paint;

public partial class MainWindow : Fluent.RibbonWindow
{
    private readonly ShapeFactory _factory = ShapeFactory.Instance;
    public MainViewModel ViewModel { get; set; } = new MainViewModel();

    public MainWindow()
    {
        InitializeComponent();

        DataContext = ViewModel;
    }

    #region Global States
    public bool IsDrawing { get; set; } = false;
    public bool IsEditMode { get; set; } = false;
    public bool IsSaved { get; set; } = false;
    #endregion

    #region Shape properties
    private static int CurrentThickness = 1;
    private static SolidColorBrush CurrentColorBrush = new(Colors.Black);
    private static DoubleCollection? CurrentDash = null;
    #endregion

    #region Memory variables
    public List<IShape> Shapes { get; set; } = [];
    public Stack<IShape> Buffer { get; set; } = new Stack<IShape>();
    public IShape? Preview { get; set; } = null;
    public string SelectedShapeName { get; set; } = string.Empty;
    public List<IShape> CopyBuffers { get; set; } = [];
    public List<IShape> ChosenShapes { get; set; } = [];
    #endregion

    #region Variables for edit mode
    public double PreviousEditedX { get; set; } = -1;
    public double PreviousEditedY { get; set; } = -1;
    public List<ControlPoint> CtrlPoint { get; set; } = [];
    public string SelectedCtrlPointEdge { get; set; } = string.Empty;
    public string SelectedCtrlPointType { get; set; } = string.Empty;
    #endregion

    #region Canvas drawing variables
    public List<IShape> AllShapes = [];
    public string BackgroundImagePath = string.Empty;
    #endregion

    private void PaintWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AllShapes = (List<IShape>)_factory.Shapes;

        #region Update UI element source
        SolidColorsListView.ItemsSource = ViewModel.Colors;
        ShapeListView.ItemsSource = AllShapes;
        #endregion

        if (AllShapes.Count == 0)
        {
            // TODO: handle this exception with a dialog
            throw new Exception("No shape to run the application");
        }

        SelectedShapeName = _factory.ShapeNames.First();
        Preview = _factory.Factor(SelectedShapeName);

        KeyDown += RegisterKeyBoardShortCuts;
    }

    // TODO: Add more shortcuts for commands
    #region Command shortcuts
    private void RegisterKeyBoardShortCuts(object sender, KeyEventArgs e)
    {
        // Ctrl + Z == Undo
        if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.Z))
        {
            undoButton_Click(sender, e);
        }
    }
    #endregion

    #region Update Color Brush
    private void SolidColorsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedColor = SolidColorsListView.SelectedItem as SolidColorBrush;
        CurrentColorBrush = selectedColor!;
    }

    private void EditColorButton_Click(object sender, RoutedEventArgs e)
    {
        using System.Windows.Forms.ColorDialog colorPicker = new();

        if (colorPicker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            CurrentColorBrush = new SolidColorBrush(Color.FromRgb(colorPicker.Color.R, colorPicker.Color.G, colorPicker.Color.B));
        }
    }
    #endregion


    private void createNewButton_Click(object sender, RoutedEventArgs e)
    {
        if (BackgroundImagePath.Length > 0 && Shapes.Count == 0)
        {
            BackgroundImagePath = "";
            drawingArea.Background = new SolidColorBrush(Colors.White);
        }
        if (Shapes.Count == 0)
        {
            // MessageBox.Show("This canvas is empty");
            return;
        }


        if (IsSaved)
        {
            ResetToDefault();
            return;
        }

        var result = MessageBox.Show("Do you want to save current file?", "Unsaved changes detected", MessageBoxButton.YesNoCancel);

        if (MessageBoxResult.Yes == result)
        {
            // save then reset

            // save 
            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            var serializedShapeList = JsonConvert.SerializeObject(Shapes, settings);

            // experience 
            StringBuilder builder = new StringBuilder();
            builder.Append(serializedShapeList).Append("\n").Append($"{BackgroundImagePath}");
            string content = builder.ToString();


            var dialog = new System.Windows.Forms.SaveFileDialog();

            dialog.Filter = "JSON (*.json)|*.json";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string path = dialog.FileName;
                File.WriteAllText(path, content);
            }

            // reset
            ResetToDefault();
            IsSaved = true;
        }
        else if (MessageBoxResult.No == result)
        {
            //reset
            ResetToDefault();
            return;
        }
        else if (MessageBoxResult.Cancel == result)
        {
            return;
        }


    }

    private void openFileButton_Click(object sender, RoutedEventArgs e)
    {

        var dialog = new System.Windows.Forms.OpenFileDialog();

        dialog.Filter = "JSON (*.json)|*.json";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;

            string[] content = File.ReadAllLines(path);

            string background = "";
            string json = content[0];
            if (content.Length > 1)
                background = content[1];
            //string json = File.ReadAllText(path);


            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            Shapes.Clear();
            BackgroundImagePath = background;
            drawingArea.Children.Clear();

            List<IShape> containers = JsonConvert.DeserializeObject<List<IShape>>(json, settings);

            foreach (var item in containers)
                Shapes.Add(item);

            if (BackgroundImagePath.Length != 0)
            {
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(BackgroundImagePath, UriKind.Absolute));
                drawingArea.Background = brush;
            }

            //MessageBox.Show($"{background}");
        }

        foreach (var shape in Shapes)
        {
            var element = shape.Draw(shape.Brush, shape.Thickness, shape.StrokeDash);
            drawingArea.Children.Add(element);
        }


    }

    private void saveFileButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        var serializedShapeList = JsonConvert.SerializeObject(Shapes, settings);

        // experience 
        StringBuilder builder = new StringBuilder();
        builder.Append(serializedShapeList).Append("\n").Append($"{BackgroundImagePath}");
        string content = builder.ToString();


        var dialog = new System.Windows.Forms.SaveFileDialog();

        dialog.Filter = "JSON (*.json)|*.json";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;
            File.WriteAllText(path, content);
            IsSaved = true;
        }
    }

    private void SaveCanvasToImage(Canvas canvas, string filename, string extension = "png")
    {
        RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
         (int)canvas.Width, (int)canvas.Height,
         96d, 96d, PixelFormats.Pbgra32);
        // needed otherwise the image output is black
        canvas.Measure(new Size((int)canvas.Width, (int)canvas.Height));
        canvas.Arrange(new Rect(new Size((int)canvas.Width, (int)canvas.Height)));

        renderBitmap.Render(canvas);

        //JpegBitmapEncoder encoder = new JpegBitmapEncoder();




        switch (extension)
        {
            case "png":
                PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    pngEncoder.Save(file);
                }
                break;
            case "jpeg":
                JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
                jpegEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    jpegEncoder.Save(file);
                }
                break;
            case "tiff":
                TiffBitmapEncoder tiffEncoder = new TiffBitmapEncoder();
                tiffEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    tiffEncoder.Save(file);
                }
                break;
            case "bmp":

                BmpBitmapEncoder bitmapEncoder = new BmpBitmapEncoder();
                bitmapEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    bitmapEncoder.Save(file);
                }
                break;
            default:
                break;
        }
    }

    private void drawingArea_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (this.AllShapes.Count == 0)
            return;

        if (this.IsEditMode)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                ChosenShapes.Clear();
                RedrawCanvas();
                return;
            }

            Point currentPos = e.GetPosition(drawingArea);
            if (ChosenShapes.Count > 0)
            {
                CShape chosen = (CShape)ChosenShapes[0];
                if (CtrlPoint.Count > 0 && SelectedCtrlPointType == String.Empty && SelectedCtrlPointEdge == String.Empty)
                {
                    for (int i = 0; i < CtrlPoint.Count; i++)
                    {
                        if (CtrlPoint[i].IsHovering(chosen.RotateAngle, currentPos.X, currentPos.Y))
                        {
                            SelectedCtrlPointEdge = CtrlPoint[i].getEdge(chosen.RotateAngle);
                            SelectedCtrlPointType = CtrlPoint[i].Type;
                        }
                    }
                }
            }
            return;
        }

        IsDrawing = true;
        Point pos = e.GetPosition(drawingArea);

        Preview.HandleStart(pos.X, pos.Y);
    }

    private void drawingArea_MouseMove(object sender, MouseEventArgs e)
    {

        //mouse change
        bool isChange = false;
        if (ChosenShapes.Count == 1)
        {
            PShape shape1 = (PShape)ChosenShapes[0];
            Point currentPos1 = e.GetPosition(drawingArea);
            for (int i = 0; i < CtrlPoint.Count; i++)
            {
                if (CtrlPoint[i].IsHovering(shape1.RotateAngle, currentPos1.X, currentPos1.Y) || CtrlPoint[i].IsBeingChosen(this.SelectedCtrlPointType, this.SelectedCtrlPointEdge, shape1.RotateAngle))
                {
                    switch (CtrlPoint[i].getEdge(shape1.RotateAngle))
                    {
                        case "topleft" or "bottomright":
                            {
                                Mouse.OverrideCursor = Cursors.SizeNWSE;
                                break;
                            }
                        case "topright" or "bottomleft":
                            {
                                Mouse.OverrideCursor = Cursors.SizeNESW;
                                break;
                            }
                        case "top" or "bottom":
                            {
                                Mouse.OverrideCursor = Cursors.SizeNS;
                                break;
                            }
                        case "left" or "right":
                            {
                                Mouse.OverrideCursor = Cursors.SizeWE;
                                break;
                            }
                    }

                    if (CtrlPoint[i].Type == "move" || CtrlPoint[i].Type == "rotate")
                        Mouse.OverrideCursor = Cursors.Hand;

                    isChange = true;
                    break;
                }
            };

            if (!isChange)
                Mouse.OverrideCursor = null;
        }


        if (this.IsEditMode)
        {
            if (ChosenShapes.Count < 1)
                return;

            if (Mouse.LeftButton != MouseButtonState.Pressed)
                return;

            Point currentPos = e.GetPosition(drawingArea);

            double dx, dy;

            if (PreviousEditedX == -1 || PreviousEditedY == -1)
            {
                PreviousEditedX = currentPos.X;
                PreviousEditedY = currentPos.Y;
                return;
            }

            dx = currentPos.X - PreviousEditedX;
            dy = currentPos.Y - PreviousEditedY;

            if (ChosenShapes.Count > 1)
            {
                //handle multiple shapes

                ChosenShapes.ForEach(E =>
                {
                    PShape K = (PShape)E;

                    K.LeftTop.X = K.LeftTop.X + dx;
                    K.LeftTop.Y = K.LeftTop.Y + dy;
                    K.RightBottom.X = K.RightBottom.X + dx;
                    K.RightBottom.Y = K.RightBottom.Y + dy;
                });

            }
            else
            {
                // handle only one shapes
                /*
					Console.WriteLine($"dx {dx}| dy {dy}");
					Console.WriteLine($"currentPos {currentPos.X}| {currentPos.Y}");
					Console.WriteLine($"x {editPreviousX}| y {editPreviousY}");
					*/

                //controlPoint detect part
                PShape shape = (PShape)ChosenShapes[0];
                CtrlPoint.ForEach(ctrlPoint =>
                {
                    List<Cord> edges = new List<Cord>()
                    {
                    new Cord(shape.LeftTop),      // 0 xt
                    new CordY(shape.LeftTop),      // 1 yt
                    new Cord(shape.RightBottom),  // 2 xb
                    new CordY(shape.RightBottom)   // 3 yb
						};

                    List<int> rotate0 = new List<int>
                    {
                    0, 1, 2, 3
                    };
                    List<int> rotate90 = new List<int>
                    {
                    //xt, yt, xb, xb
                    3, 0, 1, 2
                    };
                    List<int> rotate180 = new List<int>
                    {
                    //xt, yt, xb, xb
                    2, 3, 0, 1
                    };
                    List<int> rotate270 = new List<int>
                    {
                    //xt, yt, xb, xb
                    1, 2, 3, 0
                    };

                    List<List<int>> rotateList = new List<List<int>>()
                    {
                    rotate0,
                    rotate90,
                    rotate180,
                    rotate270
                    };

                    double rot = shape.RotateAngle;
                    int index = 0;

                    if (rot > 0)
                        while (true)
                        {
                            rot -= 90;
                            if (rot < 0)
                                break;
                            index++;

                            if (index == 4)
                                index = 0;
                        }
                    else
                        while (true)
                        {
                            rot += 90;
                            if (rot > 0)
                                break;
                            index--;
                            if (index == -1)
                                index = 3;
                        };

                    Trace.WriteLine($"Type: ${SelectedCtrlPointType}");
                    Trace.WriteLine($"Edge: ${SelectedCtrlPointEdge}");

                    if (ctrlPoint.IsBeingChosen(this.SelectedCtrlPointType, this.SelectedCtrlPointEdge, shape.RotateAngle))
                    {
                        switch (ctrlPoint.Type)
                        {
                            case "rotate":
                                {
                                    Point2D centerPoint = shape.GetCenterPoint();

                                    Vector2 v = new Vector2((float)(currentPos.X - centerPoint.X), (float)(currentPos.Y - centerPoint.Y));
                                    double angle = (MathF.Atan2(v.Y, v.X) * (180f / Math.PI) + 450f) % 360f;

                                    shape.RotateAngle = angle;

                                    break;
                                }

                            case "move":
                                {
                                    shape.LeftTop.X = shape.LeftTop.X + dx;
                                    shape.LeftTop.Y = shape.LeftTop.Y + dy;
                                    shape.RightBottom.X = shape.RightBottom.X + dx;
                                    shape.RightBottom.Y = shape.RightBottom.Y + dy;
                                    break;
                                }

                            case "diag":
                                {
                                    Point2D handledXY = ctrlPoint.Handle(shape.RotateAngle, dx, dy);

                                    switch (index)
                                    {
                                        case 1:
                                            handledXY.X *= -1;
                                            break;
                                        case 2:
                                            {
                                                handledXY.Y *= -1;
                                                handledXY.X *= -1;
                                                break;
                                            }
                                        case 3:
                                            {
                                                handledXY.Y *= -1;
                                                break;
                                            }
                                    }


                                    switch (ctrlPoint.getEdge(shape.RotateAngle))
                                    {
                                        case "topleft":
                                            {
                                                edges[rotateList[index][0]].SetCord(handledXY.X);
                                                edges[rotateList[index][1]].SetCord(handledXY.Y);
                                                break;
                                            }
                                        case "topright":
                                            {
                                                edges[rotateList[index][2]].SetCord(handledXY.X);
                                                edges[rotateList[index][1]].SetCord(handledXY.Y);
                                                break;
                                            }
                                        case "bottomright":
                                            {
                                                edges[rotateList[index][2]].SetCord(handledXY.X);
                                                edges[rotateList[index][3]].SetCord(handledXY.Y);
                                                break;
                                            }
                                        case "bottomleft":
                                            {
                                                edges[rotateList[index][0]].SetCord(handledXY.X);
                                                edges[rotateList[index][3]].SetCord(handledXY.Y);
                                                break;
                                            }
                                        case "right":
                                            {
                                                edges[rotateList[index][2]].SetCord(handledXY.X);
                                                break;
                                            }
                                        case "left":
                                            {
                                                edges[rotateList[index][0]].SetCord(handledXY.X);
                                                break;
                                            }
                                        case "top":
                                            {
                                                edges[rotateList[index][3]].SetCord(-handledXY.Y);
                                                break;
                                            }
                                        case "bottom":
                                            {
                                                edges[rotateList[index][3]].SetCord(handledXY.Y);
                                                break;
                                            }
                                    }
                                    break;
                                }
                        }
                    }

                });
            }


            PreviousEditedX = currentPos.X;
            PreviousEditedY = currentPos.Y;

            RedrawCanvas();
            return;
        }

        if (IsDrawing)
        {
            Point pos = e.GetPosition(drawingArea);

            Preview.HandleEnd(pos.X, pos.Y);

            // delete old shapes
            drawingArea.Children.Clear();

            // redraw all shapes
            foreach (var shape in Shapes)
            {
                UIElement element = shape.Draw(shape.Brush, shape.Thickness, shape.StrokeDash);
                drawingArea.Children.Add(element);
            }

            // lastly, draw preview object 
            drawingArea.Children.Add(Preview.Draw(CurrentColorBrush, CurrentThickness, CurrentDash));
        }
    }

    private void drawingArea_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (this.AllShapes.Count == 0)
            return;

        IsDrawing = false;

        if (this.IsEditMode)
        {

            if (e.ChangedButton != MouseButton.Left)
                return;

            Point currentPos = e.GetPosition(drawingArea);
            for (int i = this.Shapes.Count - 1; i >= 0; i--)
            {
                PShape temp = (PShape)Shapes[i];
                if (temp.IsHovering(currentPos.X, currentPos.Y))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (!ChosenShapes.Contains(Shapes[i]))
                            this.ChosenShapes.Add(Shapes[i]);
                        else
                            this.ChosenShapes.Remove(Shapes[i]);
                    }
                    else
                    {
                        ChosenShapes.Clear();
                        this.ChosenShapes.Add(Shapes[i]);
                    }

                    RedrawCanvas();
                    break;
                }
            }

            this.PreviousEditedX = -1;
            this.PreviousEditedY = -1;

            // Restore SelectedCtrlPointEdge to Empty
            this.SelectedCtrlPointEdge = String.Empty;
            this.SelectedCtrlPointType = String.Empty;
            return;
        }

        Point pos = e.GetPosition(drawingArea);
        Preview.HandleEnd(pos.X, pos.Y);

        // Ddd to shapes list & save it color + thickness
        Shapes.Add(Preview);
        Preview.Brush = CurrentColorBrush;
        Preview.Thickness = CurrentThickness;
        Preview.StrokeDash = CurrentDash;

        // Draw new thing -> isSaved = false
        IsSaved = false;

        // Move to new preview 
        Preview = _factory.Factor(SelectedShapeName);

        // Re-draw the canvas

        RedrawCanvas();
    }

    private void drawingArea_MouseLeave(object sender, MouseEventArgs e)
    {
        //this._isDrawing = false;
    }
    private void drawingArea_MouseEnter(object sender, MouseEventArgs e)
    {
        if (this.AllShapes.Count == 0)
            return;

        if (this.IsEditMode)
            return;

        if (Mouse.LeftButton != MouseButtonState.Pressed && this.IsDrawing)
        {
            //wish there is a better solution like
            // this.drawingArea_MouseUp(sender, e)
            // but e is not MouseButtonEventArgs (;-;)
            IsDrawing = false;

            Point pos = e.GetPosition(drawingArea);
            Preview.HandleEnd(pos.X, pos.Y);

            // Ddd to shapes list & save it color + thickness
            Shapes.Add(Preview);
            Preview.Brush = CurrentColorBrush;
            Preview.Thickness = CurrentThickness;
            Preview.StrokeDash = CurrentDash;

            // Draw new thing -> isSaved = false
            IsSaved = false;

            // Move to new preview 
            Preview = _factory.Factor(SelectedShapeName);

            // Re-draw the canvas
            RedrawCanvas();
        }
    }

    private void sizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int index = sizeComboBox.SelectedIndex;

        switch (index)
        {
            case 0:
                CurrentThickness = 1;
                break;
            case 1:
                CurrentThickness = 2;
                break;
            case 2:
                CurrentThickness = 3;
                break;
            case 3:
                CurrentThickness = 5;
                break;
            default:
                break;
        }
    }

    private void ShapeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (this.AllShapes.Count == 0)
            return;

        var index = ShapeListView.SelectedIndex;

        SelectedShapeName = AllShapes[index].Name;

        Preview = _factory.Factor(SelectedShapeName);
    }

    private void exportButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.SaveFileDialog();
        dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;
            string extension = path.Substring(path.LastIndexOf('\\') + 1).Split('.')[1];

            SaveCanvasToImage(drawingArea, path, extension);
        }
        IsSaved = true;
    }

    private void dashComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int index = dashComboBox.SelectedIndex;

        switch (index)
        {
            case 0:
                CurrentDash = null;
                break;
            case 1:
                CurrentDash = [1, 1];
                break;
            case 2:
                CurrentDash = [6, 1];
                break;
            default:
                break;
        }
    }

    private void importButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.OpenFileDialog();
        dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff";

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;

            BackgroundImagePath = path;

            ImageBrush brush = new ImageBrush();
            brush.ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute));
            drawingArea.Background = brush;
        }
    }

    private void ResetToDefault()
    {
        if (this.AllShapes.Count == 0)
            return;

        IsSaved = false;
        IsDrawing = false;
        IsEditMode = false;

        ChosenShapes.Clear();

        Shapes.Clear();

        SelectedShapeName = AllShapes[0].Name;
        Preview = _factory.Factor(SelectedShapeName);

        CurrentThickness = 1;
        CurrentColorBrush = new SolidColorBrush(Colors.Red);
        CurrentDash = null;

        BackgroundImagePath = "";

        dashComboBox.SelectedIndex = 0;
        sizeComboBox.SelectedIndex = 0;

        EditMode.Header = "Draw Mode";
        drawingArea.Children.Clear();
        drawingArea.Background = new SolidColorBrush(Colors.White);
    }

    private void undoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Shapes.Count == 0)
            return;
        if (Shapes.Count == 0 && Buffer.Count == 0)
            return;

        // Push last shape into buffer and remove it from final list, then re-draw canvas
        int lastIndex = Shapes.Count - 1;
        Buffer.Push(Shapes[lastIndex]);
        Shapes.RemoveAt(lastIndex);

        RedrawCanvas();
    }

    private void redoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Buffer.Count == 0)
            return;
        if (Shapes.Count == 0 && Buffer.Count == 0)
            return;

        // Pop the last shape from buffer and add it to final list, then re-draw canvas
        Shapes.Add(Buffer.Pop());
        RedrawCanvas();
    }

    private void RedrawCanvas()
    {
        drawingArea.Children.Clear();
        Console.WriteLine(Shapes.Count);
        foreach (var shape in Shapes)
        {
            var element = shape.Draw(shape.Brush, shape.Thickness, shape.StrokeDash);
            drawingArea.Children.Add(element);
        }

        //control Point display ontop
        //rework
        if (IsEditMode && ChosenShapes.Count > 0)
        {
            ChosenShapes.ForEach(shape =>
            {
                PShape chosedShape = (PShape)shape;
                drawingArea.Children.Add(chosedShape.ControlOutline());

                //if only chose one shape
                if (ChosenShapes.Count == 1)
                {
                    List<ControlPoint> ctrlPoints = chosedShape.GetControlPoints();
                    this.CtrlPoint = ctrlPoints;
                    ctrlPoints.ForEach(K =>
                    {
                        drawingArea.Children.Add(K.DrawPoint(chosedShape.RotateAngle, chosedShape.GetCenterPoint()));
                    });
                }
            });
        }
    }

    private void EditMode_Click(object sender, RoutedEventArgs e)
    {
        this.IsEditMode = !this.IsEditMode;
        if (IsEditMode)
            EditMode.Header = "Edit Mode";
        else EditMode.Header = "Draw Mode";

        if (!this.IsEditMode)
            this.ChosenShapes.Clear();
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditMode)
        {

            ChosenShapes.ForEach(k =>
            {
                Shapes.Remove(k);
            });

            ChosenShapes.Clear();
            RedrawCanvas();
        }
    }

    private void copyButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditMode)
        {
            ChosenShapes.ForEach(K =>
            {
                CopyBuffers.Add(K);
            });
        }
    }

    private void pasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditMode)
        {
            CopyBuffers.ForEach(K =>
            {
                PShape temp = (PShape)K;
                Shapes.Add((IShape)temp.DeepCopy());
            });
            RedrawCanvas();
        }

    }
}
