using Microsoft.VisualStudio.TestTools.UnitTesting;

using Multitool.NTInterop;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multitool.NTInterop.Tests
{
    [TestClass()]
    public class PowerPlansTests
    {
        PowerOptions powerPlans = new();

        [TestMethod()]
        public void GetCurrentPowerPlanTest()
        {
            PowerPlan name = powerPlans.GetCurrentPowerPlan();

            Assert.AreEqual("Ultimate Performance", name.Name);
        }

        [TestMethod()]
        public void GetPowerPlansTest()
        {
            List<PowerPlan> plans = powerPlans.GetPowerPlans();
            List<string> names = new List<string>();
            foreach (var item in plans)
            {
                names.Add(item.Name);
            }
            List<string> actualNames = new List<string>()
            {
                "Balanced",
                "Ultimate Performance",
                "High performance",
                "Power saver"
            };
            CollectionAssert.AreEquivalent(actualNames, names);
        }

        [TestMethod()]
        public void SwitchPowerPlanTest()
        {
            List<PowerPlan> plans = powerPlans.GetPowerPlans();

            powerPlans.SwitchPowerPlan(plans[0]);
            //powerPlans.SwitchPowerPlan("Ultimate Performance");
        }
    }
}