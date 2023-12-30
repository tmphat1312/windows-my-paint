namespace Contract;

public class Util
{
    public static bool IsBetween(double x, double x1, double x2)
    {
        if (x1 > x2)
            return x < x1 && x > x2;
        else
            return x < x2 && x > x1;
    }
}

//to select reference value of X or Y of point2D cord 
public class Cord
{
    public Point2D Point = new();

    public Cord()
    {
    }

    public Cord(Point2D point)
    {
        Point = point;
    }

    virtual public double GetCord()
    {
        return Point.X;
    }
    virtual public void SetCord(double x)
    {
        Point.X += x;
    }
}

public class CordY : Cord
{
    public CordY() { }

    public CordY(Point2D point) : base(point) { }

    public override double GetCord()
    {
        return Point.Y;
    }

    public override void SetCord(double x)
    {
        Point.Y += x;
    }
}
