namespace Multicast.Modules.IapValidation {
    using Multicast.UserData;
    using System.Collections.Generic;

    public class UdReceiptData : UdObject {
        public UdListValue<string> Receipts { get; }

        public UdReceiptData(UdArgs args) : base(args) {
            this.Receipts = new UdListValue<string>(this.Child("list_receipts"), new List<string>());
        }
    }
}