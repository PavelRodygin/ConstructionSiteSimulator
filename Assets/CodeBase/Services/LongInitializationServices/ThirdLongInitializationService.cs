namespace CodeBase.Services.LongInitializationServices
{
    public class ThirdLongInitializationService : LongInitializationService
    {
        public ThirdLongInitializationService() => DelayTime = 2;
    }
}