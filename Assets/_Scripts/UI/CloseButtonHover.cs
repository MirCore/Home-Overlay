using System;
using Unity.PolySpatial;
using UnityEngine;
using UnityEngine.UI;

public class CloseButtonHover : MonoBehaviour
{
    private Button _button;
    private Image _image;
    private float _normalAlpha;
    private Animator _animator;

    private void OnEnable()
    {
        if (_button == null)
            _button = GetComponent<Button>();
        if (_animator == null)
            _animator = GetComponent<Animator>();
        _image = _button.targetGraphic as Image;
        if (_image != null) 
            _normalAlpha = _image.color.a;
    }

    private void Update()
    {
        if (_image.color.a <= _normalAlpha)
            return;

        _animator.Play("Highlighted");
        Debug.Log("Highlighted");
    }
}
