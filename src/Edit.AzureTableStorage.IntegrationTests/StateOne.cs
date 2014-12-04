namespace Edit.AzureTableStorage.IntegrationTests
{
    public class StateOne : IState
    {
        public string ValueOne { get; set; }
        public string ValueTwo { get; set; }

        public void Apply(EventOne e)
        {
            ValueOne = e.ValueOne;
        }

        public void Apply(EventTwo e)
        {
            ValueTwo = e.ValueTwo;
        }
    }
}