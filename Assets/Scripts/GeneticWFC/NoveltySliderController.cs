using TMPro;
using UnityEngine;

public class NoveltySliderController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private ILevelGenerator levelGenerator;

    private void Awake()
    {
        levelGenerator = GameObject.FindGameObjectWithTag("LevelGenerator").GetComponent<ILevelGenerator>();
    }

    public void OnValueChanged(float value)
    {
        label.text = value.ToString("F2");
        levelGenerator.SetDesiredNovelty(value);
    }
}
