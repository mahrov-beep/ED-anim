using Multicast;

namespace Tests.E2E;

public static partial class ServerResultValidation {
    public static bool DataNotNull<T>(this ServerResult<T> serverResult) where T : class, IServerResponse {
        return serverResult is { Data: not null };
    }

    public static void AssertDataIsNull<T>(this ServerResult<T> res) where T : class, IServerResponse {
        Assert.NotNull(res);
        Assert.Null(res.Data);
    }
}