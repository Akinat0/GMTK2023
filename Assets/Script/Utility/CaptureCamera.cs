using UnityEngine;

namespace Abu.Tools
{
    public static class CaptureUtility
    { 
        public static Texture2D Capture(SpriteRenderer renderer, bool inOtherLayer = true)
        {
            int prevSpriteLayer = renderer.gameObject.layer;

            Vector2 spriteScale = new Vector2(renderer.transform.lossyScale.x, renderer.transform.lossyScale.y);

            Camera camera = CreateCameraForSprite(renderer);

            if (inOtherLayer)
            {
                renderer.gameObject.layer = LayerMask.NameToLayer("RenderTexture");
                camera.cullingMask = 1 << LayerMask.NameToLayer("RenderTexture");
            }

            int textureWidth = Mathf.CeilToInt(renderer.sprite.rect.width * spriteScale.x);
            int textureHeight = Mathf.CeilToInt(renderer.sprite.rect.height * spriteScale.y);
            
            RenderTexture renderTexture = RenderTexture.GetTemporary(textureWidth, textureHeight);
            
            camera.targetTexture = renderTexture;
            camera.Render();

            Texture2D texture = ToTexture2D(camera.targetTexture);
            
            Object.DestroyImmediate(camera.gameObject);
            RenderTexture.ReleaseTemporary(renderTexture);

            renderer.gameObject.layer = prevSpriteLayer;

            return texture;
        }

        public static Camera CreateCameraForSprite(SpriteRenderer renderer)
        {
            Rect spriteRect = SpriteRectInWorld(renderer);
            
            Camera camera = new GameObject(renderer.name + "_RenderCamera").AddComponent<Camera>();
            
            // Vector2 spriteScale = new Vector2(renderer.transform.lossyScale.x, renderer.transform.lossyScale.y);
            
            camera.orthographic = true;
            camera.orthographicSize = spriteRect.height / 2;
            
            camera.transform.position = spriteRect.position;
            camera.transform.position += 10 * Vector3.back;

            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;

            return camera;
        }

        public static Camera CreateCameraForMesh(MeshRenderer mesh)
        {
            Bounds bounds = mesh.bounds;

            Camera camera = new GameObject(mesh.name + "_RenderCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = bounds.size.y / 2;
            
            camera.transform.position = mesh.transform.position;
            camera.transform.position += Vector3.back;
            
            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;

            return camera;
        }

        public static Camera CreateCameraForScreen(Camera mainCamera)
        {
            Camera camera = new GameObject( "Screen_RenderCamera").AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = mainCamera.orthographicSize;
            
            camera.transform.position = mainCamera.transform.position;

            camera.backgroundColor = Color.clear;
            camera.clearFlags = CameraClearFlags.SolidColor;

            return camera;
        }
        
        
        public static Rect SpriteRectInWorld(SpriteRenderer spriteRenderer)
        {
            float xPos = spriteRenderer.transform.position.x;
            float yPos = spriteRenderer.transform.position.y;

            float width = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
            float height = spriteRenderer.sprite.rect.height /  spriteRenderer.sprite.pixelsPerUnit;

            width *= spriteRenderer.transform.lossyScale.x;
            height *= spriteRenderer.transform.lossyScale.y;
            
            return new Rect(xPos, yPos, width, height);
        }
        
        
        public static Texture2D ToTexture2D(
            RenderTexture renderTexture,
            Texture2D targetTexture = null,
            TextureFormat? format = null
        )
        {
            if (targetTexture == null)
                targetTexture = new Texture2D(1, 1, format ?? TextureFormat.ARGB32, false);

            int width = renderTexture.width;
            int height = renderTexture.height;

            targetTexture.Reinitialize(width, height, format ?? targetTexture.format, targetTexture.mipmapCount > 1);

            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = renderTexture;

            targetTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);

            RenderTexture.active = prev;

            targetTexture.Apply(false, false);

            return targetTexture;
        }
    }
}