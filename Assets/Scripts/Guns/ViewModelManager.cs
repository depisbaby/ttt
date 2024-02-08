using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ViewModelManager : MonoBehaviour
{
    public static ViewModelManager Instance;

    public Transform defaultMuzzle;

    [SerializeField] float recoilModifier;
    ViewModel activeViewModel;
    [SerializeField]List<ViewModel> viewModels = new List<ViewModel>();
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
        if(activeViewModel != null)
        {
            activeViewModel.transform.localPosition = Vector3.Lerp(activeViewModel.transform.localPosition, Vector3.zero, Time.deltaTime * recoilRecovery);
        } 
    }

    public void SetViewModel(ViewModel target)
    {
        if(target == null)
        {
            if (activeViewModel != null)
            {
                activeViewModel.gameObject.SetActive(false);
                activeViewModel = null;
            }

            return;
        }

        if (activeViewModel != null && activeViewModel.viewModelName == target.viewModelName) return;

        ViewModel viewModel = null;

        foreach (var item in viewModels)
        {
            if(item.viewModelName == target.viewModelName)
            {
                viewModel = item;
            }
        }

        if(viewModel == null)
        {
            viewModel = Instantiate(target.gameObject).GetComponent<ViewModel>();
            viewModel.transform.parent = transform.parent;
            viewModel.transform.localPosition = Vector3.zero;
            viewModel.transform.localRotation = Quaternion.identity;
            viewModels.Add(viewModel);
        }

        if(activeViewModel != null)
        {
            activeViewModel.gameObject.SetActive(false);
        }

        activeViewModel = viewModel;
        activeViewModel.gameObject.SetActive(true);


    }

    public async void PlayShootAnimation(float recoilAmount, float _recoilRecovery)
    {
        float _recoil = recoilAmount * recoilModifier;
        activeViewModel.transform.localPosition = new Vector3(0, (_recoil * 0.2f), -(_recoil));
        recoilRecovery = _recoilRecovery;

        muzzleFlash.transform.localRotation = Quaternion.Euler(0,0, UnityEngine.Random.Range(0, 180));
        muzzleFlash.SetActive(true);

        await Task.Delay(10);

        muzzleFlash.SetActive(false);
    }

    public void HideViewModel(bool hide)
    {
        if (activeViewModel == null) return; 
        activeViewModel.gameObject.SetActive(!hide);
    }

    public Transform GetMuzzleTransform()
    {
        if(activeViewModel == null)
        {
            return defaultMuzzle;
        }

        return activeViewModel.muzzle;
    }
}
