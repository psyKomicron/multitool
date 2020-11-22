using System;
using System.Collections.Generic;
using System.Text;

namespace BusinessLayer.PreferencesManagers
{
    public interface IWindowPreferenceManager
    {
        string ItemName { get; set; }
        Dictionary<string, string> Properties { get; set; }

        /// <summary>
        /// Checks if a <see cref="Dictionary{TKey, TValue}"/> has the same keys & values as <see cref="Properties"/>.
        /// </summary>
        /// <param name="data">data to compare to</param>
        /// <returns><see cref="true"/> if the dictionnary has the same values as <see cref="Properties"/></returns>
        bool IsEquivalentTo(Dictionary<string, string> data);
        /// <summary>
        /// Checks if a implementation of <see cref="IWindowPreferenceManager"/> has 
        /// the same stored keys & values in <see cref="Properties"/> as the <paramref name="manager"/>.
        /// </summary>
        /// <param name="manager"></param>
        /// <returns></returns>
        bool IsEquivalentTo(IWindowPreferenceManager manager);
    }
}