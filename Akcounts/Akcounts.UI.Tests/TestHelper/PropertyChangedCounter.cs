using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Akcounts.UI.Tests.TestHelper
{
    public class PropertyChangedCounter
    {
        private readonly IDictionary<string, int> _propertiesChanged = new Dictionary<string, int>();

        public void HandlePropertyChange (object sender, PropertyChangedEventArgs args)
        {
            if (_propertiesChanged.ContainsKey(args.PropertyName))
                _propertiesChanged[args.PropertyName]++;
            else
                _propertiesChanged.Add(args.PropertyName, 1);
        }

        public int ChangeCount(string propertyName)
        {
            return _propertiesChanged.ContainsKey(propertyName) ? _propertiesChanged[propertyName] : 0;
        }

        public int TotalChangeCount
        {
            get
            {
                return _propertiesChanged.Values.Sum();
            }
        }

        public int NoOfPropertiesChanged
        {
            get { return _propertiesChanged.Count; }
        }
    }
}
