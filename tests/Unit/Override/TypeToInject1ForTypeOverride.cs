namespace Unit.Test.Override
{
    public class TypeToInject1ForTypeOverride : IForTypeToInject
    {
        public TypeToInject1ForTypeOverride(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}