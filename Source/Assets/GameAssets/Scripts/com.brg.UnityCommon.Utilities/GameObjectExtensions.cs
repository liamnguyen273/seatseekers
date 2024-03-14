using System;
using com.brg.Common.Random;
using System.Collections.Generic;
using UnityEngine;

namespace com.brg.UnityCommon
{
    public static class GameObjectExtensions
    {
        public static GameObject? TraversePath(this GameObject baseGo, bool isRelative, string path)
        {
            if (isRelative)
            {
                if (path is null || path == "" || path == ".")
                {
                    return baseGo;
                }
                return baseGo.transform.Find(path)?.gameObject ?? null;
            }

            var sceneGo = GameObject.Find(path) ?? null;

            if (sceneGo is not null)
            {
                // Validate baseGo and sceneGo is in the same scene;
                if (baseGo.scene != sceneGo.scene) return null;
                return sceneGo;
            }

            return null;
        }
        
        public static string RegeneratePathUpTo(this GameObject go, GameObject baseGo)
        {
            var newPath = "";
            if (go.transform.IsChildOf(baseGo.transform))
            {
                newPath = go.GetRelativePath(baseGo);
            }
            else
            {
                newPath = go.GetFullPath();
            }

            return newPath;
        }
        
        public static string GetFullPath(this GameObject obj)
        {
            var path = obj.name;
            while (obj.transform.parent != null)
            {
                obj = obj.transform.parent.gameObject;
                path = obj.name + "/" + path;
            }
            return path;
        }

        public static string GetRelativePath(this GameObject obj, GameObject parent)
        {
            if (obj.transform == parent.transform) return ".";
            
            var path = "/" + obj.name;
            while (obj.transform.parent != null && obj.transform.parent != parent.transform)
            {
                obj = obj.transform.parent.gameObject;
                path = "/" + obj.name + path;
            }
            return "." + path;
        }
        
        public static IEnumerable<T> GetDirectOrderedChildComponents<T>(this GameObject go) where T : MonoBehaviour
        {
            var tempStacks = new List<T>();
            for (int i = 0; i < go.transform.childCount; ++i)
            {
                var child = go.transform.GetChild(i);
                if (child.GetComponent<T>())
                {
                    tempStacks.Add(child.GetComponent<T>());
                }            
            }

            return tempStacks;
        }

        public static IEnumerable<T> GetDirectOrderedChildComponents<T>(this Transform transform) where T : MonoBehaviour
        {
            return transform.gameObject.GetDirectOrderedChildComponents<T>();
        }

        public static void DeleteAllChildren(this Transform transform)
        {
            var count = transform.childCount;

            var list = new List<Transform>();
            for (int i = 0; i < count; ++i)
            {
                list.Add(transform.GetChild(i));
            }

            foreach (var child in list)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
        
        public static void DeleteAllChildrenImmediately(this Transform transform)
        {
            var count = transform.childCount;

            var list = new List<Transform>();
            for (int i = 0; i < count; ++i)
            {
                list.Add(transform.GetChild(i));
            }

            foreach (var child in list)
            {
                GameObject.DestroyImmediate(child.gameObject);
            }
        }

        public static void SetGOActive(this Component component, bool value)
        {
            component.gameObject.SetActive(value);
        }
    }
}
