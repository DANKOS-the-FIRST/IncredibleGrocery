using System;
using UnityEngine.UI;

namespace Extensions
{
    public static class ButtonExtension
    {
        public static void AddEventListener<T> (this Button button, T param, Action<T> onClick)
        {
            button.onClick.AddListener (() =>
            {
                onClick (param);
            });
        }
    }
}