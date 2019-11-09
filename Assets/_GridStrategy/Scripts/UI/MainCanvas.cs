using Tofunaut.SharpUnity.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Tofunaut.GridStrategy.UI
{
    public class MainCanvas : SharpCanvas
    {
        public static MainCanvas Instance { get; private set; }

        public static Vector2 ReferenceResolution { get { return new Vector2(1920, 1080); } }

        private MainCanvas() : base("Main Canvas") { }

        public static void Create()
        {
            if(Instance != null)
            {
                return;
            }

            Instance = new MainCanvas();
            Instance.Render(AppManager.Transform);

            Instance._unityCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            Instance._unityCanvas.planeDistance = 100;

            Instance._unityCanvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Instance._unityCanvasScaler.referenceResolution = ReferenceResolution;
            Instance._unityCanvasScaler.referencePixelsPerUnit = 100;

            Instance._unityGraphicRaycaster.ignoreReversedGraphics = true;
            Instance._unityGraphicRaycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;

            Instance.SetMatchWidthOrHeight(1f);
        }
    }
}