using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Studio;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace HS2_StudioMiscSearch
{
    public static class Tools
    {
        public enum SearchType
        {
            Map,
            Background,
            BGM,
            ENV,
            External,
        }

        private static readonly List<SearchPackage> searchPackages = new List<SearchPackage>()
        {
            new SearchPackage
            {
                menuPath = "01_Add/03_Map", 
                searchName = "Search Map", 
                searchType = SearchType.Map, 
                viewportY = 40, 
                offsetMin = new Vector2(9, -269),                                                     
                offsetMax = new Vector2(141, -252)
            },
            new SearchPackage
            {
                menuPath = "01_Add/05_Background", 
                searchName = "Search Background", 
                searchType = SearchType.Background, 
                viewportY = 30, 
                offsetMin = new Vector2(9, -133),                                                     
                offsetMax = new Vector2(191, -116)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/00_BGM Player/Scroll View", 
                searchName = "Search BGM", 
                searchType = SearchType.BGM, 
                viewportY = -85, 
                offsetMin = new Vector2(9, -103),                                                     
                offsetMax = new Vector2(191, -86)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/01_ENV Player/Bottom Scroll View", 
                searchName = "Search ENV", 
                searchType = SearchType.ENV, 
                viewportY = -85, 
                offsetMin = new Vector2(9, -103),                                                     
                offsetMax = new Vector2(191, -86)
            },
            new SearchPackage
            {
                menuPath = "03_Sound/02_Outside Player/Bottom Scroll View", 
                searchName = "Search External", 
                searchType = SearchType.External, 
                viewportY = -85, 
                offsetMin = new Vector2(9, -103),                                                     
                offsetMax = new Vector2(191, -86)
            }
        };
        
        public static void CreateUI()
        {
            var scene = GameObject.Find("StudioScene");
            if (scene == null)
                return;

            var inputField = scene.transform.Find("Canvas Main Menu/04_System/02_Option/Option/Viewport/Content/Camera/Rot Speed X/InputField");
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
                
                var inputFieldTr = Object.Instantiate(inputField, pathTr);                                                 
                inputFieldTr.name = searchPackage.searchName;                                                                         
                                                                                                                           
                var inputFieldRect = inputFieldTr.GetComponent<RectTransform>();                                          
                inputFieldRect.offsetMin = searchPackage.offsetMin;                                                           
                inputFieldRect.offsetMax = searchPackage.offsetMax;                                                          
                                                                                                                           
                var inputFieldPlaceholderComp = inputFieldTr.Find("Placeholder").GetComponent<Text>();         
                inputFieldPlaceholderComp.text = "Search...";                                                              
                                                                                                                           
                var inputFieldComp = inputFieldTr.GetComponent<InputField>();                                          
                inputFieldComp.contentType = InputField.ContentType.Standard;                                           
                inputFieldComp.inputType = InputField.InputType.Standard;                                               
                inputFieldComp.keyboardType = TouchScreenKeyboardType.Default;                                          
                inputFieldComp.characterValidation = InputField.CharacterValidation.None;                               
                inputFieldComp.characterLimit = 32;                                                                     
                                                                                                                           
                inputFieldComp.text = "";                                                                               
                                                                                                                           
                inputFieldComp.onValueChanged = null;                                                                   
                inputFieldComp.onValidateInput = null;                                                                  
                                                                                                                           
                inputFieldComp.onEndEdit = new InputField.SubmitEvent();
                inputFieldComp.onEndEdit.AddListener(delegate { Search(inputFieldComp, searchPackage.searchType); });
            }
        }
// removed items don't resize panel and don't come back after init
        private static void Search(InputField inputField, SearchType type)
        {
            var text = inputField.text;
            var obj = GetObjFromSearchType(type);
            var trav = Traverse.Create(obj);
            
            ClearSearch(inputField, type);

            var dict = trav.Field("dicNode").GetValue<Dictionary<int, ListNode>>();
            
            var list = dict.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (ItemMatchesSearch(list[i].Value, text))
                    continue;
                
                Object.Destroy(list[i].Value.gameObject);
                dict.Remove(list[i].Key);
            }
            
            trav.Method("UpdateInfo").GetValue();
        }

        private static void ClearSearch(InputField inputField, SearchType type)
        {
            var obj = GetObjFromSearchType(type);
            var trav = Traverse.Create(obj);

            inputField.text = "";
            
            var dict = trav.Field("dicNode").GetValue<Dictionary<int, ListNode>>();
            
            var list = dict.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
                Object.Destroy(list[i].Value.gameObject);
            
            dict.Clear();
            trav.Method("Init").GetValue();
        }
        
        private static bool ItemMatchesSearch(ListNode data, string searchStr)
        {
            var searchIn = data.text;
            var splitSearchStr = searchStr.Split((char[]) null, StringSplitOptions.RemoveEmptyEntries);
            
            return splitSearchStr.All(s => searchIn.IndexOf(s, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private static object GetObjFromSearchType(SearchType type)
        {
            switch (type)
            {
                case SearchType.Map:
                    return HS2_StudioMiscSearch.mapList;
                case SearchType.Background:
                    return HS2_StudioMiscSearch.backgroundList;
                case SearchType.BGM:
                    return HS2_StudioMiscSearch.bgmControl;
                case SearchType.ENV:
                    return HS2_StudioMiscSearch.envControl;
                case SearchType.External:
                    return HS2_StudioMiscSearch.externalControl;
            }

            return null;
        }
    }

    public class SearchPackage
    {
        public string menuPath;
        public string searchName;
        
        public Tools.SearchType searchType;

        public Vector2 offsetMin;
        public Vector2 offsetMax;

        public float viewportY;
    }
}