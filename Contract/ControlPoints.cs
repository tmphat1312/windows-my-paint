namespace Contract;

public class RotatePoint : ControlPoint
{
    public override string Type => "rotate";
}

public class DiagPoint : ControlPoint
{
    public override string Type => "diag";
}

public class OneSidePoint : ControlPoint
{
    public override string Type => "diag";

    public override string getEdge(double angle)
    {
        string[] edge = ["top", "right", "bottom", "left"];
        int index = 0;
        if (CentrePoint.X == Point.X)
            if (CentrePoint.Y > Point.Y)
                index = 0;
            else
                index = 2;
        else
            if (CentrePoint.Y == Point.Y)
            if (CentrePoint.X > Point.X)
                index = 3;
            else
                index = 1;

        return edge[index];
    }

}

public class EndPoint : ControlPoint
{
    public override string Type => "end";
    public string Edge { get; set; }
    public override string getEdge(double angle)
    {
        return Edge;
    }
}
