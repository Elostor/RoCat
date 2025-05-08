using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingleton<T> : MonoBehaviour where T : Component
{
   protected static T _instance;
   protected bool _isEnabled;

   /// A singleton pattern.
   public static T Instance
   {
       get
       {
           if (!_instance)
           {
               _instance = FindObjectOfType<T>();
               if (!_instance)
               {
                   GameObject obj = new GameObject();
                   _instance = obj.AddComponent<T>();
               }
           }
           return _instance;
       }
   }

   /// If there's already a copy of a specific object, this destroys new copies.
   protected virtual void Awake ()
   {
       if (!Application.isPlaying)
       {
           return;
       }

       if (!_instance)
       {
           _instance = this as T;
           DontDestroyOnLoad (gameObject);
           _isEnabled = true;
       }
       else
       {
           if (this != _instance)
           {
               Destroy(this.gameObject);
           }
       }
   }
}
