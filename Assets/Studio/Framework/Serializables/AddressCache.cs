using Studio.Common;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Studio.Framework;

#pragma warning disable 649

namespace Studio.Framework
{
    public class AddressCache : MonoBehaviour
    {
        [SerializeField]
        private PrefabLib _prefab;
        [SerializeField]
        private SpriteLib _sprite;
        
        [SerializeField]
        private TextureLib _textureLib;

        public Task<Sprite> GetSpriteAsync(string category, string spriteName)
            => _sprite.GetSpriteAsync(category, spriteName);

        public Sprite GetSprite(string spriteName)
            => _sprite.GetSprite(spriteName);

        public Sprite GetSprite(string category, string spriteName)
            => _sprite.GetSprite(category, spriteName);

        public async Task LoadCharacterAsync(string charId, string costumeId)
        {
            await GetCharacterAsync($"{charId}/BasicPack/{costumeId}_basic");
            if (charId == "brown")
            {
                await GetCharacterAsync($"brown/DefensePack/{costumeId}_defense");
            }
            else if (charId == "cony")
            {
                await GetCharacterAsync($"cony/AttackPack/{costumeId}_attack");
            }
            else if (charId == "sally")
            {
                await GetCharacterAsync($"sally/raid_{costumeId}");
                await GetCharacterAsync($"sally/raid_result_{costumeId}");
            }
        }

        public async Task<GameObject> GetCharacterAsync(string filename)
        {
            return await _prefab.GetAssetAsync("character", filename);
        }

        public async Task<Sprite> GetItemAsync(string category, string filename)
        {
            return await _sprite.GetSpriteAsync(category, filename);
        }

        protected void OnDestroy()
        {
            if (_prefab)
            {
                _prefab.Clear();
            }
            if (_sprite)
            {
                _sprite.Clear();
            }
        }
    }
}