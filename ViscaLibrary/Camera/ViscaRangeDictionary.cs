using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Visca
{
    public class ViscaRangeDictionary
    {
        public Dictionary<string, Limits> dict = new Dictionary<string, Limits>();

        #region Inner Class
        public class Limits
        {
            public readonly int Low;
            public readonly int High;
            public readonly string Message;
            private readonly string _label;
            private readonly string _name;
            public Limits(string name, int low, int high, string message, string label = null)
            {
                this.Low = low;
                this.High = high;
                this.Message = message;
                this._label = label;
                this._name = name;
            }
            public string Label
            {
                get { return _label != null ? _label : _name; }
            }
        }
        #endregion Inner Class

        #region Get 

        public Limits this[string name]
        {
            get
            {
                return get(name);
            }
        }
        public Limits get(string name)
        {
            Limits value;
            if (dict.TryGetValue(name, out value))
            {
                return value;
            }
            else if (dict.TryGetValue(name + "Limits", out value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"Limit '{name}' not found in ViscaRangeDictionary.");
            }
        }

        public IViscaRangeLimits<int> getInt(string name)
        {
            try
            {
                var value = get(name);
                return new ViscaRangeLimits<int>(value.Low, value.High, value.Message);
            }
            catch (Exception)
            {
                throw new Exception($"Limit '{name}' values not valid for 'int' range limits.");
            }
        }
        public IViscaRangeLimits<byte> getByte(string name)
        {
            try
            {
                var value = get(name);
                return new ViscaRangeLimits<byte>((byte)value.Low, (byte)value.High, value.Message);
            }
            catch (Exception)
            {
                throw new Exception($"Limit '{name}' values not valid for 'int' range limits.");
            }
        }
        #endregion Get

        #region Add Methods
        public void Add(Type type)
        {
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            foreach (var field in fields)
            {
                if (dict.ContainsKey(field.Name))
                {
                    Console.WriteLine("Replacing " + field.Name);
                }
                var value = field.GetValue(null);
                Add(field.Name, value);
            }
        }

        public void Add(string name, object value)
        {
            if (value is IViscaRangeLimits<byte> byteLimits)
                Add(name, byteLimits.Low, byteLimits.High, byteLimits.Message);
            else if (value is IViscaRangeLimits<int> intLimits)
                Add(name, intLimits.Low, intLimits.High, intLimits.Message);
            else
            {
                throw new Exception($"Type of object named '{name}' not Supported.");
            }
        }

        public void Add(string name, int low, int high, string message, string label = null)
        {
            dict[name] = new Limits(name, low, high, message, label);
        }
        public void Add(object obj)
        {
            var type = obj.GetType();

            // Process public instance fields
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (dict.ContainsKey(field.Name))
                {
                    Console.WriteLine("Replacing " + field.Name);
                }
                var value = field.GetValue(obj);
                Add(field.Name, value);
            }

            // Process public instance properties
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                if (dict.ContainsKey(prop.Name))
                {
                    Console.WriteLine("Replacing " + prop.Name);
                }
                var value = prop.GetValue(obj);
                Add(prop.Name, value);
            }
        }
        #endregion
    }
}
