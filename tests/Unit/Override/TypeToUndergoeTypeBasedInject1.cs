namespace Unit.Test.Override
{
    public class TypeToUndergoeTypeBasedInject1 : IForToUndergoeInject
    {
        public TypeToUndergoeTypeBasedInject1(IForTypeToInject injectedObject)
        {
            IForTypeToInject = injectedObject;
        }

        public IForTypeToInject IForTypeToInject { get; set; }
        public TypeToInject1ForTypeOverride TypeToInject1ForTypeOverride { get; set; }
    }
}