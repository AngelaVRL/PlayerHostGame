using System.Collections.Generic;
using UnityEngine;

namespace PlayerHostGame.Gameplay
{
    public sealed class CoinsView : MonoBehaviour
    {
        [SerializeField] private MatchContext matchContext;
        [SerializeField] private GameObject coinPrefab;

        private readonly Dictionary<int, GameObject> coinObjectsById = new Dictionary<int, GameObject>();

        private void OnEnable()
        {
            matchContext.Model.CoinAdded += HandleCoinAdded;
            matchContext.Model.CoinRemoved += HandleCoinRemoved;
        }

        private void OnDisable()
        {
            matchContext.Model.CoinAdded -= HandleCoinAdded;
            matchContext.Model.CoinRemoved -= HandleCoinRemoved;
        }

        private void HandleCoinAdded(int coinId, Vector2 position)
        {
            coinObjectsById[coinId] = Instantiate(coinPrefab, position, Quaternion.identity, transform);
        }

        private void HandleCoinRemoved(int coinId)
        {
            if (coinObjectsById.TryGetValue(coinId, out GameObject coinObject))
            {
                Destroy(coinObject);
                coinObjectsById.Remove(coinId);
            }
        }
    }
}
