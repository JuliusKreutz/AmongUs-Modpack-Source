using UnityEngine;

namespace Modpack
{
    public class Arrow
    {
        public const float perc = 0.925f;
        public readonly SpriteRenderer image;
        public readonly GameObject arrow;
        private Vector3 _oldTarget;

        private static Sprite sprite;

        public static Sprite getSprite()
        {
            if (sprite) return sprite;
            sprite = Helpers.loadSpriteFromResources("Modpack.Resources.Arrow.png", 200f);
            return sprite;
        }


        public Arrow(Color color)
        {
            arrow = new GameObject("Arrow") {layer = 5};
            image = arrow.AddComponent<SpriteRenderer>();
            image.sprite = getSprite();
            image.color = color;
        }

        public void Update()
        {
            var target = _oldTarget;
            Update(target);
        }

        public void Update(Vector3 target)
        {
            if (arrow == null) return;
            _oldTarget = target;

            var main = Camera.main;
            if (main is { })
            {
                Vector2 vector = target - main.transform.position;
                var num = vector.magnitude / (main.orthographicSize * perc);
                image.enabled = num > 0.3;
                Vector2 vector2 = main.WorldToViewportPoint(target);
                if (Between(vector2.x, 0f, 1f) && Between(vector2.y, 0f, 1f))
                {
                    arrow.transform.position = target - (Vector3) vector.normalized * 0.6f;
                    var num2 = Mathf.Clamp(num, 0f, 1f);
                    arrow.transform.localScale = new Vector3(num2, num2, num2);
                }
                else
                {
                    var vector3 = new Vector2(Mathf.Clamp(vector2.x * 2f - 1f, -1f, 1f),
                        Mathf.Clamp(vector2.y * 2f - 1f, -1f, 1f));
                    var size = main.orthographicSize;
                    var num3 = size * main.aspect;
                    var vector4 = new Vector3(Mathf.LerpUnclamped(0f, num3 * 0.88f, vector3.x),
                        Mathf.LerpUnclamped(0f, size * 0.79f, vector3.y), 0f);
                    arrow.transform.position = main.transform.position + vector4;
                    arrow.transform.localScale = Vector3.one;
                }
            }

            LookAt2d(arrow.transform, target);
        }

        private void LookAt2d(Transform transform, Vector3 target)
        {
            var vector = target - transform.position;
            vector.Normalize();
            var num = Mathf.Atan2(vector.y, vector.x);
            if (transform.lossyScale.x < 0f)
                num += 3.1415927f;
            transform.rotation = Quaternion.Euler(0f, 0f, num * 57.29578f);
        }

        private bool Between(float value, float min, float max)
        {
            return value > min && value < max;
        }
    }
}