using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class InventorySlot : MonoBehaviour {

    public int mySlotID;
    Inventory inventory;
    public bool occupied = false;
    Text itemCountDisplay;
    Outline outline;

	// Use this for initialization
	void Start () {
        inventory = this.GetComponentInParent<Inventory>();
        itemCountDisplay = this.GetComponentInChildren<Text>();
        outline = this.GetComponent<Outline>();
	}
	
	public void HoverOver () 
    {
        inventory.activeSlot = mySlotID;
	}
    public void HoverOff()
    {
        inventory.activeSlot = -1;
    }
    void Selected()
    {

    }

    public void UpdateCountDisplay(int count)
    {
        itemCountDisplay.text = count.ToString();
    }

    public void FlashHighlight()
    {
        float speed = 0.25f;
        Sequence mySequence = DOTween.Sequence();
        mySequence.Append(outline.DOFade(1f, speed));
        //mySequence.Append(outline.DOFade(0f, speed));

        mySequence.SetLoops(6, LoopType.Yoyo);
        mySequence.SetEase(Ease.InQuart);

        mySequence.Play();
    }

}
