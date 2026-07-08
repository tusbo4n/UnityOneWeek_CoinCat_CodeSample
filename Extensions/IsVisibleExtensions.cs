using UnityEngine;

namespace Extention2D
{
    //gameObject.IsVisibleFrom(camera) で取得できる
    public static class CameraExtensions
    {
        public static bool IsVisibleFrom(this Renderer renderer, Camera camera)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
            return GeometryUtility.TestPlanesAABB(planes, renderer.bounds);
        }
    }

    public static class GameObjectExtensions
    {
        public static bool IsVisibleFrom(this GameObject gameObject, Camera camera)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            if (renderers != null)
            {
                foreach (Renderer r in renderers)
                {
                    if (r.IsVisibleFrom(camera))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
