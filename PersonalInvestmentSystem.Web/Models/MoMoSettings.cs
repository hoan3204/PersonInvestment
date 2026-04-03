namespace PersonalInvestmentSystem.Web.Models
{
    public class MoMoSettings
    {
        public string PartnerCode { get; set; } = string.Empty;
        public string AccessKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string IpnUrl { get; set; } = string.Empty;
        public string RequestType { get; set; } = "payWithMethod";
        public string PartnerName { get; set; } = "MoMo Payment";
        public string StoreId { get; set; } = "Test Store";
        public string Endpoint { get; set; } = string.Empty;
        // Backward compatibility: some configs still use "NotifyUrl"
        public string NotifyUrl { get; set; } = string.Empty;
        public string EffectiveRequestType =>
           string.IsNullOrWhiteSpace(RequestType) ? "payWithMethod" : RequestType;
        public string EffectiveIpnUrl =>
            !string.IsNullOrWhiteSpace(IpnUrl) ? IpnUrl : NotifyUrl;
    }
}
