namespace NetCore_PushServer.Enums
{
    public enum PushState
    {
        Error = -1,
        Canceled = -2,
        Expired = -3,

        Ready = 0,
        Sending = 1,

        Complete = 10,
    }
}
