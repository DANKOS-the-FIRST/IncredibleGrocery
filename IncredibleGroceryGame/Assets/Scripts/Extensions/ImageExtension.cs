using UnityEngine.UI;
namespace Extensions
{
    public static class ImageExtension
    {
        public static void MakeTranslucent(this Image img)
        {
            var tempColor = img.color;
            tempColor.a = 0.5f;
            img.color = tempColor;
        }
        public static void MakeOpaque(this Image img)
        {
            var tempColor = img.color;
            tempColor.a = 1f;
            img.color = tempColor;
        }
    }
}