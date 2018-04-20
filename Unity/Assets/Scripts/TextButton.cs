using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TextButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler {
    public float defaultScale = 0.9f;
    public float hoverScale = 0.95f;

    public UnityEvent onClick;

    private void Start()
    {
        defaultScale = transform.localScale.x;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = new Vector2(hoverScale, hoverScale);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = new Vector2(defaultScale, defaultScale);
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        Manager.Instance.menuManager.GetComponent<AudioSource>().Play();
        onClick.Invoke();
    }
}
