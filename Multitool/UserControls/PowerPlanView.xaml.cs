using Multitool.NTInterop.Power;

using MultitoolWPF.Tools;

using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultitoolWPF.UserControls
{
    /// <summary>
    /// Interaction logic for PowerPlanViewUserControl.xaml
    /// </summary>
    public partial class PowerPlanView : UserControl
    {
        private PowerPlan plan;

        public PowerPlanView(PowerPlan powerPlan)
        {
            plan = powerPlan;
            plan.StatusChanged += Plan_StatusChanged;
            InitializeComponent();
            SetGradients(plan.Active);
            SetPlanName(powerPlan.Name);
            SetGuid(powerPlan.Guid.ToString());
        }

        public PowerPlan PowerPlan => plan;

        private void SetGradients(bool active)
        {
            if (active)
            {
                SetStatus("Currently active");
                SetFirstGradient(Tool.GetAppRessource<Color>("DarkDevBlueColor"));
                SetSecondGradient(Tool.GetAppRessource<Color>("DevBlueColor"));
                SetThirdGradient((Color)ColorConverter.ConvertFromString("#CFCFCF"));
            }
            else
            {
                SetStatus("Inactive");
                SetFirstGradient(Tool.GetAppRessource<Color>("DarkBlackColor"));
                SetSecondGradient(Tool.GetAppRessource<Color>("LightBlackColor"));
                SetThirdGradient((Color)ColorConverter.ConvertFromString("#CFCFCF"));
            }
        }

        #region setters
        private void SetFirstGradient(Color value)
        {
            GradientStopOne.Color = value;
        }

        private void SetSecondGradient(Color value)
        {
            GradientStopTwo.Color = value;
        }

        private void SetThirdGradient(Color value)
        {
            GradientStopThree.Color = value;
        }

        private void SetPlanName(string value)
        {
            PlanName_TextBlock.Text = value;
        }

        private void SetGuid(string value)
        {
            Guid_TextBlock.Text = value;
        }

        private void SetStatus(string value)
        {
            Status_TextBlock.Text = value;
        }
        #endregion

        #region events

        #region window
        private void Control_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = Cursors.Hand;
        }

        private void Control_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Cursor = Cursors.Arrow;
        }
        #endregion

        #region view
        private void Plan_StatusChanged(object sender, System.EventArgs e)
        {
            SetGradients(plan.Active);
        }
        #endregion

        #endregion
    }
}
