// Objects/FaixasLateraisWrapper.cs
using System;
using System.ComponentModel;

namespace CrimeSketcher.Objects
{
    /// <summary>
    /// Adaptador para o PropertyGrid que expõe uma propriedade expandível
    /// com uma entrada por divisória lateral, com base no número de faixas atual.
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class FaixasLateraisWrapper : ICustomTypeDescriptor
    {
        private readonly StreetObject _rua;

        internal FaixasLateraisWrapper(StreetObject rua) => _rua = rua;

        internal StreetObject Rua => _rua;

        /// <summary>
        /// Garante que a lista tenha pelo menos o número necessário de configs.
        /// Nunca remove entradas existentes para preservar configurações anteriores.
        /// </summary>
        internal void EnsureSize()
        {
            int n = Math.Max(0, _rua.NumeroFaixas - 2);
            while (_rua.FaixasLateraisConfig.Count < n)
                _rua.FaixasLateraisConfig.Add(new ConfigFaixaLateral());
        }

        public override string ToString()
        {
            int n = Math.Max(0, _rua.NumeroFaixas - 2);
            return n == 0 ? "(sem faixas laterais)" : $"{n} divisória(s)";
        }

        // ── ICustomTypeDescriptor ─────────────────────────────────────────────

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
            => TypeDescriptor.GetAttributes(this, true);

        string ICustomTypeDescriptor.GetClassName()
            => TypeDescriptor.GetClassName(this, true);

        string ICustomTypeDescriptor.GetComponentName()
            => TypeDescriptor.GetComponentName(this, true);

        TypeConverter ICustomTypeDescriptor.GetConverter()
            => TypeDescriptor.GetConverter(this, true);

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
            => TypeDescriptor.GetDefaultEvent(this, true);

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
            => TypeDescriptor.GetDefaultProperty(this, true);

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
            => TypeDescriptor.GetEditor(this, editorBaseType, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
            => TypeDescriptor.GetEvents(this, true);

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
            => TypeDescriptor.GetEvents(this, attributes, true);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
            => ((ICustomTypeDescriptor)this).GetProperties(null);

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            EnsureSize();
            int n = Math.Max(0, _rua.NumeroFaixas - 2);
            var props = new PropertyDescriptorCollection(null);
            for (int i = 0; i < n; i++)
                props.Add(new FaixaLateralPropertyDescriptor(i));
            return props;
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) => this;
    }

    internal sealed class FaixaLateralPropertyDescriptor : PropertyDescriptor
    {
        private readonly int _index;

        public FaixaLateralPropertyDescriptor(int index)
            : base($"Divisória {index + 1}", new Attribute[]
            {
                new CategoryAttribute("Faixas Laterais"),
                new DescriptionAttribute($"Tipo e cor da divisória lateral {index + 1} (da esquerda para a direita, excluindo a linha central)"),
                new TypeConverterAttribute(typeof(ExpandableObjectConverter))
            })
        {
            _index = index;
        }

        public override Type ComponentType => typeof(FaixasLateraisWrapper);
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(ConfigFaixaLateral);

        public override bool CanResetValue(object component) => true;

        public override object GetValue(object component)
        {
            var w = (FaixasLateraisWrapper)component;
            w.EnsureSize();
            return w.Rua.FaixasLateraisConfig[_index];
        }

        public override void ResetValue(object component)
        {
            var w = (FaixasLateraisWrapper)component;
            w.EnsureSize();
            w.Rua.FaixasLateraisConfig[_index] = new ConfigFaixaLateral();
        }

        public override void SetValue(object component, object value)
        {
            var w = (FaixasLateraisWrapper)component;
            w.EnsureSize();
            if (value is ConfigFaixaLateral cfg)
                w.Rua.FaixasLateraisConfig[_index] = cfg;
        }

        public override bool ShouldSerializeValue(object component) => false;
    }
}
