using HarmonyLib;
using HugMod;
using OWML.Common;
using OWML.Logging;
using OWML.ModHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ModThatDoesNothing
{
    public class ModThatDoesNothing : ModBehaviour
    {
        private AssetBundle grabBag;
        private bool[] activations;
        private System.Random random = new();
        private bool hasDLC;
        private int selectRange, maxNumberOfActivations;
        private int numberBase = 7, numberDLC = 6;

        private IHugModApi hugApi; 

        private void Start()
        {
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            hasDLC = EntitlementsManager.IsDlcOwned() == EntitlementsManager.AsyncOwnershipStatus.Owned;
            var numberMax = numberBase + numberDLC;
            activations = new bool[numberMax];
            selectRange = hasDLC ? numberMax : numberBase;

            grabBag = ModHelper.Assets.LoadBundle("Assets/asset_grab_bag");

            hugApi = ModHelper.Interaction.TryGetModApi<IHugModApi>("VioVayo.HugMod");

            ModHelper.Console.WriteLine($"{nameof(ModThatDoesNothing)} is enabled. Don't even worry about it.", MessageType.Success);

            LoadManager.OnCompleteSceneLoad += (scene, loadScene) =>
            {
                if (loadScene is not (OWScene.SolarSystem or OWScene.EyeOfTheUniverse)) return;

                DecideActivations(maxNumberOfActivations);

                var index1 = 0;
                if (activations[index1]) { TinyfyChert(); activations[index1] = false; }
                ++index1;
                if (activations[index1]) { LongifyGabbro(); activations[index1] = false; }
                ++index1;
                if (activations[index1]) { DeathTrumpet(); activations[index1] = false; }
                ++index1;
                if (activations[index1]) { PartyScout(); activations[index1] = false; } //4

                var index2 = numberBase;
                if (activations[index2]) { LeafyAntlers(); activations[index2] = false; } //1

                if (loadScene != OWScene.SolarSystem) return;

                ++index1;
                if (activations[index1]) { ReplaceCockpitFriend(); activations[index1] = false; }
                //++index1;
                //if (activations[index1]) { EvenFriendlierPosters(); activations[index1] = false; }
                ++index1;
                if (activations[index1]) { CaffeinatedTravellers(); activations[index1] = false; }
                ++index1;
                if (activations[index1]) { OrbSwap(); activations[index1] = false; } //3

                ++index2;
                if (activations[index2]) { FabulousifyCorpses(); activations[index2] = false; }
                ++index2;
                if (activations[index2]) { OwlParty(); activations[index2] = false; }
                ++index2;
                if (activations[index2]) { StrangerParty(); activations[index2] = false; }
                ++index2;
                if (activations[index2]) { DreamFireflies(); activations[index2] = false; }
                ++index2;
                if (activations[index2]) { OwlsWithHats(); activations[index2] = false; } //5
            };
        }

        private void DecideActivations(int number)
        { for (int n = 0; n < number; ++n) activations[random.Next(selectRange)] = true; }

        public override void Configure(IModConfig config)
        {
            var level = config.GetSettingsValue<int>("level");
            maxNumberOfActivations = Convert.ToInt32(level * selectRange / 10);
            if (maxNumberOfActivations == 0) maxNumberOfActivations = 1;
        }



        //-----Possible Effects-----
        private void TinyfyChert() //1
        {
            var tinierChert = 0.5f;
            var chertObj = GameObject.Find("Traveller_HEA_Chert");
            chertObj.transform.localScale = new Vector3(tinierChert, tinierChert, tinierChert);
            //ModHelper.Console.WriteLine("Chert tinified.", MessageType.Success);
        }

        private void LongifyGabbro() //2
        {
            var longGabbro = 1.7f;
            var longGabbroThiccnessMultiplier = 0.27f; //applied to resulting height difference
            var gabbroObj = GameObject.Find("Traveller_HEA_Gabbro_ANIM_IdleFlute");
            gabbroObj.transform.localScale = new Vector3(longGabbroThiccnessMultiplier * (longGabbro - 1) + 1, longGabbroThiccnessMultiplier * (longGabbro - 1) + 1, longGabbro);
            //ModHelper.Console.WriteLine("Gabbro longified.", MessageType.Success);
        }

        private void DeathTrumpet() //3
        {
            GlobalMessenger<DeathType>.AddListener("PlayerDeath", new Callback<DeathType>(PlayDeathTrumpet));
            void PlayDeathTrumpet(DeathType irrelevant)
            {
                var obj = GameObject.Find("PauseMenu");
                obj.SetActive(false);
                var deathTrumpet = obj.AddComponent<OWAudioSource>();
                deathTrumpet.SetTrack(OWAudioMixer.TrackName.Death);
                obj.SetActive(true);
                deathTrumpet.clip = grabBag.LoadAsset<AudioClip>("Assets/GrabBagOfAssets/sad-trombone.mp3");
                deathTrumpet.SetLocalVolume(0.5f);
                GlobalMessenger<DeathType>.RemoveListener("PlayerDeath", new Callback<DeathType>(PlayDeathTrumpet));
            }
            //ModHelper.Console.WriteLine("Trumpet lol.", MessageType.Success);
        }

        private void ReplaceCockpitFriend() //4
        {
            var pottedPlant = GameObject.Find("Props_HEA_ShipFoliage");
            var pottedCactus = Instantiate(GameObject.Find("Props_HGT_Cactus_Single_A_Alt"), GameObject.Find("Ship_Body/Module_Cockpit/Props_Cockpit").transform);
            pottedCactus.transform.localScale = new(0.36f, 0.36f, 0.36f);
            pottedCactus.transform.localPosition = new(-1.65f, 0.4f, 3.9f);
            pottedCactus.transform.localEulerAngles = new(15f, 0, 0);
            Destroy(pottedPlant);
            LoadCactus();

            var streamingRenderMeshHandle = pottedCactus.GetComponentInChildren<StreamingRenderMeshHandle>();
            streamingRenderMeshHandle.OnMeshUnloaded += LoadCactus;

            var collider = pottedCactus.AddComponent<SphereCollider>();
            collider.center = new(0, 1.1f, 0);
            collider.radius = 1.2f;

            if (hugApi != null)
            {
                pottedCactus.AddComponent<OWCollider>();
                hugApi.AddHugComponent(pottedCactus);
                hugApi.SetPrompt(pottedCactus, "Cactus");
                hugApi.SetFocusPoint(pottedCactus, collider.center);
                hugApi.GetInteractReceiver(pottedCactus)._usableInShip = true;
            }

            void LoadCactus() { StreamingManager.LoadStreamingAssets("hourglasstwins/meshes/props"); }
            //ModHelper.Console.WriteLine("Hhhhh cactus.", MessageType.Success);
        }

        private void EvenFriendlierPosters() //5
        {
            var textObjects = GameObject.Find("Ship_Body").GetComponentsInChildren<Transform>()
                .Where(obj => obj.gameObject.name is "OxygenPosterCanvas" or "EnjoyCanvas" or "Scout_Poster_TextCanvases" or "Cabin_Poster_TextCanvases").Select(obj => obj.gameObject);
            foreach (var gameObj in textObjects) gameObj.SetActive(false);

            var materials1 = new List<Material>();
            var materials2 = new List<Material>();
            var materials = Resources.FindObjectsOfTypeAll<Material>();
            foreach (var material in materials)
            {
                var textureName = material?.mainTexture?.name;
                if (textureName == "Structure_HEA_PlayerShip_SignsDecal_d") materials1.Add(material);
                if (textureName == "Structure_HEA_PlayerShip_Posters_d") materials2.Add(material);
            }
            var texture1 = ModHelper.Assets.GetTexture("RawAssets/poster.png");
            var texture2 = ModHelper.Assets.GetTexture("RawAssets/posters.png");
            foreach (var material in materials1) material.mainTexture = texture1;
            foreach (var material in materials2) material.mainTexture = texture2;
            //ModHelper.Console.WriteLine("You got this!", MessageType.Success);
        }

        private void PartyScout() //6
        {
            var scout = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(obj => obj.name == "Probe_Body" && obj.scene == SceneManager.GetActiveScene());
            if (scout == null) return;
            var lightObj = scout.transform.Find("Lantern").gameObject;
            var emissive = lightObj.GetComponent<ProbeLantern>()._emissiveRenderer;
            var light = lightObj.GetComponent<Light>();
            light.color = new(1, 1, 0.2f, 1);
            StartCoroutine(PartyTime(light, emissive, 0.2f, 0.486f));

            var grooveObj = new GameObject("Probe Groove");
            grooveObj.SetActive(false);
            var partyGroove = grooveObj.AddComponent<OWAudioSource>();
            partyGroove.SetTrack(OWAudioMixer.TrackName.Environment);
            grooveObj.SetActive(true);
            partyGroove.clip = grabBag.LoadAsset<AudioClip>("Assets/GrabBagOfAssets/disco-groove.mp3");
            partyGroove.loop = true;
            partyGroove.SetMaxVolume(0);
            StartCoroutine(DelayByAFrame(() =>
            {
                partyGroove.SetMaxVolume(0.5f);
                partyGroove.SetLocalVolume(0);
            }
            ));

            partyGroove.spatialBlend = 1;
            partyGroove.spread = 180;
            partyGroove.dopplerLevel = 0;
            partyGroove.rolloffMode = AudioRolloffMode.Custom;
            var customCurve = new AnimationCurve( //copied from NH who copied from Esker, apparently
                new Keyframe(0.0333f, 1f, -30.012f, -30.012f, 0.3333f, 0.3333f),
                new Keyframe(0.0667f, 0.5f, -7.503f, -7.503f, 0.3333f, 0.3333f),
                new Keyframe(0.1333f, 0.25f, -1.8758f, -1.8758f, 0.3333f, 0.3333f),
                new Keyframe(0.2667f, 0.125f, -0.4689f, -0.4689f, 0.3333f, 0.3333f),
                new Keyframe(0.5333f, 0.0625f, -0.1172f, -0.1172f, 0.3333f, 0.3333f),
                new Keyframe(1f, 0f, -0.0333f, -0.0333f, 0.3333f, 0.3333f));
            partyGroove.SetCustomCurve(AudioSourceCurveType.CustomRolloff, customCurve);
            partyGroove.minDistance = 20;
            partyGroove.maxDistance = 40;

            var probe = scout.GetComponent<SurveyorProbe>();
            probe.OnLaunchProbe += () => 
            {
                grooveObj.transform.SetParent(scout.transform);
                grooveObj.transform.localPosition = Vector3.zero;
                partyGroove.FadeIn(1);
            };
            probe.OnRetrieveProbe += () =>
            {
                grooveObj.transform.SetParent(null, true);
                partyGroove.FadeOut(0.5f, OWAudioSource.FadeOutCompleteAction.CONTINUE);
            };
            //ModHelper.Console.WriteLine("Scout party time!", MessageType.Success);
        }

        private void CaffeinatedTravellers() //7
        {
            var n = random.Next(5);
            var travellerObj = GameObject.Find(n == 4 ? "Villager_HEA_Esker" : "Traveller_HEA_" + (n == 0 ? "Riebeck" : n == 1 ? "Chert" : n == 2 ? "Gabbro" : "Feldspar"));
            var anims = travellerObj.GetComponentsInChildren<Animator>();
            var signalSelect = n == 0 ? SignalName.Traveler_Riebeck : n == 1 ? SignalName.Traveler_Chert : n == 2 ? SignalName.Traveler_Gabbro : n == 3 ? SignalName.Traveler_Feldspar : SignalName.Traveler_Esker;
            StartCoroutine(DelayByAFrame(GiveCaffeine));

            void GiveCaffeine()
            {
                var manager = Locator.GetTravelerAudioManager();
                var signals = manager._signals.Where(obj => obj._name == signalSelect);
                foreach (var signal in signals) signal.GetOWAudioSource()._audioSource.pitch *= 2;
                foreach (var anim in anims) anim.speed *= 2;
                manager.SyncTravelers();
            }
            //ModHelper.Console.WriteLine($"{signalSelect} has zero chill.", MessageType.Success);
        }

        private void OrbSwap() //8
        {
            GameObject glowy = null;
            var renderersGlassOrb = new List<MeshRenderer>();
            var renderersMuseumOrb = new List<MeshRenderer>();
            var meshRenderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                var materialName = renderer.sharedMaterial?.name;
                if (materialName is "Props_NOM_OrbSpecs_mat") glowy ??= renderer.gameObject;
                if (materialName is "Props_NOM_Orb_mat") renderersGlassOrb.Add(renderer);
                if (materialName is "Props_HEA_TideBall_mat") renderersMuseumOrb.Add(renderer);
            }
            var matGlassOrb = renderersGlassOrb[0].sharedMaterial;
            var matMuseumOrb = renderersMuseumOrb[0].sharedMaterial;
            foreach (var renderer in renderersGlassOrb) renderer.material = matMuseumOrb;
            foreach (var renderer in renderersMuseumOrb) 
            {
                Instantiate(glowy, renderer.gameObject.transform);
                renderer.material = matGlassOrb;
            }
            //ModHelper.Console.WriteLine("It's orbin' time.", MessageType.Success);
        }


        //-----DLC Section-----
        private void FabulousifyCorpses() //1
        {
            var mummies = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Mummy_IP_Anim");
            foreach (var mummy in mummies)
            {
                var renderersBody = mummy.GetComponentsInChildren<SkinnedMeshRenderer>().Where(obj => obj.gameObject.name.Contains("Nails") || obj.gameObject.name.Contains("Body"));
                var renderersBling = mummy.GetComponentsInChildren<SkinnedMeshRenderer>().Where(obj => obj.gameObject.name.Contains("Bracelet") || obj.gameObject.name.Contains("Necklace"));
                foreach (var renderer in renderersBody) renderer.material.mainTexture = grabBag.LoadAsset<Texture>("Assets/GrabBagOfAssets/owl-fab.png");
                foreach (var renderer in renderersBling) renderer.material.mainTexture = grabBag.LoadAsset<Texture>("Assets/GrabBagOfAssets/owl-bling.png");
            }
            //ModHelper.Console.WriteLine("Owl corpses fabulousified.", MessageType.Success);
        }

        private void OwlParty() //2
        {
            var circles = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "MummyCircle");
            var circle = circles.ElementAt(random.Next(circles.Count()));

            var lightObj = Instantiate(GameObject.Find("PointLight_HEA_BlueLantern"), circle.transform);
            lightObj.transform.localPosition = new(0, 2, 0);
            lightObj.transform.localScale = new(3, 3, 3);
            var partyLight = lightObj.GetComponent<Light>();
            partyLight.intensity = 1.5f;
            partyLight.color = new(1, 0.2f, 0.2f, 1);
            StartCoroutine(PartyTime(partyLight, null, 0.2f, 0.486f));

            lightObj.SetActive(false);
            var partyGroove = lightObj.AddComponent<OWAudioSource>();
            partyGroove.SetTrack(OWAudioMixer.TrackName.Environment);
            lightObj.SetActive(true);
            partyGroove.clip = grabBag.LoadAsset<AudioClip>("Assets/GrabBagOfAssets/disco-groove.mp3");
            partyGroove.loop = true;
            partyGroove.SetMaxVolume(0);
            StartCoroutine(DelayByAFrame(() =>
            {
                partyGroove.SetMaxVolume(0.5f);
                partyGroove.SetLocalVolume(0);
            }
            ));

            var shape = lightObj.AddComponent<CylinderShape>();
            shape._pointChecksOnly = true;
            shape.center = new(0, 0.9f, 0);
            shape.height = 3.2f;
            shape.radius = 3.5f;
            var volume = lightObj.AddComponent<OWTriggerVolume>();
            volume.OnEntry += (hitObj) =>
            {
                if (!hitObj.CompareTag("PlayerDetector")) return;
                partyGroove.FadeIn(7);
            };
            volume.OnExit += (hitObj) =>
            {
                if (!hitObj.CompareTag("PlayerDetector")) return;
                partyGroove.FadeOut(5, OWAudioSource.FadeOutCompleteAction.CONTINUE);
            };

            var partyAnim = grabBag.LoadAsset<GameObject>("Assets/GrabBagOfAssets/party_animator.prefab").GetComponent<Animator>().runtimeAnimatorController;
            var mummies = circle.GetComponentsInChildren<Transform>().Where(obj => obj.gameObject.name == "Mummy_IP_Anim");
            foreach (var mummy in mummies)
            {
                var animControl = new AnimatorOverrideController();
                var anim = mummy.GetComponent<Animator>();
                animControl.runtimeAnimatorController = partyAnim;
                animControl["placeholder"] = anim.GetCurrentAnimatorClipInfo(0)[0].clip;
                anim.runtimeAnimatorController = animControl;
                anim.enabled = true;
            }
            //ModHelper.Console.WriteLine("Owl party time!", MessageType.Success);
        }

        private void LeafyAntlers() //3
        {
            var birdLeafPrefab = grabBag.LoadAsset<GameObject>("Assets/GrabBagOfAssets/bird_leafs.prefab");
            var renderers = birdLeafPrefab.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers) renderer.material.shader = Shader.Find("Outer Wilds/Environment/Foliage");

            var leafOwlPairs = new List<(Transform, Transform)>();
            var owls = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == "Ghostbird_IP_ANIM");
            foreach (var owl in owls)
            {
                var owlHead = owl.GetComponentsInChildren<Transform>().FirstOrDefault(obj => obj.name == "Ghostbird_Skin_01:Ghostbird_Rig_V01:Head");
                if (owlHead == null) continue;

                var antlers = owl.GetComponentsInChildren<Transform>(false).Where(obj => obj.gameObject.name.Contains("Antler_"));
                var birdLeaf = Instantiate(birdLeafPrefab, owlHead);
                foreach (var antler in antlers) birdLeaf.transform.Find(antler.gameObject.name).gameObject.SetActive(true);

                var leaves = birdLeaf.GetComponentsInChildren<Transform>(false).Where(obj => obj.gameObject.name.Contains("moss"));
                foreach (var leaf in leaves) leafOwlPairs.Add((leaf, owl.transform));
            }
            StartCoroutine(AlignRotations(leafOwlPairs, new(100, 2.5f, 1), new(0, 0, 1), new(0, 1, 0)));
            //ModHelper.Console.WriteLine("Bird leafs!", MessageType.Success);
        }

        private void StrangerParty() //4
        {
            var emissive = GameObject.Find("ArtificialSun_Bulb").GetComponent<OWEmissiveRenderer>();
            var light = GameObject.Find("IP_SunLight").GetComponent<Light>();
            light.color = new(1, 0.5f, 1, 1);
            StartCoroutine(PartyTime(light, emissive, 0.5f, 0.486f));

            light.gameObject.SetActive(false);
            var partyGroove = light.gameObject.AddComponent<OWAudioSource>();
            partyGroove.SetTrack(OWAudioMixer.TrackName.Environment);
            light.gameObject.SetActive(true);
            partyGroove.clip = grabBag.LoadAsset<AudioClip>("Assets/GrabBagOfAssets/disco-groove-muffled.mp3");
            partyGroove.loop = true;
            partyGroove.SetMaxVolume(0);
            StartCoroutine(DelayByAFrame(() =>
            {
                partyGroove.SetMaxVolume(0.7f);
                partyGroove.SetLocalVolume(0);
            }
            ));

            var volume = Locator.GetRingWorldController()._insideRingWorldVolume;
            volume.OnEntry += (hitObj) => 
            {
                if (!hitObj.CompareTag("PlayerDetector")) return;
                partyGroove.FadeIn(2.5f);
            };
            volume.OnExit += (hitObj) => 
            {
                if (!hitObj.CompareTag("PlayerDetector")) return;
                partyGroove.FadeOut(2.5f, OWAudioSource.FadeOutCompleteAction.CONTINUE);
            };
            //ModHelper.Console.WriteLine("So it echoes through the Stranger...", MessageType.Success);
        }

        private void DreamFireflies() //5
        {
            var fireflies = GameObject.Find("Prefab_TH_Fireflies");
            var sectors = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name.Contains("Sector_DreamZone_") || obj.name == "Sector_Underground");
            var positionsPrefab = grabBag.LoadAsset<GameObject>("Assets/GrabBagOfAssets/positions_fireflies.prefab");
            foreach (var sector in sectors)
            {
                var positions = positionsPrefab.transform.Find(sector.name).Cast<Transform>();
                foreach (var position in positions)
                {
                    var firefliesAtPos = Instantiate(fireflies, sector.transform);
                    firefliesAtPos.transform.localPosition = position.localPosition;
                }
            }
            //ModHelper.Console.WriteLine("Lookit the pretty lights!", MessageType.Success);
        }

        private void OwlsWithHats() //6
        {
            var renderers1 = new List<MeshRenderer>();
            var renderers2 = new List<MeshRenderer>();
            var renderers3 = new List<MeshRenderer>();
            var meshRenderers = Resources.FindObjectsOfTypeAll<MeshRenderer>();
            foreach (var renderer in meshRenderers)
            {
                var textureName = renderer.sharedMaterial?.mainTexture?.name;
                if (textureName is "LOD_Decal_IP_GhostPortraits_d" or "LOD_Decal_DW_GhostPortraits_d") renderers1.Add(renderer);
                if (textureName is "LOD_Decal_IP_Murals_SecretAlcove_d") renderers2.Add(renderer);
                if (textureName is "LOD_Decal_IP_TallPictures_d" or "LOD_Decal_DW_TallPictures_d") renderers3.Add(renderer);
            }
            var texture1 = grabBag.LoadAsset<Texture>("Assets/GrabBagOfAssets/portraits.png");
            var texture2 = grabBag.LoadAsset<Texture>("Assets/GrabBagOfAssets/portraits-scary.png");
            var texture3 = grabBag.LoadAsset<Texture>("Assets/GrabBagOfAssets/portraits-tall.png");
            foreach (var renderer in renderers1) renderer.material.mainTexture = texture1;
            foreach (var renderer in renderers2) renderer.material.mainTexture = texture2;
            foreach (var renderer in renderers3) renderer.material.mainTexture = texture3;
            //ModHelper.Console.WriteLine("Fashionable owels~", MessageType.Success);
        }


        //-----Beyond this point there be Coroutines-----
        IEnumerator DelayByAFrame(Action action)
        {
            yield return null;
            action();
        }

        IEnumerator PartyTime(Light partyLight, OWEmissiveRenderer emissive, float brightness, float beatTime)
        {
            emissive?.SetEmissionColor(partyLight.color);
            while (partyLight != null)
            {
                if (!OWTime.IsPaused())
                {
                    var red = partyLight.color.r; var green = partyLight.color.g; var blue = partyLight.color.b;
                    //partyLight.color = new(green != 1 ? 1 : brightness, blue != 1 ? 1 : brightness, red != 1 ? 1 : brightness, 1); //rygcbm
                    var colour = red == green ?
                        new Color(blue != 1 ? 1 : brightness, red != 1 ? 1 : brightness, green != 1 ? 1 : brightness, 1) :
                        new Color(blue == 1 ? 1 : brightness, red == 1 ? 1 : brightness, green == 1 ? 1 : brightness, 1); //rgbcmy
                    partyLight.color = colour;
                    emissive?.SetEmissionColor(colour);
                }
                yield return new WaitForSecondsRealtime(beatTime);
            }
        }

        IEnumerator AlignRotations(List<(Transform, Transform)> objRefPairs, DampedSpringQuat spring, Vector3 localDirectionObject, Vector3 localDirectionReference)
        {
            while (objRefPairs.Count != 0)
            {
                if (!OWTime.IsPaused()) for (int n = objRefPairs.Count - 1; n >= 0; --n)
                {
                    if (objRefPairs[n].Item1 != null && objRefPairs[n].Item2 != null)
                    {
                        var directionObject = objRefPairs[n].Item1.TransformDirection(localDirectionObject);
                        var directionReference = objRefPairs[n].Item2.TransformDirection(localDirectionReference);
                        var toTargetRotation = Quaternion.AngleAxis(Vector3.Angle(directionObject, directionReference), Vector3.Cross(directionObject, directionReference));
                        objRefPairs[n].Item1.rotation = spring.Update(objRefPairs[n].Item1.rotation, toTargetRotation * objRefPairs[n].Item1.rotation, Time.deltaTime);
                    }
                    else objRefPairs.Remove(objRefPairs[n]);
                }
                yield return null;
            }
        }
    }

    [HarmonyPatch]
    public class Patches
    {
        [HarmonyPatch(typeof(UnityLogger))]
        public static class UnityLoggerPatches
        {
            [HarmonyPrefix] //Based on the one NH uses for a different annoying harmless error log, thank you JohnCorby
            [HarmonyPatch("OnLogMessageReceived")]
            public static bool UnityLogger_OnLogMessageReceived(string message)
            {
                // Filter out goofy error that doesn't actually break anything
                return !(message == "Material doesn't have a texture property '_MainTex'");
            }
        }
    }
}