using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Visca
{
    /// <summary>
    /// Represents a dictionary for storing and retrieving range limits for VISCA camera parameters.
    /// Provides methods to add limits from types, objects, or directly, and to retrieve limits as <see cref="Limits"/>, <see cref="IViscaRangeLimits{int}"/>, or <see cref="IViscaRangeLimits{byte}"/>.
    /// </summary>
    public class ViscaRangeDictionary
    {
        /// <summary>
        /// Internal dictionary mapping property names to their limits.
        /// </summary>
        public Dictionary<string, Limits> dict = new Dictionary<string, Limits>();

        #region Inner Class
        /// <summary>
        /// Represents the range limits for a VISCA parameter.
        /// </summary>
        public class Limits
        {
            /// <summary>
            /// The lower bound of the range.
            /// </summary>
            public readonly int Low;
            /// <summary>
            /// The upper bound of the range.
            /// </summary>
            public readonly int High;
            /// <summary>
            /// The message describing the range.
            /// </summary>
            public readonly string Message;
            private readonly string _label;
            private readonly string _propertyName;

            /// <summary>
            /// Initializes a new instance of the <see cref="Limits"/> class.
            /// </summary>
            /// <param name="propertyName">The name of the property.</param>
            /// <param name="low">The lower bound.</param>
            /// <param name="high">The upper bound.</param>
            /// <param name="message">The message describing the range.</param>
            /// <param name="label">An optional label for the property.</param>
            public Limits(string propertyName, int low, int high, string message, string label = null)
            {
                Low = low;
                High = high;
                Message = message;
                _label = label;
                _propertyName = propertyName;
            }

            /// <summary>
            /// Get the label for the property, or the property name if no label is set.
            /// </summary>
            public string Label
            {
                get { return _label != null ? _label : _propertyName; }
            }
        }
        #endregion Inner Class

        #region Get 

        /// <summary>
        /// Get the <see cref="Limits"/> for the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The <see cref="Limits"/> object.</returns>
        public Limits this[string propertyName]
        {
            get
            {
                return get(propertyName);
            }
        }

        /// <summary>
        /// Retrieve the <see cref="Limits"/> for the given name, or name with "Limits" suffix.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The <see cref="Limits"/> object.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the limit is not found.</exception>
        public Limits get(string propertyName)
        {
            Limits value;
            if (dict.TryGetValue(propertyName, out value))
            {
                return value;
            }
            else if (dict.TryGetValue(propertyName + "Limits", out value))
            {
                return value;
            }
            else
            {
                throw new KeyNotFoundException($"Limit '{propertyName}' not found in ViscaRangeDictionary.");
            }
        }

        /// <summary>
        /// Get the range limits as <see cref="IViscaRangeLimits{int}"/> for the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The range limits as <see cref="IViscaRangeLimits{int}"/>.</returns>
        /// <exception cref="Exception">Thrown if the values are not valid for int range limits.</exception>
        public IViscaRangeLimits<int> getInt(string propertyName)
        {
            try
            {
                var value = get(propertyName);
                return new ViscaRangeLimits<int>(value.Low, value.High, value.Message);
            }
            catch (Exception)
            {
                throw new Exception($"Limit '{propertyName}' values not valid for 'int' range limits.");
            }
        }

        /// <summary>
        /// Get the range limits as <see cref="IViscaRangeLimits{byte}"/> for the specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <returns>The range limits as <see cref="IViscaRangeLimits{byte}"/>.</returns>
        /// <exception cref="Exception">Thrown if the values are not valid for byte range limits.</exception>
        public IViscaRangeLimits<byte> getByte(string propertyName)
        {
            try
            {
                var value = get(propertyName);
                if (byte.MaxValue < value.High || byte.MaxValue < value.Low
                   || byte.MinValue > value.High || byte.MinValue > value.Low)
                {
                    throw new Exception($"Limit '{propertyName}' values not valid for 'byte' range limits.");
                }
                return new ViscaRangeLimits<byte>((byte)value.Low, (byte)value.High, value.Message);
            }
            catch (Exception)
            {
                throw new Exception($"Unable to use Limit '{propertyName}' as ViscaRangeLimits<byte>.");
            }
        }
        #endregion Get

        #region Add Methods
        /// <summary>
        /// Add limits from all public static fields of the specified type.
        /// </summary>
        /// <param name="type">The type containing static fields with limits.</param>
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

        /// <summary>
        /// Add a limit by name and value, supporting <see cref="IViscaRangeLimits{byte}"/> and <see cref="IViscaRangeLimits{int}"/>.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="value">The limit value.</param>
        /// <exception cref="Exception">Thrown if the value type is not supported.</exception>
        public void Add(string propertyName, object value)
        {
            if (value is IViscaRangeLimits<byte> byteLimits)
                Add(propertyName, byteLimits.Low, byteLimits.High, byteLimits.Message);
            else if (value is IViscaRangeLimits<int> intLimits)
                Add(propertyName, intLimits.Low, intLimits.High, intLimits.Message);
            else
            {
                throw new Exception($"Type of object named '{propertyName}' not Supported.");
            }
        }

        /// <summary>
        /// Add a limit by specifying all properties.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="low">The lower bound.</param>
        /// <param name="high">The upper bound.</param>
        /// <param name="message">The message describing the range.</param>
        /// <param name="label">An optional label for the property.</param>
        public void Add(string propertyName, int low, int high, string message, string label = null)
        {
            dict[propertyName] = new Limits(propertyName, low, high, message, label);
        }

        /// <summary>
        /// Add limits from all public instance fields and properties of the specified object.
        /// </summary>
        /// <param name="obj">The object containing fields and properties with limits.</param>
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
