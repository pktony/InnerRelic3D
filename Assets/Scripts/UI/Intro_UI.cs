using System.Collections;
using UnityEngine;
using TMPro;

public class Intro_UI : MonoBehaviour
{
    TextMeshProUGUI introText;

    [SerializeField] float introSpeed = 100f;

    private void Awake()
    {
        introText = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        StartCoroutine(ShowIntro());
    }

    IEnumerator ShowIntro()
    {
        while(introText.fontSize <= 100f)
        {
            introText.fontSize += Time.deltaTime * introSpeed;
            yield return null;
        }
        yield return new WaitForSeconds(3.0f);
        introText.gameObject.SetActive(false);
    }
}
