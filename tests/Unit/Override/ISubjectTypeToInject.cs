namespace Unit.Test.Override
{
    public interface ISubjectTypeToInject
    {
        int X { get; set; }
        string Y { get; set; }
        IInterfaceForTypesToInject InjectedObject { get; set; }
    }
}