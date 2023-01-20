using Studio.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Studio.Framework
{
    [CreateAssetMenu(fileName = "TextureLib.asset", menuName = "Game/Asset/TextureLib")]
    public class TextureLib : AssetLib<Texture2D>
    {
    }
}