using System.Collections.Generic;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class SnapshotInterpolator : MonoBehaviour
    {
        private readonly List<Sample> samples = new List<Sample>();

        public void AddSample(Vector2 position)
        {
            samples.Add(new Sample(Time.unscaledTime, position));
        }

        private void Update()
        {
            if (samples.Count == 0)
            {
                return;
            }

            float renderTime = Time.unscaledTime - GameplayConfig.InterpolationDelaySeconds;
            DiscardConsumedSamples(renderTime);
            transform.position = EvaluatePosition(renderTime);
        }

        private void DiscardConsumedSamples(float renderTime)
        {
            while (samples.Count >= 2 && samples[1].ReceivedAt <= renderTime)
            {
                samples.RemoveAt(0);
            }
        }

        private Vector2 EvaluatePosition(float renderTime)
        {
            Sample from = samples[0];
            if (samples.Count == 1 || renderTime <= from.ReceivedAt)
            {
                return from.Position;
            }

            Sample to = samples[1];
            float span = to.ReceivedAt - from.ReceivedAt;
            if (span <= Mathf.Epsilon)
            {
                return to.Position;
            }

            float blend = (renderTime - from.ReceivedAt) / span;
            return Vector2.Lerp(from.Position, to.Position, blend);
        }

        private readonly struct Sample
        {
            public float ReceivedAt { get; }
            public Vector2 Position { get; }

            public Sample(float receivedAt, Vector2 position)
            {
                ReceivedAt = receivedAt;
                Position = position;
            }
        }
    }
}
