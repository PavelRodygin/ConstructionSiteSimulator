namespace CodeBase.Services.LongInitializationServices
{
    public class SecondLongInitializationService : LongInitializationService
    {
        public SecondLongInitializationService() => DelayTime = 3;
    }
}