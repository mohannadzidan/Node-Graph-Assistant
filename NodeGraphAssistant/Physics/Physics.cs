using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Physics
{
    public static bool drawColliders = false;
    public static List<Collider> colliders = new List<Collider>();
    public static Vector2 lastRaycastPoint;
    public static bool Pointcast(Vector2 point, out Collider collider)
    {
        lastRaycastPoint = point;
        for (int i = colliders.Count - 1; i >= 0; i--)
        {

            if (colliders[i].RecivesRaycasts && colliders[i].Raycast(point))
            {
                collider = colliders[i];
                return true;
            }
        }
        collider = null;
        return false;
    }
    public static Collider Pointcast(Vector2 point)
    {
        lastRaycastPoint = point;
        for (int i = colliders.Count - 1; i >= 0; i--)
        {

            if (colliders[i].RecivesRaycasts && colliders[i].Raycast(point))
            {
                return colliders[i];
            }
        }
        return null;
    }
    public static Collider Pointcast<T>(Vector2 point, bool skipDifferentTypes = true)
    {
        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            if (colliders[i].RecivesRaycasts && colliders[i].Raycast(point))
            {
                if (colliders[i].Drawable.GetType() == typeof(T))
                {
                    return colliders[i];

                }
                else if (!skipDifferentTypes) break;
            }
        }
        return null;
    }
    public static bool Rectcast(RectangleF rect, out Collider collider)
    {
        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            if (colliders[i].RecivesRaycasts)
            {
                RectangleF colliderRect = colliders[i].GetBounds();
                colliderRect.Location += Program.canvas.Translation + colliders[i].Drawable.GetGlobalPosition();
                rect.Contains(ref colliderRect, out bool res);
                if (res)
                {
                    collider = colliders[i];
                    return true;
                }
            }
        }
        collider = null;
        return false;
    }
    public static Collider[] RectcastAll<T>(RectangleF rect)
    {
        List<Collider> resColliders = new List<Collider>();

        for (int i = colliders.Count - 1; i >= 0; i--)
        {
            if (colliders[i].RecivesRaycasts && colliders[i].Drawable.GetType() == typeof(T))
            {
                RectangleF colliderRect = colliders[i].GetBounds();
                colliderRect.Location += Program.canvas.Translation + colliders[i].Drawable.GetGlobalPosition();

                rect.Contains(ref colliderRect, out bool res);
                if (res)
                {
                    resColliders.Add(colliders[i]);
                }
            }
        }
        return resColliders.ToArray();
    }
    public static void DrawColliders(RenderTarget renderTarget, Vector2 translation)
    {
        if (!drawColliders) return;
        foreach (Collider c in colliders)
        {
            c.Draw(renderTarget, translation);
        }
        renderTarget.FillEllipse(new Ellipse(lastRaycastPoint + translation, 2f, 2f), Brushes.Collider);
    }
}
