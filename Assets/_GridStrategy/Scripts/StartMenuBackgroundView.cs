////////////////////////////////////////////////////////////////////////////////
//
//  StartMenuBackgroundView (c) 2019 Tofunaut
//
//  Created by Nathaniel Ellingson for GridStrategy on 11/09/2019
//
////////////////////////////////////////////////////////////////////////////////

using Tofunaut.SharpUnity;
using Tofunaut.UnityUtils;
using UnityEngine;

namespace Tofunaut.GridStrategy
{
    public class StartMenuBackgroundView : AsyncAssetView
    {
        private SharpCamera _camera;
        private SharpLight _light;
        private GameObject _fogPrefab;
        private GameObject _instantiatedFog;

        public StartMenuBackgroundView() : base (AppManager.AssetManager)
        {
            _camera = new StartMenuBackgroundCamera();
            _light = SharpLight.Sun();
        }

        public override void Load(LoadProgressCallback progressCallback)
        {
            int numCompleted = 0;
            int numToLoad = 1; // update as more assets become necessary
            if(!_assetManager.IsLoaded(typeof(ParticleSystem), AssetPaths.Prefabs.FX.GroundFog))
            {
                _assetManager.Load(AssetPaths.Prefabs.FX.GroundFog, (bool succesful, GameObject payload) =>
                {
                    if (succesful)
                    {
                        _fogPrefab = payload;
                    }
                    numCompleted += 1;
                    progressCallback((float)numCompleted / numToLoad);
                });
            }
            else
            {
                numCompleted += 1;
            }

            progressCallback((float)numCompleted / numToLoad);
        }

        public override void Show()
        {
            _instantiatedFog = Object.Instantiate(_fogPrefab);
            _instantiatedFog.transform.position = new Vector3(-1f, 0f, 0f);

            _camera.Render(null);
            _light.Render(null);
        }

        public override void Destroy()
        {
            _camera.Destroy();
            _light.Destroy();

            Object.Destroy(_instantiatedFog.gameObject);
            _instantiatedFog = null;
        }

        public override void Release()
        {
            _assetManager.Release<ParticleSystem>(AssetPaths.Prefabs.FX.GroundFog);
        }

        private class StartMenuBackgroundCamera : SharpCamera
        {
            public StartMenuBackgroundCamera() : base("StartMenuBackgroundCamera") 
            {
                LocalPosition = new Vector3(0, 1.5f, -10f);
            }

            protected override void Build()
            {
                base.Build();

                _unityCamera.clearFlags = CameraClearFlags.SolidColor;
                _unityCamera.backgroundColor = Color.black;
                _unityCamera.fieldOfView = 11.5f;
            }
        }
    }
}