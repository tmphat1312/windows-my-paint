using Contract;
using Newtonsoft.Json;
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
    public string CurrentShapeName { get; set; } = string.Empty;
    private static int CurrentThickness = 1;
    private static SolidColorBrush CurrentColorBrush = new(Colors.Black);
    private static DoubleCollection? CurrentDash = null;
    public IShape CurrentDrawingShape { get; set; }
    #endregion

    #region Memory variables
    public List<IShape> Shapes { get; set; } = [];
    public Stack<IShape> Buffer { get; set; } = new Stack<IShape>();
    public List<IShape> CopyBuffers { get; set; } = [];
    public List<IShape> SelectedShapes { get; set; } = [];
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
        UpdateCurrentDrawingShape(AllShapes.First());
        UpdateModeUI();
        #endregion

        KeyDown += RegisterKeyBoardShortCuts;
        MouseDown += RegisterMouseShortCuts;
    }

    #region Utilities methods
    private void UpdateCurrentDrawingShape(IShape? selectedShape = null)
    {
        if (selectedShape is not null)
        {
            CurrentShapeName = selectedShape.Name;
            CurrentShapeText.Text = selectedShape.Name;
            CurrentDrawingShape = selectedShape;
        }

        CurrentDrawingShape.Brush = CurrentColorBrush;
        CurrentDrawingShape.Thickness = CurrentThickness;
        CurrentDrawingShape.StrokeDash = CurrentDash;
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

        if (IsEditMode && SelectedShapes.Count > 0)
        {
            SelectedShapes.ForEach(shape =>
            {
                PShape chosedShape = (PShape)shape;
                DrawingArea.Children.Add(chosedShape.ControlOutline());

                if (SelectedShapes.Count == 1)
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

        ClearDrawingArea();
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

    private void ClearSelectedShapes()
    {
        SelectedShapes.Clear();
        DrawOnCanvas();
    }

    private void ClearDrawingArea()
    {
        BackgroundImagePath = string.Empty;

        Shapes.Clear();
        SelectedShapes.Clear();
        DrawingArea.Children.Clear();
        DrawingArea.Background = new SolidColorBrush(Colors.White);
    }

    private void AddPreviewToCanvas()
    {
        PreviewDrawingArea.Children.Clear();
        PreviewDrawingArea.Children.Add(CurrentDrawingShape.Draw(CurrentColorBrush, CurrentThickness, CurrentDash!));
    }

    private void MakePreviewStable()
    {
        PreviewDrawingArea.Children.Clear();

        Shapes.Add(CurrentDrawingShape);
        DrawingArea.Children.Add(CurrentDrawingShape.Draw(CurrentColorBrush, CurrentThickness, CurrentDash!));
    }

    private MessageBoxResult SaveFilePrompt()
    {
        if (IsSaved || DrawingArea.Children.Count == 0)
        {
            return MessageBoxResult.Yes;
        }

        var result = MessageBox.Show("Your current session will be lost?", "Do you want to save this working session?", MessageBoxButton.YesNoCancel);

        if (MessageBoxResult.Yes == result)
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
            }

            IsSaved = true;

        }

        return result;
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
                SelectedShapes.Clear();
                Shapes.ForEach(K =>
                {
                    SelectedShapes.Add(K);
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
            // Ctrl + M == Toggle Modes
            else if (Keyboard.IsKeyDown(Key.M))
            {
                ChangeMode_Click(sender, e);
            }
            // Ctrl + X == Cut
            else if (Keyboard.IsKeyDown(Key.X))
            {
                CutButton_Click(sender, e);
            }
            // Ctrl + D == Duplicate
            else if (Keyboard.IsKeyDown(Key.D))
            {
                DuplicateButton_Click(sender, e);
            }
        }

        if (Keyboard.IsKeyDown(Key.Delete))
        {
            Delete_Click(sender, e);
        }
    }

    private void RegisterMouseShortCuts(object sender, MouseButtonEventArgs e)
    {
        if (Mouse.RightButton == MouseButtonState.Pressed)
        {
            if (IsEditMode)
            {
                ClearSelectedShapes();
            }
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
        UpdateCurrentDrawingShape(selectedShape);
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
        IsEditMode = !IsEditMode;

        UpdateModeUI();

        if (!IsEditMode)
        {
            ClearSelectedShapes();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            SelectedShapes.ForEach(k =>
            {
                Shapes.Remove(k);
            });

            ClearSelectedShapes();
        }
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            CopyBuffers.Clear();

            SelectedShapes.ForEach(K =>
            {
                CopyBuffers.Add(K);
            });
        }
    }

    private void DuplicateButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            CopyButton_Click(sender, e);

            SelectedShapes.Clear();

            CopyBuffers.ForEach(K =>
            {
                PShape temp = (PShape)K;
                IShape cloned = (IShape)temp.DeepCopy();
                PShape copied = (PShape)cloned;

                var pos = Mouse.GetPosition(DrawingArea);

                copied.LeftTop.X += 10;
                copied.LeftTop.Y += 10;
                copied.RightBottom.X += 10;
                copied.RightBottom.Y += 10;

                Shapes.Add(cloned);
                SelectedShapes.Add(cloned);
            });
            DrawOnCanvas();
        }
    }

    private void PasteButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            SelectedShapes.Clear();

            CopyBuffers.ForEach(K =>
            {
                PShape temp = (PShape)K;
                IShape cloned = (IShape)temp.DeepCopy();
                PShape copied = (PShape)cloned;

                var pos = Mouse.GetPosition(DrawingArea);

                if (copied.IsHovering(pos.X, pos.Y))
                {
                    copied.LeftTop.X += 10;
                    copied.LeftTop.Y += 10;
                    copied.RightBottom.X += 10;
                    copied.RightBottom.Y += 10;
                }
                else
                {
                    var width = copied.RightBottom.X - copied.LeftTop.X;
                    var height = copied.RightBottom.Y - copied.LeftTop.Y;

                    copied.LeftTop.X = pos.X;
                    copied.LeftTop.Y = pos.Y;
                    copied.RightBottom.X = pos.X + width;
                    copied.RightBottom.Y = pos.Y + height;
                }

                Shapes.Add(cloned);
                SelectedShapes.Add(cloned);
            });
            DrawOnCanvas();

        }
    }

    private void CutButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsEditMode)
        {
            CopyBuffers.Clear();

            SelectedShapes.ForEach(K =>
            {
                CopyBuffers.Add(K);
            });

            SelectedShapes.ForEach(K =>
            {
                Shapes.Remove(K);
            });

            ClearSelectedShapes();

            DrawOnCanvas();
        }
    }

    private void UndoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Shapes.Count == 0)
        {
            return;
        }

        int lastIndex = Shapes.Count - 1;

        Buffer.Push(Shapes[lastIndex]);
        Shapes.RemoveAt(lastIndex);
        DrawOnCanvas();
    }

    private void RedoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Buffer.Count == 0)
        {
            return;
        }

        Shapes.Add(Buffer.Pop());
        DrawOnCanvas();
    }
    #endregion

    #region Drawing Area
    private void DrawingArea_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        if (!IsEditMode)
        {
            IsDrawing = true;
            Point pos = e.GetPosition(DrawingArea);
            CurrentDrawingShape.HandleStart(pos.X, pos.Y);

            return;
        }

        Point currentPos = e.GetPosition(DrawingArea);

        if (SelectedShapes.Count > 0)
        {
            PShape chosen = (PShape)SelectedShapes[0];
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
    }

    private void DrawingArea_MouseMove(object sender, MouseEventArgs e)
    {
        bool isMouseChange = false;

        if (SelectedShapes.Count == 1)
        {
            PShape shape1 = (PShape)SelectedShapes[0];
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

                    isMouseChange = true;
                    break;
                }
            };

            if (!isMouseChange)
            {
                Mouse.OverrideCursor = null;
            }
        }

        if (IsEditMode)
        {
            if (SelectedShapes.Count < 1 || (Mouse.LeftButton != MouseButtonState.Pressed))
            {
                return;
            }

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

            if (SelectedShapes.Count > 1)
            {
                SelectedShapes.ForEach(E =>
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
                PShape shape = (PShape)SelectedShapes[0];

                CtrlPoint.ForEach(ctrlPoint =>
                {
                    List<Cord> edges =
                        [
                            new Cord(shape.LeftTop),
                            new CordY(shape.LeftTop),
                            new Cord(shape.RightBottom),
                            new CordY(shape.RightBottom)
                        ];

                    List<int> rotate0 = [0, 1, 2, 3];
                    List<int> rotate90 = [3, 0, 1, 2];
                    List<int> rotate180 = [2, 3, 0, 1];
                    List<int> rotate270 = [1, 2, 3, 0];

                    List<List<int>> rotateList = [rotate0, rotate90, rotate180, rotate270];

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

                    //Trace.WriteLine($"Type: ${SelectedCtrlPointType}");
                    //Trace.WriteLine($"Edge: ${SelectedCtrlPointEdge}");

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
            CurrentDrawingShape.HandleEnd(pos.X, pos.Y);

            AddPreviewToCanvas();
            UpdateCurrentDrawingShape();
        }
    }

    private void DrawingArea_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
        {
            return;
        }

        IsDrawing = false;

        if (IsEditMode)
        {
            Point currentPos = e.GetPosition(DrawingArea);

            for (int i = this.Shapes.Count - 1; i >= 0; i--)
            {
                PShape temp = (PShape)Shapes[i];

                if (temp.IsHovering(currentPos.X, currentPos.Y))
                {
                    if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        if (!SelectedShapes.Contains(Shapes[i]))
                            this.SelectedShapes.Add(Shapes[i]);
                        else
                            this.SelectedShapes.Remove(Shapes[i]);
                    }
                    else
                    {
                        SelectedShapes.Clear();
                        SelectedShapes.Add(Shapes[i]);
                    }

                    DrawOnCanvas();
                    break;
                }
            }

            this.PreviousEditedX = -1;
            this.PreviousEditedY = -1;

            this.SelectedCtrlPointEdge = String.Empty;
            this.SelectedCtrlPointType = String.Empty;
        }
        else
        {
            Point pos = e.GetPosition(DrawingArea);
            CurrentDrawingShape.HandleEnd(pos.X, pos.Y);

            MakePreviewStable();

            CurrentDrawingShape = _factory.Factor(CurrentShapeName);

            IsSaved = false;
        }
    }

    private void DrawingArea_MouseLeave(object sender, MouseEventArgs e)
    {
        if (IsDrawing)
        {
            IsDrawing = false;

            Point pos = e.GetPosition(DrawingArea);
            CurrentDrawingShape.HandleEnd(pos.X - 1, pos.Y - 1);

            UpdateCurrentDrawingShape();
            Shapes.Add(CurrentDrawingShape);

            MakePreviewStable();

            CurrentDrawingShape = _factory.Factor(CurrentShapeName);

            IsSaved = false;
        }
    }
    #endregion

    #region File Actions
    private void CreateNewButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrEmpty(BackgroundImagePath) && Shapes.Count == 0)
        {
            BackgroundImagePath = string.Empty;
            DrawingArea.Background = new SolidColorBrush(Colors.White);
        }

        if (DrawingArea.Children.Count == 0)
        {
            return;
        }

        if (IsSaved)
        {
            ResetToDefault();
            return;
        }

        SaveFilePrompt();

        ResetToDefault();
    }

    private void OpenFileButton_Click(object sender, RoutedEventArgs e)
    {
        bool isCancelled = SaveFilePrompt() == MessageBoxResult.Cancel;

        if (isCancelled)
        {
            return;
        }

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
            SelectedShapes.Clear();
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
