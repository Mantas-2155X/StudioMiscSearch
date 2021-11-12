using System;
using System.Linq;
using System.Collections.Generic;

using BepInEx;
using HarmonyLib;

using Studio;

using UnityEngine;
using UnityEngine.UI;

namespace PH_StudioMiscSearch
{ 
    [BepInProcess("PlayHomeStudio64bit")]
    [BepInProcess("PlayHomeStudio32bit")]
    [BepInPlugin(nameof(PH_StudioMiscSearch), nameof(PH_StudioMiscSearch), VERSION)]
    public class PH_StudioMiscSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.2.0";

        private static readonly List<SearchPackage> searchPackages = new List<SearchPackage>()
        {
            new SearchPackage
            {
                menuPath = "01_Add/03_Map", 
                searchName = "Search Map", 
                viewportY = 35, 
                offsetMin = new Vector2(7, -215),                                                     
                offsetMax = new Vector2(113, -198),
                fixLayout = true
            },
            new SearchPackage
            {
                menuPath = "01_Add/05_Background", 
                searchName = "Search Background", 
                viewportY = -89, 
                offsetMin = new Vector2(7, -107),                                                     
                offsetMax = new Vector2(153, -90)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/00_BGM Player/Scroll View", 
                searchName = "Search BGM", 
                viewportY = -65, 
                offsetMin = new Vector2(7, -83),                                                     
                offsetMax = new Vector2(153, -66)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/01_ENV Player/Bottom Scroll View", 
                searchName = "Search ENV", 
                viewportY = -65, 
                offsetMin = new Vector2(7, -83),                                                     
                offsetMax = new Vector2(153, -66)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/02_Outside Player/Bottom Scroll View", 
                searchName = "Search External", 
                viewportY = -65, 
                offsetMin = new Vector2(7, -83),                                                     
                offsetMax = new Vector2(153, -66)
            }
        };
        
        private void Awake() => Harmony.CreateAndPatchAll(typeof(PH_StudioMiscSearch));

        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), "Awake")]
        private static void Studio_Init_Postfix() => CreateUI();
        
        private static void CreateUI()
        {
            var scene = GameObject.Find("StudioScene");
            if (scene == null)
                return;

            var inputField = scene.transform.Find("Canvas Main Menu/02_Manipulate/02_Light/InputField Intensity");
            if (inputField == null)
                return;

            foreach (var searchPackage in searchPackages)
            {
                var pathTr = scene.transform.Find("Canvas Main Menu/" + searchPackage.menuPath);                                        
                if (pathTr == null)                                                                                         
                    return;                                                                                                
                                                                                                                           
                var viewportRect = pathTr.Find("Viewport").GetComponent<RectTransform>();                                        
                if (viewportRect == null)                                                                                       
                    return;                                                                                                
                
                viewportRect.offsetMin = new Vector2(viewportRect.offsetMin.x, searchPackage.viewportY);                                                  
                
                var inputFieldTr = Instantiate(inputField, pathTr);                                                 
                inputFieldTr.name = searchPackage.searchName;                                                                         
                                                                                  
                Destroy(inputFieldTr.GetComponent<InputFieldToCamera>());
                
                var inputFieldRect = inputFieldTr.GetComponent<RectTransform>();                                          
                inputFieldRect.offsetMin = searchPackage.offsetMin;                                                           
                inputFieldRect.offsetMax = searchPackage.offsetMax;

                inputFieldTr.GetComponent<Image>().color = Color.black;
                
                if (searchPackage.fixLayout)
                    Destroy(pathTr.Find("Viewport/Content").GetComponent<AutoLayoutCtrl>());

                var inputFieldPlaceholderComp = inputFieldTr.Find("Placeholder").GetComponent<Text>();         
                inputFieldPlaceholderComp.text = "Search...";                                                              
                                                                                                                           
                var inputFieldComp = inputFieldTr.GetComponent<InputField>();                                          
                inputFieldComp.contentType = InputField.ContentType.Standard;                                           
                inputFieldComp.inputType = InputField.InputType.Standard;                                               
                inputFieldComp.keyboardType = TouchScreenKeyboardType.Default;                                          
                inputFieldComp.characterValidation = InputField.CharacterValidation.None;                               
                inputFieldComp.characterLimit = 32;                                                                     
                                                                                                                           
                inputFieldComp.text = "";                                                                               
                                                                                                                           
                inputFieldComp.onEndEdit = null;                                                                   
                inputFieldComp.onValidateInput = null;                                                                  
                
                inputFieldComp.onValueChanged = new InputField.OnChangeEvent();
                inputFieldComp.onValueChanged.AddListener(delegate { Search(inputFieldComp); });
            }
        }

        private static void Search(InputField inputField)
        {
            var content = inputField.transform.parent.Find("Viewport/Content");
            
            var gameSceneNodes = content.GetComponentsInChildren<GameSceneNode>(true);
            var studioNodes = content.GetComponentsInChildren<StudioNode>(true);
            
            foreach (var node in gameSceneNodes.Where(node => node != null))
                node.gameObject.SetActive(node.text.Length == 0 || ItemMatchesSearch(node.text, inputField.text));
            
            foreach (var node in studioNodes.Where(node => node != null))
                node.gameObject.SetActive(node.text.Length == 0 || ItemMatchesSearch(node.text, inputField.text));
        }

        private static bool ItemMatchesSearch(string data, string searchStr)
        {
            var searchIn = data;
            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            
            return splitSearchStr.All(s => searchIn.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
        }
    }
    
    public class SearchPackage
    {
        public string menuPath;
        public string searchName;
        
        public Vector2 offsetMin;
        public Vector2 offsetMax;

        public float viewportY;

        public bool fixLayout;
    }
}