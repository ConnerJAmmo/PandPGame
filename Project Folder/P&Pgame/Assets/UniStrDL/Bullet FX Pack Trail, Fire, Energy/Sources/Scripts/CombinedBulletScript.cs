using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace bullet.fx.pack
{

    // Enum to differentiate between the two potential mesh types
    public enum MeshType
    {
        Bullet,
        Cylinder
    }

    public sealed class CombinedBulletScript : MonoBehaviour
    {

        [SerializeField] private Rigidbody Rigidbody;
        [SerializeField] private BulletEffectType BulletEffectType;
        [SerializeField] private Transform EndPosiotionBullet;
        [SerializeField] private Material BulletTrailMaterial;
        [SerializeField] private GameObject Fire2Effect;
        [SerializeField] private GameObject Fire3Effect;
        [SerializeField] private MeshType meshType;

        private Vector3 endPositionTrail;
        private bool IsFlying = true;
        private bool hasCollided = false;
        private Material material;
        private float fadeDuration = 1f;
        private float timer = 0f;
        private bool subjectToDeletion = false;

        private void Start()
        {
            var customMesh = new Mesh();
            customMesh.name = (meshType == MeshType.Bullet) ? "Bullet" : "Fire3DCylinder";

            // Call the appropriate SetVertices method based on selection
            customMesh.vertices = (meshType == MeshType.Bullet) ? SetVerticesBullet() : SetVerticesCylinder();
            // Call the appropriate SetTriangles method based on selection
            customMesh.triangles = (meshType == MeshType.Bullet) ? SetTrianglesBullet() : SetTrianglesCylinder();

            if (meshType == MeshType.Cylinder)
            {
                customMesh.uv = SetUVs(customMesh.vertices);
            }

            customMesh.RecalculateNormals();
            GetComponent<MeshFilter>().mesh = customMesh;

            if (meshType == MeshType.Bullet)
            {
                var collider = gameObject.AddComponent<MeshCollider>();
                collider.sharedMesh = null;
                collider.sharedMesh = customMesh;
                collider.convex = true;
                material = GetComponent<Renderer>().material;
                material.SetFloat("_Fade", 1f);
                endPositionTrail = EndPosiotionBullet.position;
                Fire2Effect.SetActive(BulletEffectType == BulletEffectType.Fire2);
                Fire3Effect.SetActive(BulletEffectType == BulletEffectType.Fire3);

                if (BulletEffectType == BulletEffectType.Fire1)
                {
                    StartCoroutine(StartCreateBulletTrail());
                }
            }
        }

        private IEnumerator StartCreateBulletTrail()
        {
            while (IsFlying)
            {
                yield return new WaitForSeconds(0.02f);
                CreateBulletTrail(EndPosiotionBullet.position, endPositionTrail);
                endPositionTrail = EndPosiotionBullet.position;
            }
        }

        private void CreateBulletTrail(Vector3 start, Vector3 end)
        {
            GameObject trail = new GameObject("BulletTrail");
            LineRenderer line = trail.AddComponent<LineRenderer>();

            line.material = BulletTrailMaterial;
            line.startWidth = 0.05f;
            line.endWidth = 0.01f;
            line.positionCount = 6;

            for (int i = 0; i < line.positionCount; i++)
            {
                float t = (float)i / (line.positionCount - 1);
                line.SetPosition(i, Vector3.Lerp(start, end, t) + Random.insideUnitSphere * 0.005f);
            }

            StartCoroutine(FadeOutTrail(line, 0.1f));
        }

        private IEnumerator FadeOutTrail(LineRenderer line, float fadeTime)
        {
            float elapsed = 0f;
            Color startColor = line.material.color;

            while (elapsed < fadeTime)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeTime);
                line.startWidth = Mathf.Lerp(0.05f, 0f, elapsed / fadeTime);
                line.endWidth = Mathf.Lerp(0.01f, 0f, elapsed / fadeTime);
                line.material.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                yield return null;
            }

            Destroy(line.gameObject);
        }

        private async void OnCollisionEnter(Collision collision)
        {
            if (hasCollided) return;
            hasCollided = true;

            Rigidbody.useGravity = true;
            IsFlying = false;
            Fire2Effect.SetActive(false);
            Fire3Effect.SetActive(false);
            await Task.Delay(5000);
            subjectToDeletion = true;
        }

        void Update()
        {
            if (subjectToDeletion)
            {
                timer += Time.deltaTime;
                float fadeValue = Mathf.Clamp01(1f - (timer / fadeDuration));
                material.SetFloat("_Fade", fadeValue);

                if (fadeValue <= 0)
                {
                    subjectToDeletion = false;
                    Destroy(gameObject);
                }
            }
        }

        private Vector3[] SetVerticesBullet()
        {
            var coef = 0.4f;
            var coef2 = 0.34f;
            var coef3 = 0.24f;
            var radiusMedium = Mathf.Cos(Mathf.PI / 4f);
            var radiusMediumHalf = radiusMedium * 0.4f;
            return new Vector3[] {
        // Swapped from (X, Y, Z) to (X, Z, Y)
        new Vector3(0, 0, -0.5f),
        new Vector3(-0.4f * coef, 0, -0.5f),
        new Vector3(-radiusMediumHalf * coef, radiusMediumHalf * coef, -0.5f),
        new Vector3(0, 0.4f * coef, -0.5f),
        new Vector3(radiusMediumHalf * coef, radiusMediumHalf * coef, -0.5f),
        new Vector3(0.4f * coef, 0, -0.5f),
        new Vector3(radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.5f),
        new Vector3(0, -0.4f * coef, -0.5f),
        new Vector3(-radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.5f),
        new Vector3(-0.4f * coef, 0, -0.44f),
        new Vector3(-radiusMediumHalf * coef, radiusMediumHalf * coef, -0.44f),
        new Vector3(0, 0.4f * coef, -0.44f),
        new Vector3(radiusMediumHalf * coef, radiusMediumHalf * coef, -0.44f),
        new Vector3(0.4f * coef, 0, -0.44f),
        new Vector3(radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.44f),
        new Vector3(0, -0.4f * coef, -0.44f),
        new Vector3(-radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.44f),
        new Vector3(-0.4f * coef2, 0, -0.44f),
        new Vector3(-radiusMediumHalf * coef2, radiusMediumHalf * coef2, -0.44f),
        new Vector3(0, 0.4f * coef2, -0.44f),
        new Vector3(radiusMediumHalf * coef2, radiusMediumHalf * coef2, -0.44f),
        new Vector3(0.4f * coef2, 0, -0.44f),
        new Vector3(radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -0.44f),
        new Vector3(0, -0.4f * coef2, -0.44f),
        new Vector3(-radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -0.44f),
        new Vector3(-0.4f * coef2, 0, -0.40f),
        new Vector3(-radiusMediumHalf * coef2, radiusMediumHalf * coef2, -0.40f),
        new Vector3(0, 0.4f * coef2, -0.40f),
        new Vector3(radiusMediumHalf * coef2, radiusMediumHalf * coef2, -0.40f),
        new Vector3(0.4f * coef2, 0, -0.40f),
        new Vector3(radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -0.40f),
        new Vector3(0, -0.4f * coef2, -0.40f),
        new Vector3(-radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -0.40f),
        new Vector3(-0.4f * coef, 0, -0.40f),
        new Vector3(-radiusMediumHalf * coef, radiusMediumHalf * coef, -0.40f),
        new Vector3(0, 0.4f * coef, -0.40f),
        new Vector3(radiusMediumHalf * coef, radiusMediumHalf * coef, -0.40f),
        new Vector3(0.4f * coef, 0, -0.40f),
        new Vector3(radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.40f),
        new Vector3(0, -0.4f * coef, -0.40f),
        new Vector3(-radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.40f),
        new Vector3(-0.4f * coef, 0, 0.2f),
        new Vector3(-radiusMediumHalf * coef, radiusMediumHalf * coef, 0.2f),
        new Vector3(0, 0.4f * coef, 0.2f),
        new Vector3(radiusMediumHalf * coef, radiusMediumHalf * coef, 0.2f),
        new Vector3(0.4f * coef, 0, 0.2f),
        new Vector3(radiusMediumHalf * coef, -radiusMediumHalf * coef, 0.2f),
        new Vector3(0, -0.4f * coef, 0.2f),
        new Vector3(-radiusMediumHalf * coef, -radiusMediumHalf * coef, 0.2f),
        new Vector3(-0.4f * coef2, 0, 0.2f),
        new Vector3(-radiusMediumHalf * coef2, radiusMediumHalf * coef2, 0.2f),
        new Vector3(0, 0.4f * coef2, 0.2f),
        new Vector3(radiusMediumHalf * coef2, radiusMediumHalf * coef2, 0.2f),
        new Vector3(0.4f * coef2, 0, 0.2f),
        new Vector3(radiusMediumHalf * coef2, -radiusMediumHalf * coef2, 0.2f),
        new Vector3(0, -0.4f * coef2, 0.2f),
        new Vector3(-radiusMediumHalf * coef2, -radiusMediumHalf * coef2, 0.2f),
        new Vector3(-0.4f * coef2, 0, 0.3f),
        new Vector3(-radiusMediumHalf * coef2, radiusMediumHalf * coef2, 0.3f),
        new Vector3(0, 0.4f * coef2, 0.3f),
        new Vector3(radiusMediumHalf * coef2, radiusMediumHalf * coef2, 0.3f),
        new Vector3(0.4f * coef2, 0, 0.3f),
        new Vector3(radiusMediumHalf * coef2, -radiusMediumHalf * coef2, 0.3f),
        new Vector3(0, -0.4f * coef2, 0.3f),
        new Vector3(-radiusMediumHalf * coef2, -radiusMediumHalf * coef2, 0.3f),
        new Vector3(-0.4f * coef3, 0, 0.4f),
        new Vector3(-radiusMediumHalf * coef3, radiusMediumHalf * coef3, 0.4f),
        new Vector3(0, 0.4f * coef3, 0.4f),
        new Vector3(radiusMediumHalf * coef3, radiusMediumHalf * coef3, 0.4f),
        new Vector3(0.4f * coef3, 0, 0.4f),
        new Vector3(radiusMediumHalf * coef3, -radiusMediumHalf * coef3, 0.4f),
        new Vector3(0, -0.4f * coef3, 0.4f),
        new Vector3(-radiusMediumHalf * coef3, -radiusMediumHalf * coef3, 0.4f),
        new Vector3(0, 0, 0.5f)
    };
        }


        private int[] SetTrianglesBullet()
        {
            return new int[] {
                0, 2, 1,
                0, 3, 2,
                0, 4, 3,
                0, 5, 4,
                0, 6, 5,
                0, 7, 6,
                0, 8, 7,
                0, 1, 8,

                9, 1, 2,
                9, 2, 10,
                10, 2, 3,
                10, 3, 11,
                11, 3, 4,
                11, 4, 12,
                12, 4, 5,
                12, 5, 13,
                13, 5, 6,
                13, 6, 14,
                14, 6, 7,
                14, 7, 15,
                15, 7, 8,
                15, 8, 16,
                16, 8, 9,
                9, 8, 1,

                17, 9, 10,
                17, 10, 18,
                18, 10, 11,
                18, 11, 19,
                19, 11, 12,
                19, 12, 20,
                20, 12, 13,
                20, 13, 21,
                21, 13, 14,
                21, 14, 22,
                22, 14, 15,
                22, 15, 23,
                23, 15, 16,
                23, 16, 24,
                24, 16, 17,
                17, 16, 9,

                25, 17, 18,
                25, 18, 26,
                26, 18, 19,
                26, 19, 27,
                27, 19, 20,
                27, 20, 28,
                28, 20, 21,
                28, 21, 29,
                29, 21, 22,
                29, 22, 30,
                30, 22, 23,
                30, 23, 31,
                31, 23, 24,
                31, 24, 32,
                32, 24, 25,
                25, 24, 17,

                33, 25, 26,
                33, 26, 34,
                34, 26, 27,
                34, 27, 35,
                35, 27, 28,
                35, 28, 36,
                36, 28, 29,
                36, 29, 37,
                37, 29, 30,
                37, 30, 38,
                38, 30, 31,
                38, 31, 39,
                39, 31, 32,
                39, 32, 40,
                40, 32, 33,
                33, 32, 25,

                41, 33, 34,
                41, 34, 42,
                42, 34, 35,
                42, 35, 43,
                43, 35, 36,
                43, 36, 44,
                44, 36, 37,
                44, 37, 45,
                45, 37, 38,
                45, 38, 46,
                46, 38, 39,
                46, 39, 47,
                47, 39, 40,
                47, 40, 48,
                48, 40, 41,
                41, 40, 33,

                49, 41, 42,
                49, 42, 50,
                50, 42, 43,
                50, 43, 51,
                51, 43, 44,
                51, 44, 52,
                52, 44, 45,
                52, 45, 53,
                53, 45, 46,
                53, 46, 54,
                54, 46, 47,
                54, 47, 55,
                55, 47, 48,
                55, 48, 56,
                56, 48, 49,
                49, 48, 41,

                57, 49, 50,
                57, 50, 58,
                58, 50, 51,
                58, 51, 59,
                59, 51, 52,
                59, 52, 60,
                60, 52, 53,
                60, 53, 61,
                61, 53, 54,
                61, 54, 62,
                62, 54, 55,
                62, 55, 63,
                63, 55, 56,
                63, 56, 64,
                64, 56, 57,
                57, 56, 49,

                65, 57, 58,
                65, 58, 66,
                66, 58, 59,
                66, 59, 67,
                67, 59, 60,
                67, 60, 68,
                68, 60, 61,
                68, 61, 69,
                69, 61, 62,
                69, 62, 70,
                70, 62, 63,
                70, 63, 71,
                71, 63, 64,
                71, 64, 72,
                72, 64, 65,
                65, 64, 57,

                73, 65, 66,
                73, 66, 67,
                73, 67, 68,
                73, 68, 69,
                73, 69, 70,
                73, 70, 71,
                73, 71, 72, 
                73, 72, 65 
            };
        }

        private Vector3[] SetVerticesCylinder()
        {
            var coef = 0.3f;
            var coef2 = 0.2f;
            var radiusMedium = Mathf.Cos(Mathf.PI / 4f);
            var radiusMediumHalf = radiusMedium * 0.4f;
            return new Vector3[] {
        // Swapped from (X, Y, Z) to (X, Z, Y)
        new Vector3(-0.4f * coef2, 0, -2.5f),
        new Vector3(-radiusMediumHalf * coef2, radiusMediumHalf * coef2, -2.5f),
        new Vector3(0, 0.4f * coef2, -2.5f),
        new Vector3(radiusMediumHalf * coef2, radiusMediumHalf * coef2, -2.5f),
        new Vector3(0.4f * coef2, 0, -2.5f),
        new Vector3(radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -2.5f),
        new Vector3(0, -0.4f * coef2, -2.5f),
        new Vector3(-radiusMediumHalf * coef2, -radiusMediumHalf * coef2, -2.5f),
        new Vector3(-0.4f * coef, 0, -0.5f),
        new Vector3(-radiusMediumHalf * coef, radiusMediumHalf * coef, -0.5f),
        new Vector3(0, 0.4f * coef, -0.5f),
        new Vector3(radiusMediumHalf * coef, radiusMediumHalf * coef, -0.5f),
        new Vector3(0.4f * coef, 0, -0.5f),
        new Vector3(radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.5f),
        new Vector3(0, -0.4f * coef, -0.5f),
        new Vector3(-radiusMediumHalf * coef, -radiusMediumHalf * coef, -0.5f)
    };
        }


        private int[] SetTrianglesCylinder()
        {
            return new int[] {
                8, 0, 1,
                8, 1, 9,
                9, 1, 2,
                9, 2, 10,
                10, 2, 3,
                10, 3, 11,
                11, 3, 4,
                11, 4, 12,
                12, 4, 5,
                12, 5, 13,
                13, 5, 6,
                13, 6, 14,
                14, 6, 7,
                14, 7, 15,
                15, 7, 8,
                8, 7, 0 
            };
        }

        private Vector2[] SetUVs(Vector3[] vertices)
        {
            Vector2[] uvs = new Vector2[vertices.Length];
            float height = 2.0f; // This logic might need review based on your new Z height

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 vertex = vertices[i];

                // Check vertex.z instead of vertex.y
                if (vertex.z > 0)
                {
                    // Use vertex.z here too
                    float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI);
                    float v = (vertex.z + height * 0.5f) / height;
                    uvs[i] = new Vector2(u, v);
                }
                else
                {
                    // Use vertex.z here too
                    float u = Mathf.Atan2(vertex.z, vertex.x) / (2 * Mathf.PI);
                    float v = (vertex.z + height * 0.5f) / height;
                    uvs[i] = new Vector2(u, v);
                }
            }
            return uvs;
        }

    }
}
