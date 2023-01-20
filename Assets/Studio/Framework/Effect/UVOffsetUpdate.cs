using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVOffsetUpdate : MonoBehaviour {

	[SerializeField] Vector2 start = Vector3.zero;
	[SerializeField] Vector2 end = Vector3.zero;
	[SerializeField] float duration = 1;
	[SerializeField] string texture = "_MainTex";
	private float elapsed = 0;
	private Renderer _renderer = null;
	private Material[] materials = null;
    private int textureId = 0;
	// Use this for initialization
	void Start () {
		_renderer = GetComponent<Renderer>();
        if (_renderer)
    		materials = _renderer.materials;
        if (textureId == 0)
            textureId = Shader.PropertyToID(texture);
	}
	
	// Update is called once per frame
	void Update () {
        if (duration > 0 && _renderer)
		{
			elapsed = (elapsed + Time.deltaTime) % duration;
			var offset = Vector2.Lerp(start, end, elapsed / duration);
			foreach (var material in materials)
			{
				material.SetTextureOffset(textureId, offset);
			}
		}
	}
}
