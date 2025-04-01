using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    Ship ship;
    Image image;
    public float offset = .75f;

    // Start is called before the first frame update
    void Start()
    {
        ship = GetComponentInParent<Ship>();
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (ship == null)
            return;
        image.enabled = ship.health != ship.ActualMaxHealth;

        image.transform.up = Vector3.up;
        image.transform.localScale = new Vector3(
            ship.health > 0f ? ship.health / ship.ActualMaxHealth : 0f
            
            , image.transform.localScale.y, image.transform.localScale.z);
        transform.position = ship.transform.position + Vector3.up * offset;
    }
}
