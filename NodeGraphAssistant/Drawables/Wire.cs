using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

public class Wire : Drawable
{
    Collider start, end;
    Vector2[] guidePoints;
    Vector2[] wirePoints;
    float length;
    LinearGradientBrush brush;

    public Collider Start { get => start; }
    public Collider End { get => end; }

    public Wire() : base(null)
    {
    }
    public Wire(Collider firstRing, Collider secondRing) : base(null)
    {
        this.start = firstRing;
        this.end = secondRing;

    }
    public override void Draw(RenderTarget renderTarget, Vector2 translation)
    {
        if (start != null && end != null)
        {
            if (brush == null)
            {
                brush = Brushes.MakeLinearGradientBrush(renderTarget, ((NodeRing)start.Drawable).Color, ((NodeRing)end.Drawable).Color);

            }
            guidePoints = ConstructGuidePoints(translation);
            wirePoints = ConstructWirePoints(guidePoints, 10f);
            brush.StartPoint = start.Center + translation + start.Drawable.GetGlobalPosition();
            brush.EndPoint = end.Center + translation + end.Drawable.GetGlobalPosition();
            DrawPolygon(renderTarget, wirePoints, brush);
        }
        else if (start != null)
        {
            Brushes.Wire.Color = ((NodeRing)start.Drawable).Color;
            guidePoints = ConstructGuidePoints(translation);
            wirePoints = ConstructWirePoints(guidePoints, 10f);
            DrawPolygon(renderTarget, wirePoints, Brushes.Wire);
        }
    }

    public void Connect(Collider start)
    {
        if (start.Drawable.GetType() != typeof(NodeRing)) throw new InvalidOperationException("Wire connection only valid between Node rings");
        this.start = start;
        end = null;
    }
    public void Connect(Collider first, Collider second)
    {
        if (start.Drawable.GetType() != typeof(NodeRing) || second.Drawable.GetType() != typeof(NodeRing)) throw new InvalidOperationException("Wire connection only valid between Node rings");
        start = first;
        end = second;
        if (collider == null) collider = new MeshCollider(this, ConstructColliderTriangles(10));
        else ReconstructCollider();
        if (brush != null)
        {
            brush.GradientStopCollection.Dispose();
            brush.Dispose();

        }

    }
    public Vector2[] ConstructGuidePoints(Vector2 translation)
    {

        Vector2[] guidePoints = new Vector2[4];
        //end.Center + translation + end.Drawable.GetGlobalPosition();
        if (start != null)
        {
            guidePoints[0] = start.Center + translation + start.Drawable.GetGlobalPosition();
            guidePoints[1] = guidePoints[0];
            if (((NodeRing)start.Drawable).Direction == Direction.Out) guidePoints[1].X += 50f;
            if (((NodeRing)start.Drawable).Direction == Direction.In) guidePoints[1].X -= 50f;
        }
        if (end != null)
        {
            guidePoints[3] = end.Center + translation + end.Drawable.GetGlobalPosition();
            guidePoints[2] = guidePoints[3];
            if (((NodeRing)end.Drawable).Direction == Direction.Out) guidePoints[2].X += 50f;
            if (((NodeRing)end.Drawable).Direction == Direction.In) guidePoints[2].X -= 50f;
        }
        else
        {
            guidePoints[3] = Input.mousePosition;
            guidePoints[2] = guidePoints[3] + translation + 50f * GetDirection(guidePoints[3], guidePoints[0]);
        }
        length = Vector2.Distance(guidePoints[0], guidePoints[1]);
        length += Vector2.Distance(guidePoints[1], guidePoints[2]);
        length += Vector2.Distance(guidePoints[2], guidePoints[3]);
        return guidePoints;
    }
    float Angle(Vector2 from, Vector2 to)
    {
        throw new NotImplementedException();
        // Vector2
        //return (float)Math.Atan2(to.Y - from.Y, to.X - from.Y);
    }
    Vector2[] ConstructWirePoints(Vector2[] guidePoints, float segmentLength)
    {
        if (guidePoints.Length != 4) throw new Exception("points count must be equal to 4");
        if (length <= 0)
        {
            wirePoints = new Vector2[0];
        }
        float fraq = segmentLength / length;
        Vector2 previousPoint = guidePoints[0];
        List<Vector2> wirePointsList = new List<Vector2>();
        wirePointsList.Clear();
        wirePointsList.Add(previousPoint);
        for (float i = 0; i <= 1f; i += fraq)
        {
            Vector2 nextPoint = GetPointOnBezierCurve(guidePoints, i);
            wirePointsList.Add(nextPoint);
            previousPoint = nextPoint;
        }

        wirePointsList.Add(guidePoints[3]);
        return wirePointsList.ToArray();
    }
    public Triangle[] ConstructColliderTriangles(float width)
    {
        width /= 2f;
        Vector2[] guidePoints = ConstructGuidePoints(Vector2.Zero);
        // construct optimized wire points
        Vector2[] colliderWirePoints = ConstructWirePoints(guidePoints, 50f);
        List<Triangle> triangles = new List<Triangle>();
        List<Vector2> trisBases = new List<Vector2>();
        for (int i = 1; i < colliderWirePoints.Length; i++)
        {
            Vector2 dir = GetDirection(colliderWirePoints[i - 1], colliderWirePoints[i]);
            Vector2 perpendicularDir = new Vector2(dir.Y, -dir.X);
            trisBases.Add(colliderWirePoints[i] + perpendicularDir * width);
            trisBases.Add(colliderWirePoints[i] - perpendicularDir * width);
        }
        for (int i = 2; i < trisBases.Count; i += 2)
        {
            triangles.Add(new Triangle(trisBases[i], trisBases[i + 1], trisBases[i - 2]));
            triangles.Add(new Triangle(trisBases[i - 2], trisBases[i + 1], trisBases[i - 1]));

        }
        return triangles.ToArray();
    }
    public void ReconstructCollider()
    {
        if (collider == null) collider = new MeshCollider(this);
        if (start == null || end == null) throw new NullReferenceException("Cannot construct a collider beteween ends that are set to null");
        ((MeshCollider)collider).Triangles = ConstructColliderTriangles(10);
    }
    Vector2 GetPointOnBezierCurve(Vector2[] points, float t)
    {
        Vector2 a = Vector2.Lerp(points[0], points[1], t);
        Vector2 b = Vector2.Lerp(points[1], points[2], t);
        Vector2 c = Vector2.Lerp(points[2], points[3], t);
        Vector2 d = Vector2.Lerp(a, b, t);
        Vector2 e = Vector2.Lerp(b, c, t);
        Vector2 pointOnCurve = Vector2.Lerp(d, e, t);
        return pointOnCurve;
    }
    void DrawPolygon(RenderTarget renderTarget, Vector2[] points, Brush brush)
    {
        for (int i = 1; i < points.Length; i++)
        {
            renderTarget.DrawLine(points[i - 1], points[i], brush, 2f);
        }
    }
    Vector2 GetDirection(Vector2 from, Vector2 to)
    {
        Vector2 dir = to - from;
        dir.Normalize();
        return dir;
    }
    private Vector2 FindCircleCircleIntersections(Vector2 c0, float radius0, Vector2 c1, float radius1)
    {
        Vector2 intersection1, intersection2;
        // Find the distance between the centers.
        float dx = c0.X - c1.X;
        float dy = c0.Y - c1.Y;
        double dist = Math.Sqrt(dx * dx + dy * dy);

        // See how many solutions there are.
        if (dist > radius0 + radius1)
        {
            // No solutions, the circles are too far apart.
            //return new Vector2(float.NaN, float.NaN);
            //return c1 + Direction(c1, c0) * radius1;
            return (c1 + c0) / 2f;
        }
        else if (dist < Math.Abs(radius0 - radius1))
        {
            // No solutions, one circle contains the other.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            Console.WriteLine("contains!");
            return new Vector2(float.NaN, float.NaN);
        }
        else if ((dist == 0) && (radius0 == radius1))
        {
            // No solutions, the circles coincide.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            Console.WriteLine("coincide!");

            return new Vector2(float.NaN, float.NaN);
        }
        else
        {
            // Find a and h.
            double a = (radius0 * radius0 -
                radius1 * radius1 + dist * dist) / (2 * dist);
            double h = Math.Sqrt(radius0 * radius0 - a * a);

            // Find P2.
            double cx2 = c0.X + a * (c1.X - c0.X) / dist;
            double cy2 = c0.Y + a * (c1.Y - c0.Y) / dist;

            // Get the points P3.
            intersection1 = new Vector2(
                (float)(cx2 + h * (c1.Y - c0.Y) / dist),
                (float)(cy2 - h * (c1.X - c0.X) / dist));
            intersection2 = new Vector2(
                (float)(cx2 - h * (c1.Y - c0.Y) / dist),
                (float)(cy2 + h * (c1.X - c0.X) / dist));

            // See if we have 1 or 2 solutions.
            if (dist == radius0 + radius1) return intersection1;
            if (intersection1.Y >= intersection2.Y) return intersection1;
            else return intersection2;
        }
    }


    public override void OnMouseEnter(MouseEventArgs e, Collider collider)
    {
    }

    public override void OnMouseExit(MouseEventArgs e, Collider collider)
    {
    }
    public override void Destroy()
    {
        base.Destroy();
        if (start != null) ((NodeRing)start.Drawable).Connections.Remove(this);
        if (end != null) ((NodeRing)end.Drawable).Connections.Remove(this);
        if (brush != null)
        {
            brush.Dispose();
        }
        Program.MarkCanvasDirty();
    }
    public override void OnMouseDoubleClick(MouseEventArgs e, Collider collider)
    {
        if (e.Button == MouseButtons.Right)
        {
            Destroy();
        }
    }
}

