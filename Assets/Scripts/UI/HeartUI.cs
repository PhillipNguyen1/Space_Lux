﻿using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

[RequireComponent (typeof(RectTransform))]
public class HeartUI : MonoBehaviour {

	public Damageable Damageable;
	public float HealthPerHeart;
    public Sprite FullHealth;
    public Sprite EmptyBar;   
    public Image ImagePrefab;
	public Vector3 Scale;
	public float xOffset;
    [SerializeField]
	private Image[] hearts;

	private void Awake() {
		InitHearts();         
	}

	public void InitHearts() {
		Vector3 offset = new Vector3(xOffset, 0.0f, 0.0f);

		hearts = new Image[(int)(Damageable.MaxHP / HealthPerHeart)];       

        for (int i = 0; i < hearts.Length; i++) {            
            hearts[i] = Instantiate(ImagePrefab);                          
            hearts[i].name = "hearts_" + i;
			hearts[i].transform.SetParent(transform, false);
			hearts[i].rectTransform.localPosition += offset * i;
			hearts[i].rectTransform.localScale = Scale;             
		}

		UpdateHearts();        
	}

	public void UpdateHearts() {
		int numFilledHearts = Mathf.CeilToInt(Damageable.CurHP / HealthPerHeart);       
        
        if (numFilledHearts <= 0) {
            numFilledHearts = 0;
        }

		for (int i = 0; i < numFilledHearts; i++) {
            hearts[i].sprite = FullHealth;           
        }

		for (int i = numFilledHearts; i < hearts.Length; i++) {
            hearts[i].sprite = EmptyBar;         
        }
    }
}
