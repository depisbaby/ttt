using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ViewModelManager : MonoBehaviour
{
    public static ViewModelManager Instance;

    public Transform muzzle;

    [SerializeField] float recoilModifier;
    [SerializeField]GameObject viewModelHolder;
    float recoilRecovery;
    [SerializeField] GameObject muzzleFlash;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        viewModelHolder.transform.localPosition = Vector3.Lerp(viewModelHolder.transform.localPosition, Vector3.zero, Time.deltaTime * recoilRecovery);
    }

    public async void PlayShootAnimation(float recoilAmount, float _recoilRecovery)
    {
        float _recoil = recoilAmount * recoilModifier;
        viewModelHolder.transform.localPosition = new Vector3(0, (_recoil * 0.2f), -(_recoil));
        recoilRecovery = _recoilRecovery;

        muzzleFlash.transform.localRotation = Quaternion.Euler(0,0, UnityEngine.Random.Range(0, 180));
        muzzleFlash.SetActive(true);

        await Task.Delay(10);

        muzzleFlash.SetActive(false);
    }

    public void HideViewModel(bool hide)
    {
        viewModelHolder.SetActive(!hide);
    }
}
