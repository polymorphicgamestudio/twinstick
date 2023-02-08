//   ▄▀▄     ▄▀▄
//  ▄█  ▀▀▀▀▀  █▄
//  █           █
//  █  ▀  ┬  ▀  █
using System.Collections.Generic;
using System;
using UnityEngine;

public class Colors : MonoBehaviour {

	[HideInInspector] public enum ColorName {
		Red,		Yellow,		Green,
		Cyan,		Blue,		Magenta,
		DarkRed,	LightRed,	DarkGreen,
		LightGreen,	DarkBlue,	LightBlue,
		Black,		Grey,		White
	}
	[HideInInspector] public Color[] color15 = new Color[] {
		new Color(1f,0f,0f),	new Color(1f,1f,0f),	new Color(0f,1f,0f),
		new Color(0f,1f,1f),	new Color(0f,0f,1f),	new Color(1f,0f,1f),
		new Color(.5f,0f,0f),	new Color(1f,.5f,.5f),	new Color(0f,.5f,0f),
		new Color(.5f,1f,.5f),	new Color(0f,0f,.5f),	new Color(.5f,.5f,1f),
		new Color(0f,0f,0f),	new Color(.5f,.5f,.5f),	new Color(1f,1f,1f)
	};
	

	public Texture2D colorZoneImage;
	public int mapSize = 128;
	Color[] pixels;
	[HideInInspector] public List<int>[] pixIndexByColour;


	private void Awake() {
		GM.cm = this;
	}
	private void Start() {
		pixels = colorZoneImage.GetPixels();
		InitializePixIndexByColour();
	}
	private void Update() {
		//DebugColorZones();
	}

	void InitializePixIndexByColour() {
		pixIndexByColour = new List<int>[Enum.GetNames(typeof(ColorName)).Length];
		//create lists
		for (int i = 0; i < Enum.GetNames(typeof(ColorName)).Length; i++)
			pixIndexByColour[i] = new List<int>();
		//sort colors into lists
		for (int i = 0; i < pixels.Length; i++) {
			//print($"adding {i} to {(int)NameFromColor(pixels[i])}");
			if (pixels[i].a < 0.75f) continue; // don't worry about transparent areas - they're not walkable
			pixIndexByColour[(int)NameFromColor(pixels[i])].Add(i);
		}
	}

	public ColorName NameFromColor(Color c) {
		if (c.r > 0.3f) {
			if (c.r > 0.7f) {
				if (c.g > 0.3f) {
					if (c.g > 0.7f) {
						if (c.b > 0.3f) return ColorName.White;
						else return ColorName.Yellow;
					}
					else return ColorName.LightRed;
				}
				else if (c.b > 0.3f) return ColorName.Magenta;
				else return ColorName.Red;
			}
			else if (c.g > 0.3f) {
				if (c.g > 0.7f) return ColorName.LightGreen;
				else return ColorName.Grey;
			}
			else if (c.b > 0.3f) return ColorName.LightBlue;
			else return ColorName.DarkRed;
		}
		else if (c.g > 0.3f) {
			if (c.g > 0.7f) {
				if (c.b > 0.3f) return ColorName.Cyan;
				else return ColorName.Green;
			}
			else return ColorName.DarkGreen;
		}
		else if (c.b > 0.3f) {
			if (c.b > 0.7f) return ColorName.Blue;
			else return ColorName.DarkBlue;
		}
		else return ColorName.Black;
	}
	public Vector2Int PointToPixelPosition(Vector3 point) {
		float scaleFactor = colorZoneImage.width / mapSize;
		int mapOffset = mapSize / 2; // assume playable area is centered on the origin
		return new Vector2Int((int)((point.x + mapOffset) * scaleFactor),(int)((point.z + mapOffset) * scaleFactor));
	}
	public Color ColorAtPixelPosition(Vector2Int pixel) {
		return pixels[pixel.y * colorZoneImage.width + pixel.x];
	}
	public Color ColorAtPoint(Vector3 point) {
		return ColorAtPixelPosition(PointToPixelPosition(point));
	}
	public Color ColorFromName(ColorName name) {
		return color15[(int)name];
	}
	public bool IsAllowed (ColorName[] allowedColors, Color checkColor) {
		// Takes an array of places that an animal is alowed to walk
		// and for any given pixel, returns if the animal is allowed to walk in that pixel
		//if (checkColor.a < 0.75f ? true : false) return false; use alpha to mark areas in any color as non walkable.
		return Array.Exists(allowedColors, element => element == NameFromColor(checkColor));
	}
	public List<int> PixIndexByColour(int color) {
		//Debug.Log($"PixIndexByColour returning {Enum.GetNames(typeof(ColorName))[colo]} {pixels[pixIndexByColour[colo][0]]}");
		return pixIndexByColour[color];
	}
	public Vector3 pixIndexToVector3(int index) {
		float scaleFactor = colorZoneImage.width / mapSize;
		int mapOffset = mapSize / 2; // assume playable area is centered on the origin
		Vector3 vector3 = new Vector3(){
			x = ((index % colorZoneImage.width) / scaleFactor) - mapOffset,
			y = 0, // arbitrary y above the map
			z = ((index / colorZoneImage.width) / scaleFactor) - mapOffset
		};
		return vector3;
	}


	void DebugColorZones() {
		Vector3 point;
		Color color;
		int halfMapSize = mapSize / 2;
		for (int i = -halfMapSize; i < halfMapSize; i++) {
			for (int j = -halfMapSize; j < halfMapSize; j++) {
				point = new Vector3(i, 30, j);
				color = ColorAtPoint(point);
				Debug.DrawRay(point - Vector3.forward * 0.25f, Vector3.forward * 0.5f, color);
				Debug.DrawRay(point - Vector3.right * 0.25f, Vector3.right * 0.5f, color);
			}
		}
	}
}