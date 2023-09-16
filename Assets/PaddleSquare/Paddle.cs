using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PaddleSquare
{
    public class Paddle : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float
            minExtents = 4f,
            maxExtents = 4f,
            speed = 10f,
            maxTargetingBias = 0.75f;

        private float extents, targetingBias;

        [SerializeField] private bool isAI;
        [SerializeField] private TextMeshPro scoreText;
        [SerializeField] private MeshRenderer goalRenderer;

        [SerializeField, ColorUsage(true, true)]
        private Color goalColor = Color.white;

        private int score;

        private static readonly int
            timeOfLastHitId = Shader.PropertyToID("_TimeOfLastHit"),
            emissionColorId = Shader.PropertyToID("_EmissionColor"),
            faceColorId = Shader.PropertyToID("_FaceColor");

        private Material paddleMaterial, goalMaterial, scoreMaterial;

        void Awake()
        {
            paddleMaterial = GetComponent<MeshRenderer>().material;
            goalMaterial = goalRenderer.material;
            goalMaterial.SetColor(emissionColorId, goalColor);
            scoreMaterial = scoreText.fontMaterial;
            SetScore(0);
        }

        public void Move(float target, float arenaExtents)
        {
            Vector3 p = transform.localPosition;
            p.x = isAI ? AdjustByAI(p.x, target) : AdjustByPlayer(p.x);
            float limit = arenaExtents - extents;
            p.x = Mathf.Clamp(p.x, -limit, limit);
            transform.localPosition = p;
        }

        void ChangeTargetingBias() =>
            targetingBias = Random.Range(-maxTargetingBias, maxTargetingBias);

        void SetExtents(float newExtents)
        {
            extents = newExtents;
            Vector3 s = transform.localScale;
            s.x = 2 * newExtents;
            transform.localScale = s;
        }

        float AdjustByAI(float x, float target)
        {
            target += targetingBias * extents;
            if (x < target)
            {
                return Mathf.Min(x + speed * Time.deltaTime, target);
            }

            return Mathf.Max(x - speed * Time.deltaTime, target);
        }

        float AdjustByPlayer(float x)
        {
            bool goRight = Input.GetKey(KeyCode.RightArrow);
            bool goLeft = Input.GetKey(KeyCode.LeftArrow);
            if (goRight && !goLeft)
            {
                return x + speed * Time.deltaTime;
            }
            else if (goLeft && !goRight)
            {
                return x - speed * Time.deltaTime;
            }

            return x;
        }

        public bool HitBall(float ballX, float ballExtents, out float hitFactor)
        {
            ChangeTargetingBias();
            hitFactor =
                (ballX - transform.localPosition.x) /
                (extents + ballExtents);
            bool success = hitFactor is >= -1f and <= 1f;
            if (success)
            {
                paddleMaterial.SetFloat(timeOfLastHitId, Time.time);
            }

            return success;
        }


        void SetScore(int newScore, float pointsToWin = 1000f)
        {
            score = newScore;
            scoreText.SetText("{0}", newScore);
            scoreMaterial.SetColor(faceColorId, goalColor * (newScore / pointsToWin));
            SetExtents(Mathf.Lerp(maxExtents, minExtents, newScore / (pointsToWin - 1f)));
        }

        public void StartNewGame()
        {
            SetScore(0);
            ChangeTargetingBias();
        }

        public bool ScorePoint(int pointsToWin)
        {
            goalMaterial.SetFloat(timeOfLastHitId, Time.time);
            SetScore(score + 1, pointsToWin);
            return score >= pointsToWin;
        }
    }
}