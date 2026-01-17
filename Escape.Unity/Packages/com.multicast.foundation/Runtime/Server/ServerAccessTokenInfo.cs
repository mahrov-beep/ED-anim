namespace Multicast.Server {
    using System;
    using System.Globalization;
    using JetBrains.Annotations;

    public readonly struct ServerAccessTokenInfo {
        private readonly string                 raw;
        private readonly ServerAccessTokenState state;
        private readonly DateTime?              expirationDate;
        private readonly Guid?                  userId;
        private readonly string                 environment;

        [PublicAPI]
        public string Raw => this.raw;

        [PublicAPI]
        public ServerAccessTokenState State => this.IsExpiredNow ? ServerAccessTokenState.Expired : this.state;

        [PublicAPI]
        public Guid UserId => this.state is ServerAccessTokenState.Valid or ServerAccessTokenState.Expired
            ? this.userId.GetValueOrDefault()
            : throw new InvalidOperationException($"UserId not valid, token is {this.state}");

        [PublicAPI]
        public string Environment => this.state is ServerAccessTokenState.Valid or ServerAccessTokenState.Expired
            ? this.environment ?? string.Empty
            : throw new InvalidOperationException($"Environment not valid, token is {this.state}");

        [PublicAPI]
        public DateTime ExpirationDate => this.state is ServerAccessTokenState.Valid or ServerAccessTokenState.Expired
            ? this.expirationDate.GetValueOrDefault(DateTime.MinValue)
            : throw new InvalidOperationException($"ExpirationDate not valid, token is {this.state}");

        private bool IsExpiredNow => this.state is ServerAccessTokenState.Valid or ServerAccessTokenState.Expired &&
                                     DateTime.UtcNow > this.ExpirationDate;

        public ServerAccessTokenInfo(string raw, ServerAccessTokenState state, DateTime? expirationDate, Guid? userId, string environment) {
            this.raw            = raw;
            this.state          = state;
            this.expirationDate = expirationDate;
            this.userId         = userId;
            this.environment    = environment;
        }

        public override string ToString() {
            return this.state switch {
                var validState and (ServerAccessTokenState.Valid or ServerAccessTokenState.Expired) =>
                    $"{validState}(UserId={this.userId?.ToString() ?? "Undefined"}" +
                    $", Due={this.expirationDate?.ToString(DateTimeFormatInfo.InvariantInfo.ShortDatePattern) ?? "Undefined"}" +
                    $", Env={this.environment ?? "Undefined"}))",
                ServerAccessTokenState.Malformed => "MALFORMED",
                var otherState => otherState.ToString(),
            };
        }

        [PublicAPI]
        public static ServerAccessTokenInfo ParseFromAccessToken(string accessToken) {
            var state = ServerAccessTokenDecoder.TryDecode(accessToken, out var expirationDate, out var userId, out var environment);
            return new ServerAccessTokenInfo(accessToken, state, expirationDate, userId, environment);
        }
    }
}