namespace Unit.Test.Override
{
    public class TypeToInjectForPropertyOverride3 : IInterfaceForTypesToInjectForPropertyOverride
    {
        public TypeToInjectForPropertyOverride3(int value)
        {
            Value = value;
        }
        public int Value { get; set; }
    }
}