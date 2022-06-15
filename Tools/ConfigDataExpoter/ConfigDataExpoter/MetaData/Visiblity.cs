namespace ConfigDataExpoter
{
    /// <summary>
    /// 可见性
    /// </summary>
    public enum Visiblity
    {
        Invalid = -1,
        None = 0,
        Client = 1 << 0,
        Server = 1 << 1,
        Both = Client | Server
    }
}
