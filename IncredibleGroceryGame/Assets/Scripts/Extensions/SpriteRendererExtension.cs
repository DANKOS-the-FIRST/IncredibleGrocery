using UnityEngine;
namespace Extensions
{
    public static class SpriteRendererExtension
    {
        public static void MakeTranslucent(this SpriteRenderer sprite)
        {
            var tempColor = sprite.color;
            tempColor.a = 0.5f;
            sprite.color = tempColor;
        }
        public static void MakeOpaque(this SpriteRenderer sprite)
        {
            var tempColor = sprite.color;
            tempColor.a = 1f;
            sprite.color = tempColor;
        }
    }
}