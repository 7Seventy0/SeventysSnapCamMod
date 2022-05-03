using BepInEx;
using System;
using UnityEngine;
using Utilla;
using System.Collections;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.XR;

namespace SeventysSnapCamMod
{
    /// <summary>
    /// This is your mod's main class.
    /// </summary>

    /* This attribute tells Utilla to look for [ModdedGameJoin] and [ModdedGameLeave] */
    [ModdedGamemode]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    public class Plugin : BaseUnityPlugin
    {
        bool inRoom;

        bool isCamInHand;
        GameObject cameraInstance;
        GameObject snapCam;
        GameObject soundeffect;
        void OnEnable()
        {
            /* Set up your mod here */
            /* Code here runs at the start and whenever your mod is enabled*/

            HarmonyPatches.ApplyHarmonyPatches();
            Utilla.Events.GameInitialized += OnGameInitialized;
        }

        void OnDisable()
        {
            /* Undo mod setup here */
            /* This provides support for toggling mods with ComputerInterface, please implement it :) */
            /* Code here runs whenever your mod is disabled (including if it disabled on startup)*/

            HarmonyPatches.RemoveHarmonyPatches();
            Utilla.Events.GameInitialized -= OnGameInitialized;
        }

        IEnumerator SeventysStart()
        {
            var fileStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("SeventysSnapCamMod.Bundle.snapcam");
            var bundleLoadRequest = AssetBundle.LoadFromStreamAsync(fileStream);
            yield return bundleLoadRequest;

            var myLoadedAssetBundle = bundleLoadRequest.assetBundle;
            if (myLoadedAssetBundle == null)
            {
                Debug.Log("Failed to load AssetBundle!");
                yield break;
            }

            var assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>("GorillaCamera");
            yield return assetLoadRequest;

            GameObject camera = assetLoadRequest.asset as GameObject;
            cameraInstance = Instantiate(camera);

            assetLoadRequest = myLoadedAssetBundle.LoadAssetAsync<GameObject>("CameraSoundEffect");
            yield return assetLoadRequest;

             soundeffect = assetLoadRequest.asset as GameObject;
            


            snapCam = GameObject.Find("SnapCam");
            snapCam.AddComponent<SnapCam>();

            ApplyCosmetic();
        }

        IEnumerator Wait()
        {
            yield return new WaitForSeconds(2);
            ApplyCosmetic();
        }

        void ApplyCosmetic()
        {
            cameraInstance.SetActive(true);
            isCamInHand = true;
            GameObject hand = GameObject.Find("palm.01.R");
            cameraInstance.transform.SetParent(hand.transform, false);
            cameraInstance.transform.localPosition = new Vector3(0.04f, 0.08f, - 0.2f);
            cameraInstance.transform.localEulerAngles = new Vector3(348.9304f, 158.7757f, 180);
            cameraInstance.transform.localScale = new Vector3(5, 5, 5);
        }
        void UnApply()
        {
            if(cameraInstance != null)
            {
                cameraInstance.transform.parent = null;
                cameraInstance.transform.position = Vector3.zero;
                cameraInstance.SetActive(false);
               isCamInHand=false;
            }
    
        }

        void OnGameInitialized(object sender, EventArgs e)
        {
            StartCoroutine(SeventysStart());
        }
        float coolDown = 2;
        float nextUseTime = 0;


        float swapCoolDown = 0.3f;
        float nextspawp;

        float turnCoolDown = 2;
        float nextTurn;

        float animationSpeed = 0.3f;
        

        float fov = 60;

        private readonly XRNode rNode = XRNode.RightHand;
        void Update()
        {

            

            if (isCamInHand)
            {
                
                snapCam.GetComponent<Camera>().fieldOfView = fov = Mathf.Clamp(fov, 5 ,160);

                bool trigger;

                InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out trigger);

                if (Time.time > nextUseTime)
                {
                    if (trigger | Keyboard.current.f3Key.wasPressedThisFrame)
                    {
                        Instantiate(soundeffect, cameraInstance.transform.position, Quaternion.identity);
                        snapCam.GetComponent<SnapCam>().TakePic();
                        nextUseTime = Time.time + coolDown;
                    }
                }


                Vector2 stick;
                
                InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out stick);
               
                if (stick.y > 0)
                {
                    fov += -stick.y;
                }
                if (stick.y < 0)
                {
                    fov += Mathf.Abs(stick.y);
                }




                bool sec;

                InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(UnityEngine.XR.CommonUsages.secondaryButton, out sec);
                if (Time.time > turnCoolDown)
                {
                    if (sec)
                    {
                        Debug.Log("Rotating Camera");
                        LeanTween.rotateLocal(cameraInstance,new Vector3(cameraInstance.transform.localEulerAngles.x, cameraInstance.transform.localEulerAngles.y, cameraInstance.transform.localEulerAngles.z +180),animationSpeed)
                            .setEaseInOutBack() ;
                        nextTurn = Time.time + turnCoolDown;
                    }

                }
            }

            
            bool prim;

            InputDevices.GetDeviceAtXRNode(rNode).TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out prim);
            if(Time.time > nextspawp)
            if (prim)
            {
                if (!isCamInHand)
                {
                    ApplyCosmetic();
                }
                else
                {
                    UnApply();
                }
                nextspawp = Time.time+ swapCoolDown;
            }


        }

        /* This attribute tells Utilla to call this method when a modded room is joined */
        [ModdedGamemodeJoin]
        public void OnJoin(string gamemode)
        {
            /* Activate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = true;
        }

        /* This attribute tells Utilla to call this method when a modded room is left */
        [ModdedGamemodeLeave]
        public void OnLeave(string gamemode)
        {
            /* Deactivate your mod here */
            /* This code will run regardless of if the mod is enabled*/

            inRoom = false;
        }
    }
}
