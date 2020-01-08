﻿using BS_Utils.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRUIControls;
using Image = UnityEngine.UI.Image;
using Screen = HMUI.Screen;

namespace BeatSaberMarkupLanguage.FloatingScreen
{
    public class FloatingScreen : Screen
    {
        public FloatingScreenMoverPointer screenMover;
        public GameObject handle;

        public static FloatingScreen CreateFloatingScreen(Vector2 screenSize, bool createHandle, Vector3 position, Quaternion rotation)
        {
            FloatingScreen screen = new GameObject("BSMLFloatingScreen", typeof(FloatingScreen), typeof(CanvasScaler), typeof(RectMask2D), typeof(Image), typeof(VRGraphicRaycaster), typeof(SetMainCameraToCanvas)).GetComponent<FloatingScreen>();

            Canvas canvas = screen.GetComponent<Canvas>();
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2;
            canvas.sortingOrder = 4;

            CanvasScaler scaler = screen.GetComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 3.44f;
            scaler.referencePixelsPerUnit = 10f;

            Image background = screen.GetComponent<Image>();
            background.sprite = Resources.FindObjectsOfTypeAll<Sprite>().First(x => x.name == "MainScreenMask");
            background.type = Image.Type.Sliced;
            background.color = new Color(0.7450981f, 0.7450981f, 0.7450981f, 1f);
            background.material = Resources.FindObjectsOfTypeAll<Material>().First(x => x.name == "UIFogBG");
            background.preserveAspect = true;

            SetMainCameraToCanvas setCamera = screen.GetComponent<SetMainCameraToCanvas>();
            setCamera.SetPrivateField("_canvas", canvas);
            setCamera.SetPrivateField("_mainCamera", Resources.FindObjectsOfTypeAll<MainCamera>().FirstOrDefault());

            screen.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            (screen.transform as RectTransform).sizeDelta = screenSize;

            if (createHandle)
            {
                VRPointer[] vrPointers = Resources.FindObjectsOfTypeAll<VRPointer>();
                if (vrPointers.Count() != 0)
                {
                    VRPointer pointer = vrPointers.First();
                    if (screen.screenMover) Destroy(screen.screenMover);
                    screen.screenMover = pointer.gameObject.AddComponent<FloatingScreenMoverPointer>();

                    screen.handle = GameObject.CreatePrimitive(PrimitiveType.Cube);

                    screen.handle.transform.SetParent(screen.transform);
                    screen.handle.transform.localPosition = new Vector3(-screenSize.x / 2f, 0f, 0f);
                    screen.handle.transform.localScale = new Vector3(screenSize.x / 15f, screenSize.y * 0.8f, screenSize.x / 15f);

                    screen.screenMover.Init(screen);
                }
                else
                {
                    Logger.log.Warn("Failed to get VRPointer!");
                }
            }

            screen.transform.position = position;
            screen.transform.rotation = rotation;

            return screen;
        }

    }
}
