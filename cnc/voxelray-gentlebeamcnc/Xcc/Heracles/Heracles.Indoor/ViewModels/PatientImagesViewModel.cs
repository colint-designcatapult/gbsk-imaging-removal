using Heracles.Application.Models.Treatment;

using Prism.Regions;

using Xcc.Application.UI;
using Xcc.Application.UI.Mvvm;

namespace Heracles.Indoor.ViewModels
{
    [RegionMemberLifetime(KeepAlive = false)]
    internal class PatientImagesViewModel : RegionViewModelBase
    {
        #region Contructors
        public PatientImagesViewModel() : base(null, null, null)
        {
        }

        public PatientImagesViewModel(IRegionManager regionManager, ITreatmentInfoStore treatmentInfoStore) : base(regionManager)
        {
            TreatmentInfoStore = treatmentInfoStore;
        }

        #endregion Contructors

        #region Properties
        public ITreatmentInfoStore TreatmentInfoStore { get; }

        bool? _switch2D3D;
        public bool? Switch2D3D
        {
            get => _switch2D3D;
            set
            {
                if (SetProperty(ref _switch2D3D, value) && value is not null)
                {
                    // PhotoAcoustic imaging views removed as part of patient imaging removal
                }
            }
        }

        #endregion

        #region Private methods
        public override void OnNavigatedTo(NavigationContext navigationContext)
        {
            base.OnNavigatedTo(navigationContext);

            // PhotoAcoustic imaging removed as part of patient imaging removal
            Switch2D3D = false;

        }

        protected override void OnExit()
        {
            RegionManager.RequestNavigate(Regions.Main.ClinicalData.ImagesRegion, "ImagesView");
        }
        #endregion Private methods

    }
}
