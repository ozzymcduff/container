namespace Unit.Test.Override
{
    public class SubjectType1ToInjectForPropertyOverride : ISubjectTypeToInjectForPropertyOverride
    {
        public int X { get; set; }
        public string Y { get; set; }

        [Dependency]
        public IInterfaceForTypesToInjectForPropertyOverride InjectedObject { get; set; }
    }
}