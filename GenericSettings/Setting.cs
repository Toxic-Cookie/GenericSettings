using FrooxEngine;

namespace NeosTweaks.Settings
{
    class Setting
    {
        public Setting(string name, string description, object value)
        {
            Name = name;
            Description = description;
            Value = value;
        }

        public string Name { get; }
        public string Description { get; }
        public object Value { get; set; }
        public IField LinkedField { get; private set; }

        public void SetWithField(IField syncField)
        {
            LinkedField = syncField;
            Value = syncField.BoxedValue;
        }
        public object GetValue()
        {
            return Value;
        }
    }
}
