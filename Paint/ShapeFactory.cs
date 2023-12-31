using Contract;
using System.IO;
using System.Reflection;

namespace Paint;

public partial class MainWindow
{
    class ShapeFactory
    {
        readonly Dictionary<string, IShape> _prototypes = [];
        private static ShapeFactory? _instance = null;

        public static ShapeFactory Instance
        {
            get
            {
                _instance ??= new ShapeFactory();
                return _instance;
            }
        }

        public Dictionary<string, IShape> GetDictionary() => _prototypes;

        public IEnumerable<IShape> Shapes
        {
            get
            {
                var shapes = new List<IShape>();

                foreach (var shape in _prototypes.Values)
                {
                    shapes.Add(shape);
                }

                return shapes;
            }
        }

        public IEnumerable<string> ShapeNames
        {
            get
            {
                List<string> names = [];

                foreach (var shape in _prototypes.Values)
                {
                    names.Add(shape.Name);
                }

                return names;
            }
        }

        private ShapeFactory()
        {
            #region Load shapes from dll
            var exePath = Assembly.GetExecutingAssembly().Location;
            var folder = Path.GetDirectoryName(exePath);

            if (folder is null)
            {
                return;
            }

            var fis = new DirectoryInfo(folder).GetFiles("*Shape.dll");

            foreach (var f in fis)
            {
                var assembly = Assembly.LoadFile(f.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && typeof(IShape).IsAssignableFrom(type))
                    {
                        if (Activator.CreateInstance(type) is not IShape shape)
                        {
                            continue;
                        }

                        _prototypes.Add(shape.Name, shape);
                    }
                }
            }
            #endregion
        }

        public IShape Factor(string shapeName)
        {
            if (_prototypes.TryGetValue(shapeName, out IShape? value))
            {
                IShape shape = value.Clone();
                return shape;
            }

            throw new Exception($"{shapeName} | Shape not found");
        }
    }
}
