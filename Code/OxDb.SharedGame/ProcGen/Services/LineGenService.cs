using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.LineGen;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.ProcGen.Entities;
using OxDb.SharedGame.ProcGen.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


public class LineCell : Point2I
{
    public bool IsCenter { get; set; }
    public bool IsEdge { get; set; }

    public LineCell(int x, int z, bool isCenter, bool isEdge) : base(x,z)
    {
        IsCenter = isCenter;    
        IsEdge = isEdge;  
    }
}

public interface IComponentGrid
{
    int Width { get; }
    int Height { get; }
    bool IsConnectedTo(int x, int z, int nx, int nz);
    bool HasCell(int x, int z);
}

public interface ILineGenService : IInitializable
{
    List<LineCell> GetBressenhamLine(Point2I start, Point2I end, LineGenParameters lg = null);
    List<LineCell> GetBressenhamCircle(Point2I center, LineGenParameters pars);
    List<ConnectedPairData> ConnectPoints(List<ConnectPointData> points, IRandom rand, float extraConnectionPct = 0.0f);
    List<Point2I> GridConnect(int sx, int sz, int ex, int ez, bool xFirst);

    List<Point2I> GetRotatedEllipse(float cx, float cy, float rx, float ry, float angle);

    int[,] GetConnectedComponents(IComponentGrid grid);
}




public class LineGenService : ILineGenService
{
    protected INoiseService _noiseService = null!;
    public async Task Initialize(CancellationToken token)
    {
        await Task.CompletedTask;
    }

    public List<LineCell> GetBressenhamLine(Point2I start, Point2I end, LineGenParameters lg = null)
    {
        if (lg == null)
        {
            lg = new LineGenParameters();
        }

        start.X = MathUtil.Clamp(lg.MinX, start.X, lg.MaxX);
        start.Z = MathUtil.Clamp(lg.MinZ, start.Z, lg.MaxZ);
        end.X = MathUtil.Clamp(lg.MinX, end.X, lg.MaxX);
        end.Z = MathUtil.Clamp(lg.MinZ, end.Z, lg.MaxZ);

        MyRandom rand = new MyRandom(lg.Seed);

        bool addToEndOnEvenWidth = rand.NextDouble() < 0.5f;

        List<LineCell> retval = new List<LineCell>();
        if (start == null || end == null)
        {
            return retval;
        }

        double widthPosDrift = lg.MaxWidthPosDrift * (1 - 2 * rand.NextDouble());

        // Only used to determine which axis is longer, we move along that axis.
        int dx = end.X - start.X;
        int dy = end.Z - start.Z;

        int remainder = 0;

        int pathWidth = lg.WidthSize; // curr width of the line.

        bool xAxis = false;

        int length = 0; // Along major axis
        int width = 0; // Along minor axis

        int dl = 0;
        int dw = 0;

        int sl = 0;
        int sw = 0;
        int el = 0;
        int ew = 0;
        float cl = 0;
        float widthDelta = Math.Abs(lg.MaxWidthSize - lg.MinWidthSize);
        float startWidth = lg.MinWidthSize;

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            xAxis = true;
            length = Math.Abs(dx);
            width = Math.Abs(dy);
            dl = Math.Sign(dx);
            dw = Math.Sign(dy);
            sl = start.X;
            sw = start.Z;
            el = end.X;
            ew = end.Z;
            cl = (sl + el) / 2.0f;
        }
        else // Swap cx and cz axis.
        {
            xAxis = false;
            length = Math.Abs(dy);
            width = Math.Abs(dx);
            dl = Math.Sign(dy);
            dw = Math.Sign(dx);
            sl = start.Z;
            sw = start.X;
            el = end.Z;
            ew = end.X;
            cl = (sl + el) / 2.0f;
        }
        int[] offsets = new int[length + 1];

        if (lg.LinePathNoiseScale > 0)
        {
            float freq = RandUtils.FloatRange(0.0150f, 0.020f, rand) * length * 0.3f;
            if (rand.NextDouble() < 0.2f)
            {
                freq *= RandUtils.FloatRange(0.8f, 1.2f, rand);

            }

            if (lg.LinePathNoiseScale > 1)
            {
                freq *= lg.LinePathNoiseScale;
            }

            float amp = RandUtils.FloatRange(0.3f, 0.4f, rand);
            int octaves = 2;
            float pers = RandUtils.FloatRange(0.3f, 0.4f, rand);
            float[,] offsets2 = _noiseService.Generate(pers, freq, amp, octaves, rand.Next(), length + 1, length + 1);
            if (offsets2 != null && offsets2.Length > length)
            {
                for (int x = 0; x < length + 1; x++)
                {
                    offsets2[x, x] *= lg.LinePathNoiseScale * length;
                    float distPctFromEnd = Math.Min(1.0f * x / length, (length - x) * 1.0f / length);
                    offsets2[x, x] *= distPctFromEnd;
                    int newOffset = (int)offsets2[x, x];
                    if (x > 0)
                    {
                        offsets[x] = MathUtil.Clamp(offsets[x - 1] - 1, newOffset, offsets[x - 1] + 1);
                    }
                    else
                    {
                        offsets[x] = newOffset;
                    }
                    offsets[x] = MathUtil.Clamp(-x, offsets[x], x);
                    offsets[x] = MathUtil.Clamp(-(length + 1 - x), offsets[x], length + 1 - x);
                }
            }
        }

        int correctWidthPos = sw;
        int startWidthPos = 0;
        int endWidthPos = 0;

        int currWidthPos = sw;

        List<Point2I> oldPos = new List<Point2I>();

        oldPos.Add(new Point2I(sw, sw));

        int index = 0;
        for (int l = sl; l != el; l += dl, index++)
        {
            bool canShift = Math.Abs(l - sl) >= lg.InitialNoPosShiftLength;

            remainder += width;
            startWidthPos = currWidthPos;
            if (remainder >= length)
            {
                remainder -= length;
                correctWidthPos += dw;
                currWidthPos += dw;
            }
            endWidthPos = currWidthPos;

            // Possibly divert the path to left or right.

            if (lg.WidthPosShiftChance >= rand.NextDouble() && canShift)
            {
                int skewAmount = rand.Next(0, lg.WidthPosShiftSize + 1);
                if (rand.NextDouble() >= 0.5)
                {
                    skewAmount = -skewAmount;
                }
                startWidthPos += skewAmount;
                endWidthPos += skewAmount;
                currWidthPos += skewAmount;
            }

            if (widthPosDrift < 0 && rand.NextDouble() < -widthPosDrift)
            {
                startWidthPos--;
                endWidthPos--;
                currWidthPos--;
            }
            else if (widthPosDrift > 0 && rand.NextDouble() < widthPosDrift)
            {

                startWidthPos++;
                endWidthPos++;
                currWidthPos++;
            }

            // Push the path back to its correct position along the width.
            int widthPosDelta = 0;
            int widthPosError = currWidthPos - correctWidthPos;
            float snapToCenterChance = 0.02f;
            if (length > 0)
            {
                snapToCenterChance = 0.1f / length;
            }
            int absWidthPosError = Math.Abs(widthPosError);
            int signWidthPosError = Math.Sign(widthPosError);
            if (rand.NextDouble() < snapToCenterChance * absWidthPosError)
            {
                widthPosDelta = -signWidthPosError;
            }

            currWidthPos += widthPosDelta;
            startWidthPos += widthPosDelta;
            endWidthPos += widthPosDelta;

            // Now do a final correction toward the final destination

            float errorGapMult = 0.3f;
            int finalErrorGapToEnd = Math.Abs(l - el);
            int errorGapToEnd = (int)(errorGapMult * Math.Abs(l - el));
            if (absWidthPosError > errorGapToEnd && rand.NextDouble() < errorGapMult ||
                absWidthPosError > finalErrorGapToEnd)
            {
                int widthPosGap = absWidthPosError - errorGapToEnd;
                if (widthPosGap > 2)
                {
                    widthPosGap = 2;
                }

                widthPosGap *= -signWidthPosError;

                currWidthPos += widthPosGap;
                startWidthPos += widthPosGap;
                endWidthPos += widthPosGap;
            }

            // Make path wider or narrower.

            if (lg.WidthSizeChangeChance >= rand.NextDouble())
            {
                int size = Math.Max(1, lg.WidthSizeChangeAmount);
                int delta = rand.Next(-size, size + 1);
                pathWidth = MathUtil.Clamp(lg.MinWidthSize, pathWidth + delta, lg.MaxWidthSize);
            }

            // Now make path return to its normal size slowly.
            if (pathWidth > lg.WidthSize && rand.NextDouble() < 0.1 * (pathWidth - lg.WidthSize))
            {
                pathWidth--;
            }

            if (pathWidth < lg.WidthSize && rand.NextDouble() < 0.1 * (lg.WidthSize - pathWidth))
            {
                pathWidth++;
            }

            if (pathWidth > 1)
            {
                endWidthPos += pathWidth / 2;
                startWidthPos -= pathWidth / 2;

                if (pathWidth % 2 != 0)
                {
                    if (addToEndOnEvenWidth)
                    {
                        endWidthPos++;
                    }
                    else
                    {
                        startWidthPos--;
                    }
                }

            }

            // Ellipse shape from minWidth to MaxWidth in center along path and then back down to MinWidth.
            if (lg.UseOvalWidth && length > 6)
            {
                float distFromCenter = Math.Abs(l - cl);
                double currWidth = lg.MinWidthSize +
                                    Math.Sqrt(Math.Max(0.1f, (1 - distFromCenter * distFromCenter / (length / 2.0f * (length / 2.0f))) * widthDelta * widthDelta));
                pathWidth = (int)currWidth;
            }

            if (oldPos.Count > 0)
            {
                Point2I prevPos = oldPos[oldPos.Count - 1];

                if (endWidthPos < prevPos.X)
                {
                    endWidthPos = prevPos.X;
                }

                if (startWidthPos > prevPos.Z)
                {
                    startWidthPos = prevPos.Z;
                }

                int biggestPrevStart = -100000000;
                int smallestPrevEnd = 100000000;

                for (int c = oldPos.Count - 1; c >= 0 && c >= oldPos.Count - lg.MinOverlap - 1; c--)
                {
                    int pstart = oldPos[c].X;
                    int pend = oldPos[c].Z;

                    if (pstart > biggestPrevStart)
                    {
                        biggestPrevStart = pstart;
                    }

                    if (pend < smallestPrevEnd)
                    {
                        smallestPrevEnd = pend;
                    }
                }

                int lowOverlap = endWidthPos + 1 - biggestPrevStart;
                int highOverlap = smallestPrevEnd + 1 - startWidthPos;


                if (lowOverlap > highOverlap && lowOverlap < lg.MinOverlap)
                {
                    endWidthPos += lg.MinOverlap - lowOverlap;
                }
                else if (highOverlap < lg.MinOverlap)
                {
                    startWidthPos -= lg.MinOverlap - highOverlap;
                }

                int oldDiff = endWidthPos - startWidthPos;
                if (oldDiff > 5)
                {
                    oldDiff = 5;
                }

                if (startWidthPos < lg.MinX)
                {
                    startWidthPos = lg.MinX;
                    endWidthPos = startWidthPos + oldDiff;
                }
                if (endWidthPos > lg.MaxX)
                {
                    endWidthPos = lg.MaxX;
                    startWidthPos = endWidthPos - oldDiff;
                }
            }

            for (int w = startWidthPos; w <= endWidthPos; w++)
            {
                LineCell pt = null;


                bool isCenter = w == (startWidthPos + endWidthPos) / 2;
                bool isEdge = (w == startWidthPos || w == endWidthPos);

                if (xAxis)
                {
                    retval.Add(new LineCell(l, w + offsets[index], isCenter, isEdge));

                }
                else
                {
                    retval.Add(new LineCell(w + offsets[index], l, isCenter, isEdge));
                }
            }
            oldPos.Add(new Point2I(startWidthPos, endWidthPos));
        }
        return retval;
    }
    public List<LineCell> GetBressenhamCircle(Point2I center, LineGenParameters pars)
    {
        List<LineCell> retval = new List<LineCell>();
        if (center == null || pars == null)
        {
            return retval;
        }

        int numSegments = 16;

        MyRandom circRand = new MyRandom(pars.Seed);

        pars.MaxWidthSize = 1;



        for (int i = 0; i < numSegments; i++)
        {
            double startx = center.X + pars.XRadius * Math.Cos(1.0f * i / numSegments * Math.PI * 2);
            double starty = center.Z + pars.ZRadius * Math.Sin(1.0f * i / numSegments * Math.PI * 2);
            double endx = center.X + pars.XRadius * Math.Cos(1.0f * (i + 1) / numSegments * Math.PI * 2);
            double endy = center.Z + pars.ZRadius * Math.Sin(1.0f * (i + 1) / numSegments * Math.PI * 2);
            Point2I startPt = new Point2I((int)startx, (int)starty);
            Point2I endPt = new Point2I((int)endx, (int)endy);
            pars.Seed = circRand.Next();
            pars.MaxWidthPosDrift = 3;
            List<LineCell> newPts = GetBressenhamLine(startPt, endPt, pars);
            if (newPts != null)
            {
                foreach (LineCell item in newPts)
                {
                    retval.Add(item);
                }
            }
        }

        return retval;
    }

    public List<ConnectedPairData> ConnectPoints(List<ConnectPointData> points, IRandom rand, float extraConnectionPct = 0.0f)
    {
        if (points == null || points.Count < 1 || rand == null)
        {
            return new List<ConnectedPairData>();
        }

        int nextConnectSet = 1;

        List<ConnectedPairData> allPairs = new List<ConnectedPairData>();

        for (int p1 = 0; p1 < points.Count; p1++)
        {
            for (int p2 = p1 + 1; p2 < points.Count; p2++)
            {
                ConnectedPairData cpd = new ConnectedPairData();
                cpd.Point1 = points[p1];
                cpd.Point2 = points[p2];
                double dx = cpd.Point1.X - cpd.Point2.X;
                double dy = cpd.Point1.Z - cpd.Point2.Z;
                cpd.Distance = Math.Sqrt(dx * dx + dy * dy);
                allPairs.Add(cpd);
            }
        }

        allPairs = allPairs.OrderBy(x => x.Distance).ToList();

        List<ConnectedPairData> remainingPairs = new List<ConnectedPairData>(allPairs);

        List<ConnectedPairData> finalConnections = new List<ConnectedPairData>();


        foreach (ConnectedPairData pair in allPairs)
        {
            ConnectPointData center1 = pair.Point1;
            ConnectPointData center2 = pair.Point2;

            if ((center1.MaxConnections > 0 && center1.Adjacencies.Count >= center1.MaxConnections) ||
                (center2.MaxConnections > 0 && center2.Adjacencies.Count >= center2.MaxConnections))
            {
                continue;
            }

            if (center1.ConnectSet == 0 && center2.ConnectSet == 0)
            {
                center1.ConnectSet = nextConnectSet;
                center2.ConnectSet = nextConnectSet;
                nextConnectSet++;
            }
            else if (center1.ConnectSet == 0 && center2.ConnectSet > 0)
            {
                center1.ConnectSet = center2.ConnectSet;
            }
            else if (center2.ConnectSet == 0 && center1.ConnectSet > 0)
            {
                center2.ConnectSet = center1.ConnectSet;
            }
            else if (center1.ConnectSet != center2.ConnectSet)
            {
                // Set the zoneSet to the min value.

                int maxValue = Math.Max(center1.ConnectSet, center2.ConnectSet);
                int minValue = Math.Min(center1.ConnectSet, center2.ConnectSet);

                // Loop over all centers and set their ZoneSet to the min value.
                foreach (ConnectPointData point in points)
                {
                    if (point.ConnectSet == maxValue)
                    {
                        point.ConnectSet = minValue;
                    }
                }
            }
            else // Same component. Keep it in remaining roads but don't make it now.
            {
                continue;
                // Do nothing.
            }

            finalConnections.Add(pair);
            remainingPairs.Remove(pair);
            center1.Adjacencies.Add(center2);
            center2.Adjacencies.Add(center1);


            if (pair.Distance < center1.MinDistToOther)
            {
                center1.MinDistToOther = pair.Distance;
            }
            if (pair.Distance < center2.MinDistToOther)
            {
                center2.MinDistToOther = pair.Distance;
            }
        }
        int midRoadsToAdd = (int)(finalConnections.Count * extraConnectionPct);

        int maxRoadsToAdd = RandUtils.IntRange(midRoadsToAdd / 2, midRoadsToAdd * 3 / 2, rand);

        for (int i = 0; i < maxRoadsToAdd; i++)
        {
            List<ConnectedPairData> okSecondaryConnections = new List<ConnectedPairData>();

            foreach (ConnectedPairData pair in remainingPairs)
            {
                if (IsOkSecondaryConnection(pair, finalConnections, points))
                {
                    okSecondaryConnections.Add(pair);
                }
            }

            if (okSecondaryConnections.Count < 1)
            {
                break;
            }

            ConnectedPairData newSecondaryConnection = okSecondaryConnections[rand.Next() % okSecondaryConnections.Count];

            ConnectPointData point1 = newSecondaryConnection.Point1;
            ConnectPointData point2 = newSecondaryConnection.Point2;
            finalConnections.Add(newSecondaryConnection);
            okSecondaryConnections.Remove(newSecondaryConnection);
            point1.Adjacencies.Add(point2);
            point2.Adjacencies.Add(point1);
            remainingPairs = okSecondaryConnections;
        }

        return finalConnections;
    }


    private bool IsOkSecondaryConnection(ConnectedPairData pair, List<ConnectedPairData> allPairs, List<ConnectPointData> points)
    {
        float extraDistMult = 2.5f;
        float legSumMult = 1.2f; // Sum of lengths of road legs need to be at least this times the direct distance.

        ConnectPointData c1 = pair.Point1;
        ConnectPointData c2 = pair.Point2;


        if (pair.Distance > extraDistMult * c1.MinDistToOther)
        {
            return false;
        }

        if (pair.Distance > extraDistMult * c2.MinDistToOther)
        {
            return false;
        }


        // Get everything connected to c1.
        List<ConnectedPairData> firstConnList = allPairs.Where(x => x.Point1 == c1 || x.Point2 == c1).ToList();

        // Iterate over all of those connections to get adjacent points that are not c2.
        foreach (ConnectedPairData firstConn in firstConnList)
        {
            ConnectPointData centerPoint = null;

            if (firstConn.Point1 != c1 && firstConn.Point1 != c2)
            {
                centerPoint = firstConn.Point1;
            }
            else if (firstConn.Point2 != c1 && firstConn.Point2 != c2)
            {
                centerPoint = firstConn.Point2;
            }

            if (centerPoint == null)
            {
                continue;
            }

            // Now find everything connected to otherPoint that's also connected to c2.

            foreach (ConnectedPairData secondConn in allPairs)
            {
                ConnectPointData otherPoint = null;
                if (secondConn.Point1 == c2 && secondConn.Point2 != c1)
                {
                    otherPoint = secondConn.Point2;
                }
                else if (secondConn.Point2 == c2 && secondConn.Point1 != c1)
                {
                    otherPoint = secondConn.Point1;
                }

                if (otherPoint == null)
                {
                    continue;
                }

                double distSum = firstConn.Distance + secondConn.Distance;

                // If the sums of the legs of these two roads is less than legSumMult (1.2 or so) of the
                // distance between the two points, then the two connections are too close to where the
                // new road will go, so disallow it.
                if (distSum < pair.Distance * legSumMult)
                {
                    return false;
                }

            }
        }

        return true;

    }

    public List<Point2I> GridConnect(int sx, int sz, int ex, int ez, bool xFirst)
    {
        List<Point2I> points = new List<Point2I>();

        points.Add(new Point2I(sx, sz));

        if (sx == ex && sz == ez)
        {
            return points;
        }


        points.Add(new Point2I(ex, ez));

        int mx = (xFirst ? ex : sx);
        int mz = (xFirst ? sz : ez);

        points.Add(new Point2I(mx, mz));

        // Loop along X.
        if (ex != sx)
        {
            int dx = Math.Sign(ex - sx);
            for (int x = sx; x != ex; x += dx)
            {
                points.Add(new Point2I(x, mz));
            }
        }

        if (ez != sz)
        {
            int dz = Math.Sign(ez - sz);
            for (int z = sz; z != ez; z += dz)
            {
                points.Add(new Point2I(mx, z));
            }
        }

        return points;
    }

    public List<Point2I> GetRotatedEllipse(float cx, float cz, float rx, float rz, float angleDegrees)
    {

        List<Point2I> retval = new List<Point2I>();
        float angleRad = angleDegrees * (MathF.PI / 180.0f);
        float cosA = MathF.Cos(angleRad);
        float sinA = MathF.Sin(angleRad);

        // Calculate bounding box half-extents
        float width = MathF.Sqrt(MathF.Pow(rx * cosA, 2) + MathF.Pow(rz * sinA, 2));
        float height = MathF.Sqrt(MathF.Pow(rx * sinA, 2) + MathF.Pow(rz * cosA, 2));

        int xStart = (int)MathF.Floor(cx - width);
        int xEnd = (int)MathF.Ceiling(cx + width);
        int zStart = (int)MathF.Floor(cz - height);
        int zEnd = (int)MathF.Ceiling(cz + height);

        for (int z = zStart; z <= zEnd; z++)
        {
            for (int x = xStart; x <= xEnd; x++)
            {
                // Relativize to center
                float dx = x - cx;
                float dy = z - cz;

                // Rotate point back to axis-aligned space
                float xRot = dx * cosA + dy * sinA;
                float zRot = -dx * sinA + dy * cosA;

                // Standard ellipse check
                if ((xRot * xRot) / (rx * rx) + (zRot * zRot) / (rz * rz) <= 1.0f)
                {
                    retval.Add(new Point2I(x, z));
                }
            }
        }

        return retval;
    }


    private static readonly List<Point2I> _gridOffsets = new List<Point2I>()
    {
        new Point2I(0,1),
        new Point2I(0,-1),
        new Point2I(1,0),
        new Point2I(-1,0),
    };

    public int[,] GetConnectedComponents(IComponentGrid grid)
    {
        int[,] labels = new int[grid.Width,grid.Height];
        int currentLabel = 0;

        // Initialize all labels to 0 (unlabeled)
        // 0 will mean background/false, >= 1 will be component IDs
        for (int x = 0; x < grid.Width; x++)
        {
            for (int z = 0; z < grid.Height; z++)
            {
                // If it's part of a set and hasn't been labeled yet
                if (grid.HasCell(x,z) && labels[x,z] == 0)
                {
                    currentLabel++;
                    FloodFill4Way(grid, labels, x, z, currentLabel);
                }
            }
        }

        return labels;
    }



    private static void FloodFill4Way(IComponentGrid grid, int[,] labels, int cx, int cz, int currentLabel)
    {
        Queue<Point2I> queue = new Queue<Point2I>();

        labels[cx, cz] = currentLabel;
        queue.Enqueue(new Point2I(cx,cz));

        while (queue.Count > 0)
        {
            Point2I current = queue.Dequeue();
            cx = current.X;
            cz = current.Z;

            foreach (Point2I offset in _gridOffsets)
            {
                int nx = cx + offset.X;
                int nz = cz + offset.Z;

                bool hasCell = grid.HasCell(nx, nz);
                int label = labels[nx, nz];
                bool isConnected = grid.IsConnectedTo(cx, cz, nx, nz);

                if (hasCell && label == 0 && isConnected)
                {
                    labels[nx, nz] = currentLabel;
                    queue.Enqueue(new Point2I(nx, nz));
                }
            }
        }
    }
}