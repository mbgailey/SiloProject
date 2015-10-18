using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class HealthGUI : MonoBehaviour {

    public Image healthFill;
    public Transform healthTransform;
    public Color highlightColor;
    Color startColor;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public void SetHealth (float newHealth) 
    {
        //healthTransform.DOPunchPosition(new Vector3(1f, 0f, 0f), 1f);
        //healthFill.DOFillAmount(newHealth, 0.5f);

        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(healthTransform.DOPunchPosition(new Vector3(1f, 0f, 0f), 1f));

        Sequence flashSequence = DOTween.Sequence();
        float speed = 0.07f;
        flashSequence.Append(healthFill.DOColor(highlightColor, speed));
        flashSequence.SetLoops(8, LoopType.Yoyo);
        flashSequence.SetEase(Ease.InQuart);
        mySequence.Join(flashSequence);
        mySequence.Append(healthFill.DOFillAmount(newHealth, 0.5f));

        //mySequence.SetEase(Ease.InQuart);

        mySequence.Play();
    }
}
