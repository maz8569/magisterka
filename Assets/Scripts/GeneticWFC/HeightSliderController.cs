using TMPro;
using UnityEngine;

public class HeightSliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private ILevelGenerator levelGenerator;

    private void Awake()
    {
        levelGenerator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<ILevelGenerator>();
    }

    public void OnValueChanged(float value)
    {
        label.text = ((int)value).ToString();
        levelGenerator.SetDesiredHeight(value/2);
    }
}
