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

        double rot = angle;

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
