using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;

namespace Multitool.NTInterop.Tests
{
    [TestClass()]
    public class PowerPlansTests
    {
        PowerOptions powerOptions = new();

        [TestMethod()]
        public void GetCurrentPowerPlanTest()
        {
            PowerPlan name = powerOptions.GetActivePowerPlan();

            // On desktop
            //Assert.AreEqual("Ultimate Performance", name.Name);

            // On laptop
            Assert.AreEqual("Balanced", name.Name);
        }

        [TestMethod()]
        public void GetPowerPlansTest()
        {
            List<PowerPlan> plans = powerOptions.EnumeratePowerPlans();

            foreach (var item in plans)
            {
                Console.WriteLine(item);
            }

            List<string> names = new();
            foreach (var item in plans)
            {
                names.Add(item.Name);
            }
            List<string> actualNames = new()
            {
                "Balanced",
                "Ultimate Performance",
                //"High performance",
                //"Power saver"
            };
            CollectionAssert.AreEquivalent(actualNames, names);
        }

        [TestMethod()]
        public void SwitchPowerPlanTest()
        {
            List<PowerPlan> plans = powerOptions.EnumeratePowerPlans();
            PowerPlan current = powerOptions.GetActivePowerPlan();

            //powerPlans.SwitchPowerPlan(plans[0]);
            foreach (var plan in plans)
            {
                if (plan.Name != current.Name)
                {
                    powerOptions.SwitchPowerPlan(plan);
                }
            }
        }
    }
}