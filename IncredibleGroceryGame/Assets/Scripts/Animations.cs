using System.Collections;
using UnityEngine;

namespace DefaultNamespace
{
    public static class Animations
    {
        public static IEnumerator MoveToTarget(Transform obj, Vector3 target, float duration)
        {
            Vector3 startPosition = obj.position;
            float t = 0;

            float animationDuration = duration;
            while (t < 1)
            {
                obj.position = Vector3.Lerp(startPosition, target, t);
                t += Time.deltaTime / animationDuration;
                yield return null;
            }
        }
        public static IEnumerator BuyerWalkToTarget(Transform obj, Vector3 target, float duration)
        {
            var startPosition = obj.position;
            var animationStartTime = Time.time;
            float t = 0;

            var animationDuration = duration;
            while (t < 1)
            {
                t += Time.deltaTime / animationDuration;
                var newpos = Vector3.Lerp(startPosition, target, t);
                var deltaY = Mathf.Abs(Mathf.Sin((Time.time - animationStartTime)
                    * Mathf.PI * 5 / duration) * 40);
                obj.position = new Vector3(newpos.x, newpos.y + deltaY);
                yield return null;
            }
        }
    }
}