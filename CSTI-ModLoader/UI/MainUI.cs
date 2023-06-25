using UnityEngine;
using UnityEngine.UI;

namespace ModLoader.UI
{
    public static class MainUI
    {
        public static void CreatePanel()
        {
            var gameObject = new GameObject("[ModLoaderUIBaseCanvas]", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Object.DontDestroyOnLoad(gameObject);
            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var canvasScaler = gameObject.GetComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2 {x = 1920, y = 1080};
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            canvasScaler.referencePixelsPerUnit = 100;
            var gameObjectTransform = gameObject.transform;
            var background = new GameObject("MainUIBackGround", typeof(RectTransform),
                typeof(CanvasRenderer), typeof(Image));
            background.transform.SetParent(gameObjectTransform);
            var image = background.GetComponent<Image>();
            ModLoader.MainUIBackPanel = image;
            ModLoader.MainUIBackPanelRT = background.GetComponent<RectTransform>();
            image.color = new Color(0.4f, 0.9f, 1, 0.4f);
            image.raycastTarget = true;
            image.maskable = true;
            image.type = Image.Type.Sliced;
            image.fillCenter = true;
        }
    }
}