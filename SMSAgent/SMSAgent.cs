namespace SMSAgentApi
{
    public class SMSAgent
    {
        public DateTime agentSendTime { get; set; }
        public string? encryptKey { get; set; }
        public string? phoneNumber { get; set; }
        public string? messageContent { get; set; }

    }
}
