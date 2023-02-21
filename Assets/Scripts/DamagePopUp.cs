using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    private TextMeshPro textMesh;
    public float MoveSpeed = 20f;
    public float disappearTimer = 1f;
    private Color textColor;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    public void SetUp(float damageAmount)
    {
        textMesh.SetText(damageAmount.ToString());
        transform.rotation = Quaternion.Euler(90, 0, 0);
        textColor = textMesh.color;
    }

    private void Update()
    {
        transform.position += new Vector3(0, 0, MoveSpeed) * Time.deltaTime;

        disappearTimer -= Time.deltaTime;
        if(disappearTimer < 0)
        {
            float disappearSpeed = 3f;
            textColor.a -= disappearSpeed * Time.deltaTime;
            textMesh.color = textColor;
            if(textColor.a < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
