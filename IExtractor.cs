namespace Fin
{
    internal interface IExtractor
    {
        string Name { get; }
        void Run(ControlBuilderSession session);
    }
}
