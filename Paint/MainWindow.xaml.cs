using Contract;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        if (AllShapes.Count == 0)
        {
            MessageBox.Show("No shape to run the application");
            Close();
            return;
        }

        #region Update UI element source
        SolidColorsListView.ItemsSource = ViewModel.ColorOptions;
        ShapeListView.ItemsSource = AllShapes;
        SizeComboBox.ItemsSource = ViewModel.ThicknessOptions;
        DashComboBox.ItemsSource = ViewModel.DashOptions;
        UpdateSelectedShape(AllShapes.First());
        UpdateModeUI();
        #endregion

        KeyDown += RegisterKeyBoardShortCuts;
    }

    #region Utilities methods
    private void UpdateSelectedShape(IShape selectedShape)
    {
        Preview = selectedShape;
        SelectedShapeName = selectedShape.Name;
        CurrentShapeText.Text = selectedShape.Name;
    }

    private void UpdateModeUI()
    {
        DrawingModeButton.Visibility = Visibility.Collapsed;
        EditModeButton.Visibility = Visibility.Collapsed;

        if (IsEditMode)
        {
            DrawingHandlerArea.Cursor = Cursors.Hand;
            DrawingModeButton.Visibility = Visibility.Visible;
            CurrentModeText.Text = "Editing";
        }
        else
        {
            DrawingHandlerArea.Cursor = Cursors.Cross;
            EditModeButton.Visibility = Visibility.Visible;
            CurrentModeText.Text = "Drawing";
        }
    }

    private void DrawOnCanvas()
    {
        DrawingArea.Children.Clear();

        foreach (var shape in Shapes)
        {
            var element = shape.Draw(shape.Brush, shape.Thickness, shape.StrokeDash!);
            DrawingArea.Children.Add(element);
        }

        if (IsEditMode && ChosenShapes.Count > 0)
        {
            ChosenShapes.ForEach(shape =>
            {
                PShape chosedShape = (PShape)shape;
                DrawingArea.Children.Add(chosedShape.ControlOutline());

                if (ChosenShapes.Count == 1)
                {
                    List<ControlPoint> ctrlPoints = chosedShape.GetControlPoints();
                    this.CtrlPoint = ctrlPoints;

                    ctrlPoints.ForEach(K =>
                    {
                        DrawingArea.Children.Add(K.DrawPoint(chosedShape.RotateAngle, chosedShape.GetCenterPoint()));
                    });
                }
            });
        }
    }

    private void ResetToDefault()
    {
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

        DashComboBox.SelectedIndex = 0;
        SizeComboBox.SelectedIndex = 0;

        DrawingArea.Children.Clear();
        DrawingArea.Background = new SolidColorBrush(Colors.White);
    }

    private void AddBackground(string path)
    {
        BackgroundImagePath = path;

        ImageBrush brush = new()
        {
            ImageSource = new BitmapImage(new Uri(path, UriKind.Absolute)),
            Stretch = Stretch.UniformToFill,
        };

        DrawingArea.Background = brush;
    }
    #endregion


    #region Command shortcuts
    private void RegisterKeyBoardShortCuts(object sender, KeyEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl))
        {
            // Ctrl + Z == Undo
            if (Keyboard.IsKeyDown(Key.Z))
            {
                UndoButton_Click(sender, e);
            }
            // Ctrl + Y == Redo
            else if (Keyboard.IsKeyDown(Key.Y))
            {
                RedoButton_Click(sender, e);
            }
            // Ctrl + C == Copy
            else if (Keyboard.IsKeyDown(Key.C))
            {
                CopyButton_Click(sender, e);
            }
            // Ctrl + V == Paste
            else if (Keyboard.IsKeyDown(Key.V))
            {
                PasteButton_Click(sender, e);
            }
            // Ctrl + A == Select all
            else if (Keyboard.IsKeyDown(Key.A))
            {
                ChosenShapes.Clear();
                Shapes.ForEach(K =>
                {
                    ChosenShapes.Add(K);
                });
                DrawOnCanvas();
            }
            // Ctrl + S == Save
            else if (Keyboard.IsKeyDown(Key.S))
            {
                SaveFileButton_Click(sender, e);
            }
            // Ctrl + O == Open
            else if (Keyboard.IsKeyDown(Key.O))
            {
                OpenFileButton_Click(sender, e);
            }
            // Ctrl + N == New
            else if (Keyboard.IsKeyDown(Key.N))
            {
                CreateNewButton_Click(sender, e);
            }
            // Ctrl + E == Edit mode
            else if (Keyboard.IsKeyDown(Key.E))
            {
                ChangeMode_Click(sender, e);
            }
            // Ctrl + D == Drawing mode
            else if (Keyboard.IsKeyDown(Key.D))
            {
                ChangeMode_Click(sender, e);
            }
            // Ctrl + X == Cut
            else if (Keyboard.IsKeyDown(Key.X))
            {
                CopyButton_Click(sender, e);
                Delete_Click(sender, e);
            }
        }

        if (Keyboard.IsKeyDown(Key.Delete))
        {
            Delete_Click(sender, e);
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

    #region Toolbox controls (Shapes, Size, Dash)
    private void ShapeListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedShape = (IShape)ShapeListView.SelectedItem;
        UpdateSelectedShape(selectedShape);
    }

    private void SizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedSize = SizeComboBox.SelectedItem as ThicknessOption;
        CurrentThickness = selectedSize!.Value;
    }

    private void DashComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var selectedDash = DashComboBox.SelectedItem as DashOption;
        CurrentDash = selectedDash!.Value;
    }
    #endregion

    #region Toolbox controls (Mode, Undo, Redo, Delete, Copy, Paste)
    private void ChangeMode_Click(object sender, RoutedEventArgs e)
    {
        this.IsEditMode = !this.IsEditMode;

        UpdateModeUI();

        if (!this.IsEditMode)
        {
            this.ChosenShapes.Clear();
        }
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
            DrawOnCanvas();
        }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditMode)
        {
            ChosenShapes.ForEach(K =>
            {
                CopyBuffers.Add(K);
            });
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.IsEditMode)
        {
            CopyBuffers.ForEach(K =>
            {
                PShape temp = (PShape)K;
                Shapes.Add((IShape)temp.DeepCopy());
            });
            DrawOnCanvas();
        }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Buffer.Count == 0)
            return;

        int lastIndex = Shapes.Count - 1;
        Buffer.Push(Shapes[lastIndex]);
        Shapes.RemoveAt(lastIndex);

        DrawOnCanvas();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Buffer.Count == 0)
            return;

        // Pop the last shape from buffer and add it to final list, then re-draw canvas
        Shapes.Add(Buffer.Pop());
        DrawOnCanvas();
    }
    #endregion

    #region Drawing Area
    private void drawingArea_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (this.AllShapes.Count == 0)
            return;

        if (this.IsEditMode)
        {
            if (Mouse.RightButton == MouseButtonState.Pressed)
            {
                ChosenShapes.Clear();
                DrawOnCanvas();
                return;
            }

            Point currentPos = e.GetPosition(DrawingArea);
            if (ChosenShapes.Count > 0)
            {
                PShape chosen = (PShape)ChosenShapes[0];
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
        Point pos = e.GetPosition(DrawingArea);

        Preview.HandleStart(pos.X, pos.Y);
    }

    private void drawingArea_MouseMove(object sender, MouseEventArgs e)
    {

        //mouse change
        bool isChange = false;
        if (ChosenShapes.Count == 1)
        {
            PShape shape1 = (PShape)ChosenShapes[0];
            Point currentPos1 = e.GetPosition(DrawingArea);
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

            Point currentPos = e.GetPosition(DrawingArea);

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

            DrawOnCanvas();
            return;
        }

        if (IsDrawing)
        {
            Point pos = e.GetPosition(DrawingArea);

            Preview.HandleEnd(pos.X, pos.Y);

            // delete old shapes
            DrawingArea.Children.Clear();

            // redraw all shapes
            foreach (var shape in Shapes)
            {
                UIElement element = shape.Draw(shape.Brush, shape.Thickness, shape.StrokeDash);
                DrawingArea.Children.Add(element);
            }

            // lastly, draw preview object 
            DrawingArea.Children.Add(Preview.Draw(CurrentColorBrush, CurrentThickness, CurrentDash));
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

            Point currentPos = e.GetPosition(DrawingArea);
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

                    DrawOnCanvas();
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

        Point pos = e.GetPosition(DrawingArea);
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

        DrawOnCanvas();
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

            Point pos = e.GetPosition(DrawingArea);
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
            DrawOnCanvas();
        }
    }
    #endregion

    #region File Actions
    // TODO: Update this method
    private void CreateNewButton_Click(object sender, RoutedEventArgs e)
    {
        if (BackgroundImagePath.Length > 0 && Shapes.Count == 0)
        {
            BackgroundImagePath = "";
            DrawingArea.Background = new SolidColorBrush(Colors.White);
        }
        if (Shapes.Count == 0)
        {
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

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = "JSON (*.json)|*.json"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;

            string[] content = File.ReadAllLines(path);

            string background = "";
            string json = content[0];

            if (content.Length > 1)
            {
                background = content[1];
            }

            var settings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            Shapes.Clear();
            ChosenShapes.Clear();
            List<IShape> savedShapes = JsonConvert.DeserializeObject<List<IShape>>(json, settings);

            foreach (var item in savedShapes!)
            {
                Shapes.Add(item);
            }

            if (!string.IsNullOrEmpty(background))
            {
                AddBackground(background);
            }
        }

        DrawOnCanvas();
    }

    private void SaveFileButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        var serializedShapeList = JsonConvert.SerializeObject(Shapes, settings);

        StringBuilder builder = new();
        builder.Append(serializedShapeList).Append('\n').Append($"{BackgroundImagePath}");
        string content = builder.ToString();

        var dialog = new System.Windows.Forms.SaveFileDialog
        {
            Filter = "JSON (*.json)|*.json"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;
            File.WriteAllText(path, content);
            IsSaved = true;
        }
    }

    private void SaveCanvasToImage(string filename, string extension = "png")
    {
        RenderTargetBitmap renderBitmap = new(
         (int)DrawingArea.ActualWidth, (int)DrawingArea.ActualHeight,
         96d, 96d, PixelFormats.Pbgra32);

        DrawingArea.Measure(new Size((int)DrawingArea.ActualWidth, (int)DrawingArea.ActualHeight));
        DrawingArea.Arrange(new Rect(new Size((int)DrawingArea.ActualWidth, (int)DrawingArea.ActualHeight)));

        renderBitmap.Render(DrawingArea);

        switch (extension)
        {
            case "png":
                PngBitmapEncoder pngEncoder = new();
                pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    pngEncoder.Save(file);
                }
                break;
            case "jpeg":
                JpegBitmapEncoder jpegEncoder = new();
                jpegEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    jpegEncoder.Save(file);
                }
                break;
            case "tiff":
                TiffBitmapEncoder tiffEncoder = new();
                tiffEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                using (FileStream file = File.Create(filename))
                {
                    tiffEncoder.Save(file);
                }
                break;
            case "bmp":

                BmpBitmapEncoder bitmapEncoder = new();
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

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.SaveFileDialog
        {
            Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;
            string extension = path[(path.LastIndexOf('\\') + 1)..].Split('.')[1];

            SaveCanvasToImage(path, extension);
        }

        IsSaved = true;
    }

    private void AddPictureButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.OpenFileDialog
        {
            Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp | TIFF (*.tiff)|*.tiff"
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            string path = dialog.FileName;

            AddBackground(path);
        }
    }
    #endregion
}
